namespace AlvorKit;

/// <summary>Starts and controls source-file updates owned by one live workspace.</summary>
internal sealed class SourceUpdateCli(LiveCodeCliContext context)
{
    internal Task<int> Start(
        string workspace,
        string? launch,
        string? discoveryDirectory)
    {
        var manifest = context.Workspaces.Read(workspace);
        launch ??= manifest.AlvorSenseSessionId is null
            ? throw new InvalidOperationException(
                "--launch is required when the workspace has no associated AlvorSense session.")
            : Path.Combine(
                manifest.RepositoryRoot,
                "out",
                "alvorsense-sessions",
                manifest.AlvorSenseSessionId,
                "editable-launch.json");
        var coordinator = SourceUpdateCoordinatorProcess.Start(
            manifest,
            launch,
            discoveryDirectory);
        context.WriteRecorded(
            workspace,
            "source-update-start",
            new { launch = Path.GetFullPath(launch), discoveryDirectory },
            coordinator);
        return Task.FromResult(0);
    }

    internal async Task<int> Apply(
        string workspace,
        string source,
        string diff,
        string? updateId)
    {
        var liveWorkspace = context.Workspaces.Read(workspace);
        var coordinator = Coordinator(liveWorkspace);
        updateId ??= DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) +
            "-" + Guid.NewGuid().ToString("N")[..8];
        ValidateUpdateId(updateId);
        var sourcePath = Path.GetFullPath(source);
        var inputDiffPath = Path.GetFullPath(diff);
        if (!File.Exists(inputDiffPath))
            throw new FileNotFoundException("Source Update diff was not found.", inputDiffPath);
        var immutableDiffPath = Path.Combine(
            SourceUpdateCoordinatorPaths.Directory(liveWorkspace.WorkspacePath),
            "diffs",
            updateId + ".diff");
        Directory.CreateDirectory(Path.GetDirectoryName(immutableDiffPath)!);
        if (File.Exists(immutableDiffPath))
            throw new InvalidOperationException($"Source Update id already exists: {updateId}");
        File.Copy(inputDiffPath, immutableDiffPath);

        var request = new SourceUpdateCoordinatorRequest(
            "apply",
            sourcePath,
            immutableDiffPath,
            updateId);
        var response = await SourceUpdateCoordinatorProcess.Send(coordinator, request);
        context.WriteRecorded(
            workspace,
            "source-update-apply",
            new
            {
                sourcePath,
                immutableDiffPath,
                updateId,
                sourceHash = HashFile(sourcePath),
                diffHash = HashFile(immutableDiffPath)
            },
            response);
        return response.Ok ? 0 : 2;
    }

    internal async Task<int> Status(string workspace)
    {
        var liveWorkspace = context.Workspaces.Read(workspace);
        var coordinator = Coordinator(liveWorkspace);
        var response = await SourceUpdateCoordinatorProcess.Send(
            coordinator,
            new("status"));
        context.WriteRecorded(
            workspace,
            "source-update-status",
            new { coordinator.ProcessId, coordinator.PipeName },
            response);
        return response.Ok ? 0 : 2;
    }

    internal async Task<int> Stop(string workspace)
    {
        var liveWorkspace = context.Workspaces.Read(workspace);
        var coordinator = Coordinator(liveWorkspace);
        var response = await SourceUpdateCoordinatorProcess.Send(
            coordinator,
            new("stop"));
        context.WriteRecorded(
            workspace,
            "source-update-stop",
            new { coordinator.ProcessId },
            response);
        return response.Ok ? 0 : 2;
    }

    internal Task<int> Coordinator(
        string workspacePath,
        string launch,
        string session,
        string? discoveryDirectory) =>
        new SourceUpdateCoordinatorHost(
            Path.GetFullPath(workspacePath),
            Path.GetFullPath(launch),
            session,
            discoveryDirectory).Run();

    private static SourceUpdateCoordinatorManifest Coordinator(
        LiveWorkspaceManifest workspace)
    {
        var path = SourceUpdateCoordinatorPaths.Manifest(workspace.WorkspacePath);
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                "This workspace has no Source Update coordinator. Run 'source start' first.");
        }
        return SourceUpdateCoordinatorJson.ReadFile<SourceUpdateCoordinatorManifest>(path);
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void ValidateUpdateId(string updateId)
    {
        if (updateId.Length is < 1 or > 96 ||
            updateId.Any(static character =>
                !char.IsLetterOrDigit(character) &&
                character is not '-' and not '_'))
        {
            throw new InvalidDataException(
                "Update id must contain 1 to 96 letters, digits, hyphens, or underscores.");
        }
    }
}
