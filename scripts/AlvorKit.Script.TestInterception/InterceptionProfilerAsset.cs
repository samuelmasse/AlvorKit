namespace AlvorKit.Script.TestInterception;

/// <summary>Resolves and validates the profiler asset for the current supported x64 host.</summary>
internal static class InterceptionProfilerAsset
{
    internal const string PathVariable =
        "ALVORKIT_INTERCEPTION_PROFILER_PATH";

    /// <summary>Gets the native package RID for the current supported host.</summary>
    internal static string RuntimeIdentifier =>
        OperatingSystem.IsWindows()
            ? "win-x64"
            : OperatingSystem.IsLinux()
                ? "linux-x64"
                : throw new PlatformNotSupportedException(
                    "The interception profiler supports Windows and Linux only.");

    /// <summary>Gets the platform-native profiler filename for the current host.</summary>
    internal static string FileName =>
        OperatingSystem.IsWindows()
            ? "AlvorKit.Interception.Profiler.Native.dll"
            : OperatingSystem.IsLinux()
                ? "libAlvorKit.Interception.Profiler.Native.so"
                : throw new PlatformNotSupportedException(
                    "The interception profiler supports Windows and Linux only.");

    /// <summary>Resolves the explicit, inherited, or packaged profiler.</summary>
    internal static string Resolve(string repositoryRoot, string? configuredPath)
    {
        CoreClrProfilerGuard.RequireCurrent(isOptedIn: true);
        var candidates = new[]
        {
            configuredPath,
            Environment.GetEnvironmentVariable(PathVariable),
            Path.Combine(
                AppContext.BaseDirectory,
                "runtimes",
                RuntimeIdentifier,
                "native",
                FileName)
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var path = Path.GetFullPath(candidate, repositoryRoot);
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException(
            $"The packaged {FileName} {RuntimeIdentifier} asset was not found. " +
            "Restore AlvorKit.Interception.Profiler.Native or pass --profiler-path.");
    }
}
