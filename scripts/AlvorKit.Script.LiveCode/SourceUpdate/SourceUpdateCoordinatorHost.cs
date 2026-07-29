namespace AlvorKit.Script.LiveCode;

/// <summary>Retains the exact compiler baseline and acknowledges only target-confirmed generations.</summary>
[ExcludeFromCodeCoverage(Justification = "Long-lived local process integration is covered by CLI integration tests.")]
internal sealed class SourceUpdateCoordinatorHost(
    string workspacePath,
    string launchManifestPath,
    string sessionSelector,
    string? discoveryDirectory)
{
    private readonly CancellationTokenSource lifetime = new();
    private readonly SemaphoreSlim generationGate = new(1, 1);
    private readonly Lock statusGate = new();
    private SourceUpdateCoordinatorResponse status = new(true, "starting", 0);
    private SourceUpdateCoordinatorManifest? manifest;
    private SourceUpdateProjectBaseline? baseline;
    private LiveCodeClient? client;

    internal async Task<int> Run()
    {
        try
        {
            var workspace = Workspace();
            var launch = SourceUpdateCoordinatorJson.ReadFile<SourceUpdateCompilerLaunch>(
                launchManifestPath);
            var session = new LiveCodeDiscovery(discoveryDirectory).Find(sessionSelector);
            VerifyTarget(workspace, session);
            client = new(session);
            var bridges = await client.Bridges(lifetime.Token);
            if (!bridges.Any(static bridge => bridge.Name == "source-update" && bridge.Version == 1))
                throw new InvalidOperationException("The target does not advertise Source Update bridge version 1.");

            baseline = await SourceUpdateProjectBaseline.Create(launch, lifetime.Token);
            status = new(true, "ready", baseline.Generation);
            manifest = new(
                1,
                workspacePath,
                launchManifestPath,
                Manifest().PipeName,
                Environment.ProcessId,
                DateTimeOffset.UtcNow,
                true,
                baseline.Generation,
                "ready",
                null,
                null);
            SaveStatus();

            var handlers = new List<Task>();
            while (!lifetime.IsCancellationRequested)
            {
                var pipe = new NamedPipeServerStream(
                    manifest.PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                try
                {
                    await pipe.WaitForConnectionAsync(lifetime.Token);
                    handlers.Add(Handle(pipe));
                    handlers.RemoveAll(static task => task.IsCompleted);
                }
                catch
                {
                    await pipe.DisposeAsync();
                    throw;
                }
            }
            await Task.WhenAll(handlers);
            return 0;
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            return 0;
        }
        catch (Exception exception)
        {
            FailStartup(exception.Message);
            return 1;
        }
        finally
        {
            if (manifest is not null && Status().State == "stopping")
                SetStatus(new(true, "stopped", baseline?.Generation ?? Status().Generation));
            baseline?.Dispose();
            generationGate.Dispose();
            lifetime.Dispose();
        }
    }

    private async Task Handle(NamedPipeServerStream pipe)
    {
        await using (pipe)
        {
            SourceUpdateCoordinatorResponse response;
            try
            {
                var request = await SourceUpdateCoordinatorJson.Read<SourceUpdateCoordinatorRequest>(
                    pipe,
                    lifetime.Token);
                response = request.Operation switch
                {
                    "apply" => await BeginApply(request),
                    "status" => Status(),
                    "stop" => Stop(),
                    _ => new(false, "rejected", Status().Generation, Error: "Unknown coordinator operation.")
                };
            }
            catch (Exception exception)
            {
                response = new(false, "failed", Status().Generation, Error: exception.Message);
            }
            await SourceUpdateCoordinatorJson.Write(pipe, response, CancellationToken.None);
        }
    }

    private async Task<SourceUpdateCoordinatorResponse> BeginApply(
        SourceUpdateCoordinatorRequest request)
    {
        if (!await generationGate.WaitAsync(0, lifetime.Token))
            return new(false, "busy", Status().Generation, Error: "A Source Update is already pending.");

        try
        {
            var workspace = Workspace();
            var sourcePath = Path.GetFullPath(
                request.SourcePath ?? throw new InvalidDataException("Source path is required."));
            var diffPath = Path.GetFullPath(
                request.DiffPath ?? throw new InvalidDataException("Diff path is required."));
            RequireWithin(workspace.RepositoryRoot, sourcePath, "source file");
            RequireWithin(
                Path.Combine(workspace.WorkspacePath, "source", "diffs"),
                diffPath,
                "immutable diff");
            var updateId = request.UpdateId
                ?? throw new InvalidDataException("Update id is required.");
            if (updateId.Length is < 1 or > 96 ||
                updateId.Any(static character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
            {
                throw new InvalidDataException(
                    "Update id must contain 1 to 96 letters, digits, hyphens, or underscores.");
            }

            var source = await File.ReadAllTextAsync(sourcePath, lifetime.Token);
            var diff = await File.ReadAllTextAsync(diffPath, lifetime.Token);
            var proposal = await baseline!.Prepare(
                sourcePath,
                diff,
                source,
                workspace.RepositoryRoot,
                updateId,
                lifetime.Token);
            if (!string.Equals(
                    source,
                    await File.ReadAllTextAsync(sourcePath, lifetime.Token),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Source file changed while the delta was being compiled.");
            }

            var operationId = "source-update-" + updateId;
            var payload = JsonSerializer.SerializeToElement(
                new SourceUpdateBridgeRequest("apply", proposal.Request),
                SourceUpdateCoordinatorJson.Options);
            var queued = await client!.EnqueueBridge(
                operationId,
                "source-update",
                payload,
                version: 1,
                lifetime.Token);
            SetStatus(new(
                true,
                "queued",
                baseline.Generation,
                queued.OperationId));
            _ = Monitor(proposal, diffPath, queued.OperationId);
            return Status();
        }
        catch
        {
            generationGate.Release();
            throw;
        }
    }

    private async Task Monitor(
        SourceUpdateDeltaProposal proposal,
        string diffPath,
        string operationId)
    {
        try
        {
            while (!lifetime.IsCancellationRequested)
            {
                var operation = await client!.BridgeOperationStatus(operationId, lifetime.Token);
                if (operation.State != LiveCodeBridgeOperationState.Completed)
                {
                    SetStatus(new(true, operation.State.ToString().ToLowerInvariant(), baseline!.Generation, operationId));
                    await Task.Delay(25, lifetime.Token);
                    continue;
                }

                var bridge = operation.Result
                    ?? throw new InvalidOperationException("Completed Source Update operation returned no result.");
                if (bridge.Status != LiveCodeBridgeExecutionStatus.Completed)
                {
                    throw new InvalidOperationException(
                        bridge.Error ?? "Source Update bridge execution failed.");
                }
                if (!bridge.Values.TryGetValue("response", out var value))
                    throw new InvalidOperationException("Source Update bridge returned no response value.");
                var response = value.Deserialize<SourceUpdateBridgeResponse>(
                    SourceUpdateCoordinatorJson.Options)
                    ?? throw new InvalidOperationException("Source Update bridge response was invalid.");
                var apply = response.Apply
                    ?? throw new InvalidOperationException("Source Update bridge returned no apply result.");

                var applied = apply.Status is
                    SourceUpdateApplyStatus.Applied or
                    SourceUpdateApplyStatus.AppliedWithHandlerWarnings;
                if (applied)
                {
                    baseline!.Commit(proposal);
                    TrackIntervention(proposal, apply, operationId);
                }
                var worktreeAhead = applied && !string.Equals(
                    await File.ReadAllTextAsync(proposal.SourcePath, lifetime.Token),
                    proposal.ResultSource,
                    StringComparison.Ordinal);
                var state = apply.Status switch
                {
                    SourceUpdateApplyStatus.Applied => "applied",
                    SourceUpdateApplyStatus.AppliedWithHandlerWarnings => "restart-required",
                    SourceUpdateApplyStatus.RestartRequired => "restart-required",
                    _ => "rejected"
                };
                var evidencePath = SourceUpdateCoordinatorPaths.Evidence(workspacePath, operationId);
                SourceUpdateCoordinatorJson.WriteFile(
                    evidencePath,
                    new
                    {
                        operationId,
                        proposal.PreviousGeneration,
                        generation = baseline!.Generation,
                        proposal.SourcePath,
                        diffPath,
                        proposal.DiffSha256,
                        proposal.Request.PreviousSourceHash,
                        proposal.Request.ResultSourceHash,
                        proposal.Request.ExpectedMethodToken,
                        proposal.Request.ChangedTypeTokens,
                        proposal.Request.MetadataDeltaHash,
                        proposal.Request.IlDeltaHash,
                        proposal.Request.PdbDeltaHash,
                        worktreeAhead,
                        apply
                    });
                SetStatus(new(
                    applied,
                    state,
                    baseline.Generation,
                    operationId,
                    apply,
                    evidencePath,
                    apply.Error,
                    worktreeAhead));
                return;
            }
        }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested)
        {
            SetStatus(new(
                false,
                "restart-required",
                baseline!.Generation,
                operationId,
                Error: "Coordinator stopped while the runtime result was ambiguous."));
        }
        catch (Exception exception)
        {
            SetStatus(new(
                false,
                "restart-required",
                baseline!.Generation,
                operationId,
                Error: exception.Message));
        }
        finally
        {
            generationGate.Release();
        }
    }

    private SourceUpdateCoordinatorResponse Stop()
    {
        if (generationGate.CurrentCount == 0)
            return new(false, "busy", Status().Generation, Error: "Cannot stop while an update is pending.");
        var response = new SourceUpdateCoordinatorResponse(true, "stopping", Status().Generation);
        SetStatus(response);
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            lifetime.Cancel();
        });
        return response;
    }

    private SourceUpdateCoordinatorResponse Status()
    {
        lock (statusGate)
            return status;
    }

    private void SetStatus(SourceUpdateCoordinatorResponse next)
    {
        lock (statusGate)
            status = next;
        SaveStatus();
    }

    private void SaveStatus()
    {
        SourceUpdateCoordinatorResponse snapshot;
        lock (statusGate)
            snapshot = status;
        var current = manifest ?? Manifest();
        manifest = current with
        {
            ProcessId = Environment.ProcessId,
            Ready = snapshot.State is not "starting" and not "failed" and not "stopped",
            Generation = snapshot.Generation,
            State = snapshot.State,
            OperationId = snapshot.OperationId,
            Error = snapshot.Error
        };
        SourceUpdateCoordinatorJson.WriteFile(
            SourceUpdateCoordinatorPaths.Manifest(workspacePath),
            manifest);
    }

    private void FailStartup(string error)
    {
        File.WriteAllText(SourceUpdateCoordinatorPaths.Error(workspacePath), error, Encoding.UTF8);
        var current = File.Exists(SourceUpdateCoordinatorPaths.Manifest(workspacePath))
            ? Manifest()
            : new SourceUpdateCoordinatorManifest(
                1,
                workspacePath,
                launchManifestPath,
                "",
                Environment.ProcessId,
                DateTimeOffset.UtcNow,
                false,
                0,
                "failed",
                null,
                error);
        SourceUpdateCoordinatorJson.WriteFile(
            SourceUpdateCoordinatorPaths.Manifest(workspacePath),
            current with { Ready = false, State = "failed", Error = error });
    }

    private SourceUpdateCoordinatorManifest Manifest() =>
        SourceUpdateCoordinatorJson.ReadFile<SourceUpdateCoordinatorManifest>(
            SourceUpdateCoordinatorPaths.Manifest(workspacePath));

    private LiveWorkspaceManifest Workspace()
    {
        var path = Path.Combine(workspacePath, "session.json");
        return JsonSerializer.Deserialize<LiveWorkspaceManifest>(
            File.ReadAllText(path, Encoding.UTF8),
            SourceUpdateCoordinatorJson.Options)
            ?? throw new InvalidOperationException($"Invalid live workspace manifest: {path}");
    }

    private static void VerifyTarget(
        LiveWorkspaceManifest workspace,
        LiveCodeSessionManifest session)
    {
        if (workspace.LiveCode.SessionId != session.SessionId ||
            workspace.LiveCode.ProcessId != session.ProcessId ||
            workspace.LiveCode.StartedUtc != session.StartedUtc)
        {
            throw new InvalidOperationException(
                "The Source Update coordinator discovered a different target process.");
        }
    }

    private void TrackIntervention(
        SourceUpdateDeltaProposal proposal,
        SourceUpdateApplyResult apply,
        string operationId)
    {
        var workspace = Workspace();
        var store = new LiveWorkspaceStore(workspace.RepositoryRoot);
        store.UpsertIntervention(
            workspacePath,
            new(
                "source-update-" + apply.ModuleMvid,
                LiveWorkspaceInterventionKind.SourceUpdate,
                $"Source Update generation {baseline!.Generation} is active in the running module.",
                LiveWorkspaceInterventionState.RestartRequired,
                operationId,
                proposal.SourcePath,
                "Restart the target process, then resolve this intervention."));
    }

    private static void RequireWithin(string root, string path, string description)
    {
        var prefix = Path.GetFullPath(root).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
            throw new InvalidOperationException($"The {description} is outside its allowed root or missing.");
    }
}
