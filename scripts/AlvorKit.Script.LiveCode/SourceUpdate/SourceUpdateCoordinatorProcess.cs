namespace AlvorKit.Script.LiveCode;

/// <summary>Starts and contacts the detached process that owns one Roslyn generation chain.</summary>
[ExcludeFromCodeCoverage(Justification = "Coordinates a detached local helper process and named pipe.")]
internal static class SourceUpdateCoordinatorProcess
{
    internal static SourceUpdateCoordinatorManifest Start(
        LiveWorkspaceManifest workspace,
        string launchManifestPath,
        string? discoveryDirectory)
    {
        var manifestPath = SourceUpdateCoordinatorPaths.Manifest(workspace.WorkspacePath);
        if (File.Exists(manifestPath))
        {
            var existing = SourceUpdateCoordinatorJson.ReadFile<SourceUpdateCoordinatorManifest>(manifestPath);
            if (existing.Ready && IsAlive(existing.ProcessId))
                throw new InvalidOperationException("This workspace already has a running Source Update coordinator.");
        }

        launchManifestPath = Path.GetFullPath(launchManifestPath);
        if (!File.Exists(launchManifestPath))
            throw new FileNotFoundException("Editable launch manifest was not found.", launchManifestPath);

        var pipeName = "alvorkit-source-update-" + Guid.NewGuid().ToString("N");
        var initial = new SourceUpdateCoordinatorManifest(
            1,
            workspace.WorkspacePath,
            launchManifestPath,
            pipeName,
            0,
            DateTimeOffset.UtcNow,
            false,
            0,
            "starting",
            null,
            null);
        SourceUpdateCoordinatorJson.WriteFile(manifestPath, initial);

        var assembly = CopyRuntime(workspace.WorkspacePath);
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = workspace.RepositoryRoot
        };
        start.ArgumentList.Add(assembly);
        start.ArgumentList.Add("source");
        start.ArgumentList.Add("coordinator");
        start.ArgumentList.Add("--workspace-path");
        start.ArgumentList.Add(workspace.WorkspacePath);
        start.ArgumentList.Add("--launch");
        start.ArgumentList.Add(launchManifestPath);
        start.ArgumentList.Add("--session");
        start.ArgumentList.Add(workspace.LiveCode.SessionId);
        if (discoveryDirectory is not null)
        {
            start.ArgumentList.Add("--discovery-dir");
            start.ArgumentList.Add(Path.GetFullPath(discoveryDirectory));
        }

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException("Failed to start the Source Update coordinator.");
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(manifestPath))
            {
                var ready = SourceUpdateCoordinatorJson.ReadFile<SourceUpdateCoordinatorManifest>(manifestPath);
                if (ready.Ready)
                    return ready;
                if (ready.State == "failed")
                    throw new InvalidOperationException(ready.Error ?? "Source Update coordinator failed.");
            }
            if (process.HasExited)
                break;
            Thread.Sleep(50);
        }

        var errorPath = SourceUpdateCoordinatorPaths.Error(workspace.WorkspacePath);
        var error = File.Exists(errorPath)
            ? File.ReadAllText(errorPath, Encoding.UTF8)
            : "Timed out waiting for the Source Update coordinator.";
        throw new InvalidOperationException(error);
    }

    internal static async Task<SourceUpdateCoordinatorResponse> Send(
        SourceUpdateCoordinatorManifest manifest,
        SourceUpdateCoordinatorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!manifest.Ready || !IsAlive(manifest.ProcessId))
            throw new InvalidOperationException("The Source Update coordinator is not running.");
        await using var pipe = new NamedPipeClientStream(
            ".",
            manifest.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
        await pipe.ConnectAsync(5000, cancellationToken);
        await SourceUpdateCoordinatorJson.Write(pipe, request, cancellationToken);
        return await SourceUpdateCoordinatorJson.Read<SourceUpdateCoordinatorResponse>(
            pipe,
            cancellationToken);
    }

    private static string CopyRuntime(string workspacePath)
    {
        var sourceAssembly = typeof(SourceUpdateCoordinatorProcess).Assembly.Location;
        var sourceDirectory = Path.GetDirectoryName(sourceAssembly)!;
        var targetDirectory = SourceUpdateCoordinatorPaths.Runtime(workspacePath);
        Directory.CreateDirectory(targetDirectory);
        foreach (var source in System.IO.Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, source);
            var target = Path.Combine(targetDirectory, relative);
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: true);
        }
        return Path.Combine(targetDirectory, Path.GetFileName(sourceAssembly));
    }

    private static bool IsAlive(int processId)
    {
        if (processId <= 0)
            return false;
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
