using System.Runtime.InteropServices;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Validates that a target can be built on a given host.</summary>
internal static class HostCompatibility
{
    /// <summary>Throws when the requested target cannot be built by the supplied host.</summary>
    public static void EnsureCanBuild(TargetRid target, HostInfo host)
    {
        if (target.OperatingSystem == TargetOperatingSystem.Windows && !host.IsWindows)
            throw new PlatformNotSupportedException($"{target.Value} must be built on Windows.");
        if (target.OperatingSystem == TargetOperatingSystem.Linux && !host.IsLinux)
            throw new PlatformNotSupportedException($"{target.Value} must be built on Linux.");
        if (target.OperatingSystem == TargetOperatingSystem.MacOS && !host.IsMacOS)
            throw new PlatformNotSupportedException($"{target.Value} must be built on macOS.");
        EnsureLinuxHostArchitecture(target, host);
    }

    /// <summary>Rejects native Linux builds on the wrong Linux architecture.</summary>
    private static void EnsureLinuxHostArchitecture(TargetRid target, HostInfo host)
    {
        if (target.OperatingSystem != TargetOperatingSystem.Linux || target.Architecture == TargetArchitecture.Arm)
            return;

        var expected = target.Architecture == TargetArchitecture.X64 ? Architecture.X64 : Architecture.Arm64;
        if (host.Architecture != expected)
            throw new PlatformNotSupportedException($"{target.Value} must be built on a {expected} Linux host.");
    }
}
