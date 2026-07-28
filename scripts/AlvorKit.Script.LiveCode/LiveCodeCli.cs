namespace AlvorKit.Script.LiveCode;

/// <summary>Coordinates discovery, compilation, scope selection, and result presentation.</summary>
internal sealed class LiveCodeCli(LiveCodeCliContext context)
{
    /// <summary>Lists running development sessions.</summary>
    internal Task<int> List(string? discoveryDirectory)
    {
        var sessions = new LiveCodeDiscovery(discoveryDirectory)
            .List()
            .Select(static session => new
            {
                session.ProtocolVersion,
                session.SessionId,
                session.Name,
                session.ProcessId,
                session.Port,
                session.StartedUtc,
                session.FrozenInspectionEnabled
            });
        context.Write(sessions);
        return Task.FromResult(0);
    }

    /// <summary>Reads the active injector scope graph.</summary>
    internal async Task<int> Graph(
        string sessionSelector,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var graph = await new LiveCodeClient(session).Graph();
        context.WriteRecorded(
            workspace,
            "livecode-graph",
            new { sessionId = session.SessionId },
            graph);
        return 0;
    }

    /// <summary>Lists the target's predefined bridge contracts.</summary>
    internal async Task<int> Bridges(
        string sessionSelector,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var bridges = await new LiveCodeClient(session).Bridges();
        context.WriteRecorded(
            workspace,
            "livecode-bridges",
            new { sessionId = session.SessionId },
            bridges);
        return 0;
    }

    /// <summary>Invokes one predefined JSON bridge.</summary>
    internal async Task<int> Bridge(
        string sessionSelector,
        string name,
        int version,
        string? file,
        string? discoveryDirectory,
        string? workspace)
    {
        var source = context.NormalizeRedirectedText(await context.ReadSource(file));
        var payload = string.IsNullOrWhiteSpace(source)
            ? JsonSerializer.SerializeToElement(new { }, context.Json)
            : JsonDocument.Parse(source).RootElement.Clone();
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var result = await new LiveCodeClient(session).Bridge(name, payload, version);
        var cliResult = LiveCodeCliContext.BridgeResult(
            result,
            await context.SaveArtifacts(result.Artifacts));
        context.WriteRecorded(
            workspace,
            "livecode-bridge",
            new
            {
                sessionId = session.SessionId,
                name,
                version,
                payload,
                payloadFile = file is null ? null : Path.GetFullPath(file)
            },
            cliResult);
        return result.Status == LiveCodeBridgeExecutionStatus.Completed ? 0 : 2;
    }

    /// <summary>Runs an atomic AlvorSense command batch through its predefined bridge.</summary>
    internal async Task<int> Puppet(
        string sessionSelector,
        string? file,
        string? discoveryDirectory,
        string? workspace)
    {
        var source = context.NormalizeRedirectedText(await context.ReadSource(file));
        var commands = source.Split(
            ["\r\n", "\n", "\r"],
            StringSplitOptions.None);
        var payload = JsonSerializer.SerializeToElement(new { commands }, context.Json);
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var result = await new LiveCodeClient(session).Bridge(
            "alvorsense",
            payload,
            version: 1);
        var cliResult = LiveCodeCliContext.BridgeResult(
            result,
            await context.SaveArtifacts(result.Artifacts));
        context.WriteRecorded(
            workspace,
            "livecode-puppet",
            new
            {
                sessionId = session.SessionId,
                commands,
                commandFile = file is null ? null : Path.GetFullPath(file)
            },
            cliResult);
        return result.Status == LiveCodeBridgeExecutionStatus.Completed ? 0 : 2;
    }

    /// <summary>Compiles and executes an ordinary command in one active scope.</summary>
    internal async Task<int> Execute(
        string sessionSelector,
        string scopeSelector,
        string? file,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var client = new LiveCodeClient(session);
        var scope = LiveCodeCliContext.SelectScope(
            await client.Graph(),
            scopeSelector);
        var sourceIdentity = context.WorkspaceSource(workspace, file, "lc");
        var source = sourceIdentity is not null
            ? await File.ReadAllTextAsync(sourceIdentity.Path)
            : await context.ReadSource(file);
        var compilation = new LiveCodeCompiler().Compile(
            source,
            await client.References());
        var result = await client.Execute(
            scope.Id,
            compilation.EntryType,
            compilation.Assembly,
            compilation.Symbols);
        context.WriteRecorded(
            workspace,
            "livecode-exec",
            new
            {
                sessionId = session.SessionId,
                scopeId = scope.Id,
                source = sourceIdentity
            },
            result);
        return result.Status == LiveCodeExecutionStatus.Completed ? 0 : 2;
    }

    /// <summary>Reads frozen-inspection state without entering the game loop.</summary>
    internal async Task<int> FrozenStatus(
        string sessionSelector,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var status = await new LiveCodeClient(session).FrozenInspectionStatus();
        context.WriteRecorded(
            workspace,
            "livecode-frozen-status",
            new { sessionId = session.SessionId },
            status);
        return status.Enabled ? 0 : 2;
    }

    /// <summary>Executes a command through the out-of-band frozen-inspection lane.</summary>
    internal async Task<int> FrozenExecute(
        string sessionSelector,
        string scopeSelector,
        string? file,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = Find(sessionSelector, discoveryDirectory, workspace);
        var client = new LiveCodeClient(session);
        var scope = LiveCodeCliContext.SelectScope(
            await client.Graph(),
            scopeSelector);
        var sourceIdentity = context.WorkspaceSource(workspace, file, "lc");
        var source = sourceIdentity is null
            ? await context.ReadSource(file)
            : await File.ReadAllTextAsync(sourceIdentity.Path);
        var compilation = new LiveCodeCompiler().Compile(
            source,
            await client.References());
        var result = await client.ExecuteFrozen(
            scope.Id,
            compilation.EntryType,
            compilation.Assembly,
            compilation.Symbols);
        context.WriteRecorded(
            workspace,
            "livecode-frozen-exec",
            new
            {
                sessionId = session.SessionId,
                scopeId = scope.Id,
                source = sourceIdentity
            },
            result);
        return result.Execution.Status == LiveCodeExecutionStatus.Completed ? 0 : 2;
    }

    private LiveCodeSessionManifest Find(
        string selector,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = new LiveCodeDiscovery(discoveryDirectory).Find(selector);
        context.VerifyWorkspaceTarget(workspace, session);
        return session;
    }
}
