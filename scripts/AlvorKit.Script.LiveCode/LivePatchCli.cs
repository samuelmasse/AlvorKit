namespace AlvorKit.Script.LiveCode;

/// <summary>Compiles, submits, records, and controls exact LivePatch handlers.</summary>
internal sealed class LivePatchCli(LiveCodeCliContext context)
{
    /// <summary>Reads the target's negotiated LivePatch capabilities.</summary>
    internal Task<int> PatchCapabilities(string session, string? discovery, string? workspace) =>
        PatchSimple(session, "capabilities", null, discovery, workspace);

    /// <summary>Lists active and retained terminal patches.</summary>
    internal Task<int> PatchList(
        string session,
        string? discovery,
        string? workspace) =>
        PatchSimple(session, "list", null, discovery, workspace);

    /// <summary>Reads one patch by stable ID.</summary>
    internal Task<int> PatchStatus(
        string session,
        ulong patchId,
        string? discovery,
        string? workspace) =>
        PatchSimple(session, "status", patchId, discovery, workspace);

    /// <summary>Stops dispatch and requests native restoration for one patch.</summary>
    internal Task<int> PatchRemove(
        string session,
        ulong patchId,
        string? discovery,
        string? workspace) =>
        PatchSimple(session, "remove", patchId, discovery, workspace);

    /// <summary>Compiles and installs a new exact-signature handler.</summary>
    internal async Task<int> PatchInstall(
        string sessionSelector,
        string scopeSelector,
        string selectorKind,
        string target,
        string? targetAssembly,
        string? name,
        string? file,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = new LiveCodeDiscovery(discoveryDirectory).Find(sessionSelector);
        context.VerifyWorkspaceTarget(workspace, session);
        var client = new LiveCodeClient(session);
        var scope = LiveCodeCliContext.SelectScope(await client.Graph(), scopeSelector);
        var sourceIdentity = context.WorkspaceSource(workspace, file, "lp");
        var source = sourceIdentity is null
            ? await context.ReadSource(file)
            : await File.ReadAllTextAsync(sourceIdentity.Path);
        var compilation = new LiveCodeCompiler().CompilePatch(
            source,
            await client.References());
        var (targetType, targetMethod) = ParseTarget(target);
        var kind = NormalizeSelector(selectorKind);
        var payload = JsonSerializer.SerializeToElement(new
        {
            operation = "install",
            executorScopeId = scope.Id,
            selector = new
            {
                kind,
                scopeId = kind == "all" ? (long?)null : scope.Id
            },
            target = new
            {
                assembly = targetAssembly,
                type = targetType,
                method = targetMethod
            },
            compilation.EntryType,
            assembly = compilation.Assembly,
            symbols = compilation.Symbols,
            name
        }, context.Json);
        var result = await client.Bridge("livepatch", payload, version: 1);
        var cliResult = LiveCodeCliContext.BridgeResult(
            result,
            await context.SaveArtifacts(result.Artifacts));
        context.WriteRecorded(
            workspace,
            "livepatch-install",
            new
            {
                sessionId = session.SessionId,
                executorScopeId = scope.Id,
                selector = kind,
                target,
                targetAssembly,
                name,
                source = sourceIdentity
            },
            cliResult);
        TrackPatch(workspace, cliResult, sourceIdentity, name, target);
        return result.Status == LiveCodeBridgeExecutionStatus.Completed ? 0 : 2;
    }

    /// <summary>Compiles and atomically publishes a replacement handler.</summary>
    internal async Task<int> PatchReplace(
        string sessionSelector,
        ulong patchId,
        string? scopeSelector,
        string? file,
        string? discoveryDirectory,
        string? workspace)
    {
        var session = new LiveCodeDiscovery(discoveryDirectory).Find(sessionSelector);
        context.VerifyWorkspaceTarget(workspace, session);
        var client = new LiveCodeClient(session);
        var executorScopeId = string.IsNullOrWhiteSpace(scopeSelector)
            ? null
            : (long?)LiveCodeCliContext.SelectScope(
                await client.Graph(),
                scopeSelector).Id;
        var sourceIdentity = context.WorkspaceSource(workspace, file, "lp");
        var source = sourceIdentity is null
            ? await context.ReadSource(file)
            : await File.ReadAllTextAsync(sourceIdentity.Path);
        var compilation = new LiveCodeCompiler().CompilePatch(
            source,
            await client.References());
        var payload = JsonSerializer.SerializeToElement(new
        {
            operation = "replace",
            patchId,
            executorScopeId,
            compilation.EntryType,
            assembly = compilation.Assembly,
            symbols = compilation.Symbols
        }, context.Json);
        var result = await client.Bridge("livepatch", payload, version: 1);
        var cliResult = LiveCodeCliContext.BridgeResult(
            result,
            await context.SaveArtifacts(result.Artifacts));
        context.WriteRecorded(
            workspace,
            "livepatch-replace",
            new
            {
                sessionId = session.SessionId,
                patchId,
                executorScopeId,
                source = sourceIdentity
            },
            cliResult);
        TrackPatch(workspace, cliResult, sourceIdentity, null, null);
        return result.Status == LiveCodeBridgeExecutionStatus.Completed ? 0 : 2;
    }

