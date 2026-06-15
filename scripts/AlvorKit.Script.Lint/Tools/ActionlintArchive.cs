namespace AlvorKit.Script.Lint;

/// <summary>Describes the actionlint release asset required for a platform.</summary>
/// <param name="Os">Actionlint asset operating-system segment.</param>
/// <param name="Arch">Actionlint asset architecture segment.</param>
/// <param name="Extension">Release archive extension.</param>
/// <param name="ExecutableName">Executable filename inside the archive.</param>
internal sealed record ActionlintArchive(string Os, string Arch, string Extension, string ExecutableName)
{
    /// <summary>True when the asset is a zip archive.</summary>
    public bool IsZip => Extension.Equals("zip", StringComparison.Ordinal);

    /// <summary>Creates the release asset name for a specific actionlint version.</summary>
    public string FileName(string version) =>
        $"actionlint_{version}_{Os}_{Arch}.{Extension}";

    /// <summary>Creates the GitHub release URL for a specific actionlint version.</summary>
    public string Url(string version) =>
        $"https://github.com/rhysd/actionlint/releases/download/v{version}/{FileName(version)}";

    /// <summary>Creates the archive descriptor for the current runtime.</summary>
    [ExcludeFromCodeCoverage]
    public static ActionlintArchive Current() =>
        For(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX), RuntimeInformation.ProcessArchitecture);

    /// <summary>Creates an archive descriptor from explicit platform values for testing and runtime selection.</summary>
    public static ActionlintArchive For(bool windows, bool linux, bool osx, Architecture architecture)
    {
        var arch = architecture switch
        {
            Architecture.X64 => "amd64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"actionlint is not configured for {architecture}."),
        };

        if (windows)
            return new("windows", arch, "zip", "actionlint.exe");
        if (linux)
            return new("linux", arch, "tar.gz", "actionlint");
        if (osx)
            return new("darwin", arch, "tar.gz", "actionlint");

        throw new PlatformNotSupportedException("actionlint is only configured for Windows, Linux, and macOS.");
    }
}
