namespace AlvorKit;

/// <summary>Resolves and validates the profiler asset for the current supported host.</summary>
internal static class InterceptionProfilerAsset
{
    internal const string PathVariable =
        "ALVORKIT_INTERCEPTION_PROFILER_PATH";

    /// <summary>Gets the native package RID for the current supported host.</summary>
    internal static string RuntimeIdentifier =>
        RuntimeIdentifierFor(
            OperatingSystem.IsWindows(),
            OperatingSystem.IsLinux(),
            OperatingSystem.IsMacOS(),
            RuntimeInformation.ProcessArchitecture);

    /// <summary>Maps a supported operating system and process architecture to its package RID.</summary>
    internal static string RuntimeIdentifierFor(
        bool isWindows,
        bool isLinux,
        bool isMacOS,
        Architecture architecture) =>
        (isWindows, isLinux, isMacOS, architecture) switch
        {
            (true, false, false, Architecture.X64) => "win-x64",
            (false, true, false, Architecture.X64) => "linux-x64",
            (false, true, false, Architecture.Arm64) => "linux-arm64",
            (false, false, true, Architecture.Arm64) => "osx-arm64",
            _ => throw new PlatformNotSupportedException(
                "The interception profiler supports Windows x64, Linux x64/Arm64, and macOS Arm64 only.")
        };

    /// <summary>Gets the platform-native profiler filename for the current host.</summary>
    internal static string FileName =>
        OperatingSystem.IsWindows()
            ? "AlvorKit.Interception.Profiler.Native.dll"
            : OperatingSystem.IsLinux()
                ? "libAlvorKit.Interception.Profiler.Native.so"
                : OperatingSystem.IsMacOS()
                    ? "libAlvorKit.Interception.Profiler.Native.dylib"
                    : throw new PlatformNotSupportedException(
                        "The interception profiler supports Windows, Linux, and macOS only.");

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
