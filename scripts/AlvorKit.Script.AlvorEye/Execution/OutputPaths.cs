namespace AlvorKit.Script.AlvorEye;

/// <summary>Creates output directories for AlvorEye runs.</summary>
internal static class OutputPaths
{
    /// <summary>Creates and returns the run directory for a scenario.</summary>
    public static (string RunId, string RunDirectory, string FramesDirectory, string LogsDirectory) Create(
        string repoRoot,
        ScenarioOutput output)
    {
        var runId = output.RunId ?? DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
        var runDirectory = output.Directory is { Length: > 0 } directory
            ? Resolve(repoRoot, directory)
            : Path.Combine(repoRoot, "out", "alvoreye", "runs", runId);
        var framesDirectory = Path.Combine(runDirectory, "frames");
        var logsDirectory = Path.Combine(runDirectory, "logs");
        Directory.CreateDirectory(framesDirectory);
        Directory.CreateDirectory(logsDirectory);
        return (runId, runDirectory, framesDirectory, logsDirectory);
    }

    /// <summary>Resolves a possibly relative path against the repository root.</summary>
    public static string Resolve(string repoRoot, string path) => Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(repoRoot, path));
}
