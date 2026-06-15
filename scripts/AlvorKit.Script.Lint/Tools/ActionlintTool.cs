namespace AlvorKit.Script.Lint;

/// <summary>Downloads and resolves the pinned actionlint executable used by the lint coordinator.</summary>
[ExcludeFromCodeCoverage]
internal sealed class ActionlintTool : IActionlintTool
{
    /// <summary>Pinned actionlint version used by local and CI lint runs.</summary>
    private const string Version = "1.7.12";

    /// <summary>Returns a usable actionlint executable path, downloading it under out/tools when missing.</summary>
    public async Task<string> EnsureAsync(string repoRoot)
    {
        var existing = FindExisting(repoRoot);
        if (existing is not null)
            return existing;

        var archive = ActionlintArchive.Current();
        var installRoot = Path.Combine(repoRoot, "out", "tools", "actionlint", Version, $"{archive.Os}-{archive.Arch}");
        var executablePath = Path.Combine(installRoot, archive.ExecutableName);
        if (File.Exists(executablePath))
            return executablePath;

        Directory.CreateDirectory(installRoot);
        var archivePath = Path.Combine(installRoot, archive.FileName(Version));
        await DownloadAsync(archive.Url(Version), archivePath);
        Extract(archivePath, installRoot, archive);
        MakeExecutable(executablePath);

        if (!File.Exists(executablePath))
            throw new InvalidOperationException($"actionlint archive did not contain {archive.ExecutableName}.");

        return executablePath;
    }

    /// <summary>Finds an actionlint executable already available in the repo root or PATH.</summary>
    private static string? FindExisting(string repoRoot)
    {
        var localName = OperatingSystem.IsWindows() ? "actionlint.exe" : "actionlint";
        var localPath = Path.Combine(repoRoot, localName);
        if (File.Exists(localPath))
            return localPath;

        return FindOnPath(localName);
    }

    /// <summary>Finds an executable on PATH.</summary>
    private static string? FindOnPath(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
            return null;

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    /// <summary>Downloads an actionlint archive to disk.</summary>
    private static async Task DownloadAsync(string url, string archivePath)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await using var destination = File.Create(archivePath);
        await response.Content.CopyToAsync(destination);
    }

    /// <summary>Extracts an actionlint release archive into the install directory.</summary>
    private static void Extract(string archivePath, string installRoot, ActionlintArchive archive)
    {
        if (archive.IsZip)
        {
            ZipFile.ExtractToDirectory(archivePath, installRoot, overwriteFiles: true);
            return;
        }

        using var archiveStream = File.OpenRead(archivePath);
        using var gzip = new GZipStream(archiveStream, CompressionMode.Decompress);
        TarFile.ExtractToDirectory(gzip, installRoot, overwriteFiles: true);
    }

    /// <summary>Marks the downloaded actionlint file as executable on Unix-like systems.</summary>
    private static void MakeExecutable(string executablePath)
    {
        if (OperatingSystem.IsWindows() || !File.Exists(executablePath))
            return;

        File.SetUnixFileMode(
            executablePath,
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
            UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
    }
}
