namespace AlvorKit.Script.NativeBuild;

/// <summary>Per-platform build settings for a native library.</summary>
internal sealed class PlatformBuildConfig
{
    /// <summary>Linux packages installed for every target on the platform.</summary>
    public string[] Packages { get; init; } = [];

    /// <summary>Linux packages installed only for native x64 or arm64 builds.</summary>
    public string[] NativePackages { get; init; } = [];

    /// <summary>Linux packages installed only for linux-arm cross builds.</summary>
    public string[] ArmPackages { get; init; } = [];

    /// <summary>Optional dpkg architecture added before installing linux-arm packages.</summary>
    public string? ArmArchitecture { get; init; }

    /// <summary>Additional CMake configure options.</summary>
    public string[] CMakeOptions { get; init; } = [];

    /// <summary>Additional CMake configure options keyed by runtime identifier.</summary>
    public Dictionary<string, string[]> RidCMakeOptions { get; init; } = [];

    /// <summary>Path to the built CMake output relative to the build directory.</summary>
    public string? CMakeOutput { get; init; }

    /// <summary>Native link libraries passed to direct compiler builds.</summary>
    public string[] LinkLibraries { get; init; } = [];

    /// <summary>ELF shared library names allowed in Linux verification.</summary>
    public string[] AllowedDependencies { get; init; } = [];

    /// <summary>Returns the Linux packages needed for the selected target.</summary>
    public IEnumerable<string> LinuxPackages(TargetRid target) =>
        target.Architecture == TargetArchitecture.Arm
            ? Packages.Concat(ArmPackages)
            : Packages.Concat(NativePackages);

    /// <summary>Returns common and RID-specific CMake configure options.</summary>
    public IEnumerable<string> CMakeOptionsFor(TargetRid target) =>
        RidCMakeOptions.TryGetValue(target.Value, out var ridOptions)
            ? CMakeOptions.Concat(ridOptions)
            : CMakeOptions;
}
