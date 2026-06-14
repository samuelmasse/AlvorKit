using System.Runtime.InteropServices;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Operating system and architecture values used for host compatibility checks.</summary>
internal sealed class HostInfo
{
    /// <summary>Creates host information from platform flags and architecture.</summary>
    public HostInfo(bool isWindows, bool isLinux, bool isMacOS, Architecture architecture)
    {
        IsWindows = isWindows;
        IsLinux = isLinux;
        IsMacOS = isMacOS;
        Architecture = architecture;
    }

    /// <summary>True when the process is running on Windows.</summary>
    public bool IsWindows { get; }

    /// <summary>True when the process is running on Linux.</summary>
    public bool IsLinux { get; }

    /// <summary>True when the process is running on macOS.</summary>
    public bool IsMacOS { get; }

    /// <summary>Process architecture reported by the runtime.</summary>
    public Architecture Architecture { get; }

    /// <summary>Reads host information from the current process.</summary>
    public static HostInfo Current() =>
        new(OperatingSystem.IsWindows(), OperatingSystem.IsLinux(), OperatingSystem.IsMacOS(), RuntimeInformation.ProcessArchitecture);
}
