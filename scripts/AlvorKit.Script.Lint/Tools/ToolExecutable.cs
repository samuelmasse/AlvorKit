namespace AlvorKit.Script.Lint;

/// <summary>Resolves executable names that differ between Windows and Unix runners.</summary>
[ExcludeFromCodeCoverage]
internal static class ToolExecutable
{
    /// <summary>Returns the npx command name for the current operating system.</summary>
    public static string Npx() =>
        FindOnPath(OperatingSystem.IsWindows() ? "npx.cmd" : "npx")
        ?? (OperatingSystem.IsWindows() ? "npx.cmd" : "npx");

    /// <summary>Finds a tool executable on PATH and returns its absolute path.</summary>
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
}
