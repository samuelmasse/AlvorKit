namespace AlvorKit.Script.AlvorSense;

/// <summary>Prepares an isolated copy of the AlvorSense command-line runtime for detached host processes.</summary>
internal static class AlvorSenseHostRuntime
{
    /// <summary>Copies the current script runtime into the session directory and returns the copied entry assembly path.</summary>
    /// <param name="sessionDir">Session directory that will own the detached host process.</param>
    /// <param name="assemblyPath">Entry assembly path for the foreground AlvorSense process.</param>
    /// <returns>The copied assembly path to execute for the host command.</returns>
    internal static string CopyForSession(string sessionDir, string assemblyPath)
    {
        var sourceDir = Path.GetDirectoryName(assemblyPath)
            ?? throw new InvalidOperationException("The AlvorSense assembly path has no directory.");
        var runtimeDir = AlvorSensePaths.HostRuntime(sessionDir);
        Directory.CreateDirectory(runtimeDir);
        CopyDirectory(sourceDir, runtimeDir);
        return Path.Combine(runtimeDir, Path.GetFileName(assemblyPath));
    }

    /// <summary>Copies all files and subdirectories from one runtime directory to another.</summary>
    /// <param name="sourceDir">Runtime directory produced by the build.</param>
    /// <param name="targetDir">Session-local runtime directory to populate.</param>
    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        foreach (var sourceFile in Directory.GetFiles(sourceDir))
        {
            var targetFile = Path.Combine(targetDir, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, targetFile, overwrite: true);
        }

        foreach (var sourceSubdir in Directory.GetDirectories(sourceDir))
        {
            var targetSubdir = Path.Combine(targetDir, Path.GetFileName(sourceSubdir));
            Directory.CreateDirectory(targetSubdir);
            CopyDirectory(sourceSubdir, targetSubdir);
        }
    }
}