    private async Task<int> PatchSimple(
        string sessionSelector,
        string operation,
        ulong? patchId,
        string? discoveryDirectory,
        string? workspace)
    {
        var payload = JsonSerializer.SerializeToElement(
            new { operation, patchId },
            context.Json);
        var session = new LiveCodeDiscovery(discoveryDirectory).Find(sessionSelector);
        context.VerifyWorkspaceTarget(workspace, session);
        var result = await new LiveCodeClient(session).Bridge(
            "livepatch",
            payload,
            version: 1);
        var cliResult = LiveCodeCliContext.BridgeResult(
            result,
            await context.SaveArtifacts(result.Artifacts));
        context.WriteRecorded(
            workspace,
            $"livepatch-{operation}",
            new { sessionId = session.SessionId, patchId },
            cliResult);
        TrackPatch(workspace, cliResult, null, null, null);
        return result.Status == LiveCodeBridgeExecutionStatus.Completed ? 0 : 2;
    }

    private void TrackPatch(
        string? workspace,
        LiveCodeBridgeCliResult result,
        LiveWorkspaceSource? source,
        string? name,
        string? target)
    {
        if (workspace is null ||
            result.Status != LiveCodeBridgeExecutionStatus.Completed ||
            !result.Values.TryGetValue("patch", out var patch) ||
            !patch.TryGetProperty("patchId", out var patchIdElement) ||
            !patchIdElement.TryGetUInt64(out var patchId))
        {
            return;
        }

        var manifest = context.Workspaces.Read(workspace);
        var id = $"livepatch-{patchId}";
        var existing = manifest.Interventions.SingleOrDefault(item => item.Id == id);
        var patchState = patch.TryGetProperty("state", out var stateElement)
            ? stateElement.GetString()
            : null;
        var nativeState = patch.TryGetProperty("nativeState", out var nativeStateElement)
            ? nativeStateElement.GetString()
            : null;
        var state = patchState switch
        {
            "removed" => LiveWorkspaceInterventionState.Resolved,
            "failed" when nativeState == "removed" => LiveWorkspaceInterventionState.Resolved,
            "removing" => LiveWorkspaceInterventionState.Removing,
            _ => LiveWorkspaceInterventionState.Active
        };
        context.Workspaces.UpsertIntervention(
            workspace,
            new(
                id,
                LiveWorkspaceInterventionKind.LivePatch,
                name ?? existing?.Description ?? target ?? $"LivePatch {patchId}",
                state,
                patchId.ToString(CultureInfo.InvariantCulture),
                source?.Path ?? existing?.SourcePath,
                existing?.Cleanup ??
                    $"patch remove --session {manifest.LiveCode.SessionId} --patch {patchId}"));
    }

    private static (string Type, string Method) ParseTarget(string target)
    {
        var separator = target.LastIndexOf("::", StringComparison.Ordinal);
        if (separator <= 0 || separator == target.Length - 2)
        {
            throw new ArgumentException(
                "LivePatch target must use the exact 'Namespace.Type::Method' form.",
                nameof(target));
        }

        return (target[..separator], target[(separator + 2)..]);
    }

    private static string NormalizeSelector(string selector) =>
        selector switch
        {
            "exact-instance" => "exactInstance",
            "exact-scope" => "exactScope",
            "descendants" => "descendants",
            "all" => "all",
            _ => throw new ArgumentException(
                $"Unknown selector '{selector}'. Use exact-instance, exact-scope, descendants, or all.")
        };
}
