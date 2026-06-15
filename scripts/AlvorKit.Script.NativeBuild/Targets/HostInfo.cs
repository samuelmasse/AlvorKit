namespace AlvorKit.Script.NativeBuild;

/// <summary>Operating system and architecture values used for host compatibility checks.</summary>
/// <param name="IsWindows">True when the process is running on Windows.</param>
/// <param name="IsLinux">True when the process is running on Linux.</param>
/// <param name="IsMacOS">True when the process is running on macOS.</param>
/// <param name="Architecture">Process architecture reported by the runtime.</param>
internal sealed record HostInfo(bool IsWindows, bool IsLinux, bool IsMacOS, Architecture Architecture)
{
    /// <summary>Reads host information from the current process.</summary>
    [ExcludeFromCodeCoverage]
    public static HostInfo Current() =>
        new(OperatingSystem.IsWindows(), OperatingSystem.IsLinux(), OperatingSystem.IsMacOS(), RuntimeInformation.ProcessArchitecture);
}
