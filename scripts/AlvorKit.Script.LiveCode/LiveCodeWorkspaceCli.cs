namespace AlvorKit.Script.LiveCode;

/// <summary>Creates and audits persistent local records around one exact running LiveCode process.</summary>
internal sealed class LiveCodeWorkspaceCli(
    TextWriter output,
    string repositoryRoot)
{
    private readonly LiveWorkspaceStore store = new(repositoryRoot);
    private readonly JsonSerializerOptions json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    internal async Task<int> Init(
        string id,
        string purpose,
        string sessionSelector,
        string? alvorSenseSessionId,
        string? discoveryDirectory)
    {
        var session = new LiveCodeDiscovery(discoveryDirectory).Find(sessionSelector);
        var client = new LiveCodeClient(session);
        var graph = await client.Graph();
        var bridges = await client.Bridges();
        var manifest = store.Create(
            id,
            purpose,
            new(session.SessionId, session.Name, session.ProcessId, session.StartedUtc),
            alvorSenseSessionId,
            graph.Revision);
        store.WriteBaseline(manifest, "graph.json", graph);
        store.WriteBaseline(manifest, "bridges.json", bridges);

        string? capabilitiesPath = null;
        if (bridges.Any(bridge => bridge.Name == "livepatch"))
        {
            var payload = JsonSerializer.SerializeToElement(new { operation = "capabilities" }, json);
            var capabilities = await client.Bridge("livepatch", payload, version: 1);
            capabilitiesPath = store.WriteBaseline(
                manifest,
                "livepatch-capabilities.json",
                capabilities);
        }

        Write(new
        {
            manifest.Id,
            manifest.WorkspacePath,
            manifest.LiveCode,
            manifest.AlvorSenseSessionId,
            manifest.BaselineGraphRevision,
            graphPath = Path.Combine(manifest.WorkspacePath, "baseline", "graph.json"),
            bridgesPath = Path.Combine(manifest.WorkspacePath, "baseline", "bridges.json"),
            capabilitiesPath
        });
        return 0;
    }

    internal Task<int> Status(
        string workspace,
        string? discoveryDirectory)
    {
        var manifest = store.Read(workspace);
        LiveCodeSessionManifest? live = null;
        string? error = null;
        try
        {
            live = new LiveCodeDiscovery(discoveryDirectory).Find(manifest.LiveCode.SessionId);
            if (live.ProcessId != manifest.LiveCode.ProcessId ||
                live.StartedUtc != manifest.LiveCode.StartedUtc)
            {
                error = "The discovered session id no longer has the recorded process identity.";
                live = null;
            }
        }
        catch (InvalidOperationException exception)
        {
            error = exception.Message;
        }

        Write(new
        {
            manifest,
            liveProcessVerified = live is not null,
            liveProcessError = error,
            unresolvedInterventions = manifest.Interventions.Where(
                intervention => intervention.State != LiveWorkspaceInterventionState.Resolved)
        });
        return Task.FromResult(live is null ? 2 : 0);
    }

    internal Task<int> AssociateSense(string workspace, string sessionId)
    {
        var manifest = store.AssociateAlvorSense(workspace, sessionId);
        Write(new
        {
            manifest.Id,
            manifest.AlvorSenseSessionId,
            manifest.UpdatedUtc
        });
        return Task.FromResult(0);
    }

    internal Task<int> AddIntervention(
        string workspace,
        string id,
        string kind,
        string description,
        string state,
        string? runtimeId,
        string? source,
        string? cleanup)
    {
        var intervention = new LiveWorkspaceIntervention(
            id,
            ParseKind(kind),
            description,
            ParseState(state),
            runtimeId,
            source is null ? null : Path.GetFullPath(source),
            cleanup);
        var manifest = store.UpsertIntervention(workspace, intervention);
        Write(new
        {
            manifest.Id,
            intervention,
            manifest.UpdatedUtc
        });
        return Task.FromResult(0);
    }

    internal Task<int> ResolveIntervention(string workspace, string id)
    {
        var manifest = store.ResolveIntervention(workspace, id);
        Write(new
        {
            manifest.Id,
            intervention = manifest.Interventions.Single(item => item.Id == id),
            manifest.UpdatedUtc
        });
        return Task.FromResult(0);
    }

    internal Task<int> Close(string workspace)
    {
        var manifest = store.Close(workspace);
        Write(new
        {
            manifest.Id,
            manifest.Status,
            manifest.UpdatedUtc,
            unresolvedInterventions = Array.Empty<object>()
        });
        return Task.FromResult(0);
    }

    private static LiveWorkspaceInterventionKind ParseKind(string value) =>
        value.ToLowerInvariant() switch
        {
            "livecode" => LiveWorkspaceInterventionKind.LiveCode,
            "livepatch" => LiveWorkspaceInterventionKind.LivePatch,
            "bridge" => LiveWorkspaceInterventionKind.Bridge,
            _ => throw new ArgumentException(
                $"Unknown intervention kind '{value}'. Use livecode, livepatch, or bridge.")
        };

    private static LiveWorkspaceInterventionState ParseState(string value) =>
        value.ToLowerInvariant() switch
        {
            "active" => LiveWorkspaceInterventionState.Active,
            "restart-required" => LiveWorkspaceInterventionState.RestartRequired,
            _ => throw new ArgumentException(
                $"Unknown intervention state '{value}'. Use active or restart-required.")
        };

    private void Write<T>(T value) =>
        output.WriteLine(JsonSerializer.Serialize(value, json));
}
