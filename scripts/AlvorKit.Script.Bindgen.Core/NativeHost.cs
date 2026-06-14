namespace AlvorKit.Script.Bindgen;

/// <summary>Maps the current host to NuGet runtime identifiers and native library file names.</summary>
internal static class NativeHost
{
    /// <summary>Gets the runtime identifier for the process host.</summary>
    internal static string CurrentRuntimeIdentifier =>
        RuntimeIdentifier(CurrentOperatingSystem(), RuntimeInformation.ProcessArchitecture);

    /// <summary>Gets the native library file name for the process host.</summary>
    internal static string CurrentLibraryFileName(string libraryName) =>
        LibraryFileName(CurrentOperatingSystem(), libraryName);

    /// <summary>Formats a runtime identifier from an operating system and CPU architecture.</summary>
    internal static string RuntimeIdentifier(NativeOperatingSystem operatingSystem, Architecture architecture) =>
        operatingSystem switch
        {
            NativeOperatingSystem.Windows => architecture switch
            {
                Architecture.X64 => "win-x64",
                Architecture.X86 => "win-x86",
                Architecture.Arm64 => "win-arm64",
                _ => throw new PlatformNotSupportedException($"Unsupported Windows architecture: {architecture}")
            },
            NativeOperatingSystem.Linux => architecture switch
            {
                Architecture.X64 => "linux-x64",
                Architecture.Arm64 => "linux-arm64",
                Architecture.Arm => "linux-arm",
                _ => throw new PlatformNotSupportedException($"Unsupported Linux architecture: {architecture}")
            },
            NativeOperatingSystem.MacOS => architecture switch
            {
                Architecture.X64 => "osx-x64",
                Architecture.Arm64 => "osx-arm64",
                _ => throw new PlatformNotSupportedException($"Unsupported macOS architecture: {architecture}")
            },
            _ => throw UnsupportedOperatingSystem()
        };

    /// <summary>Formats the platform-specific native library file name.</summary>
    internal static string LibraryFileName(NativeOperatingSystem operatingSystem, string libraryName) =>
        operatingSystem switch
        {
            NativeOperatingSystem.Windows => libraryName + ".dll",
            NativeOperatingSystem.Linux => "lib" + libraryName + ".so",
            NativeOperatingSystem.MacOS => "lib" + libraryName + ".dylib",
            _ => throw UnsupportedOperatingSystem()
        };

    /// <summary>Detects the operating system for the current process.</summary>
    private static NativeOperatingSystem CurrentOperatingSystem() =>
        OperatingSystem.IsWindows()
            ? NativeOperatingSystem.Windows
            : OperatingSystem.IsLinux()
                ? NativeOperatingSystem.Linux
                : OperatingSystem.IsMacOS()
                    ? NativeOperatingSystem.MacOS
                    : throw UnsupportedOperatingSystem();

    /// <summary>Creates the shared exception for unsupported host platforms.</summary>
    private static PlatformNotSupportedException UnsupportedOperatingSystem() =>
        new("Native export verification is not configured for this operating system.");
}
