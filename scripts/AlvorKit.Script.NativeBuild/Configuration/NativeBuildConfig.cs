namespace AlvorKit.Script.NativeBuild;

/// <summary>Build manifest loaded from native/&lt;library&gt;/conf/native-build.yml.</summary>
internal sealed class NativeBuildConfig
{
    /// <summary>Build strategy name, currently single-c or cmake.</summary>
    public required string Kind { get; init; }

    /// <summary>Windows-specific build settings.</summary>
    public PlatformBuildConfig Windows { get; init; } = new();

    /// <summary>Linux-specific build settings.</summary>
    public PlatformBuildConfig Linux { get; init; } = new();

    /// <summary>macOS-specific build settings.</summary>
    public PlatformBuildConfig MacOS { get; init; } = new();

    /// <summary>Returns settings for the requested operating system.</summary>
    public PlatformBuildConfig Platform(TargetOperatingSystem operatingSystem) =>
        operatingSystem switch
        {
            TargetOperatingSystem.Windows => Windows,
            TargetOperatingSystem.Linux => Linux,
            TargetOperatingSystem.MacOS => MacOS,
            _ => throw new PlatformNotSupportedException()
        };
}
