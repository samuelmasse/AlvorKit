using System.Runtime.InteropServices;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Parsed .NET runtime identifier with helper values for native toolchains.</summary>
internal sealed class TargetRid
{
    /// <summary>Creates a parsed runtime identifier.</summary>
    public TargetRid(string value, TargetOperatingSystem operatingSystem, TargetArchitecture architecture)
    {
        Value = value;
        OperatingSystem = operatingSystem;
        Architecture = architecture;
    }

    /// <summary>Runtime identifier string, for example linux-x64.</summary>
    public string Value { get; }

    /// <summary>Operating system represented by the runtime identifier.</summary>
    public TargetOperatingSystem OperatingSystem { get; }

    /// <summary>CPU architecture represented by the runtime identifier.</summary>
    public TargetArchitecture Architecture { get; }

    /// <summary>MSVC architecture argument used by Launch-VsDevShell.</summary>
    public string VisualStudioArchitecture => Architecture switch
    {
        TargetArchitecture.X64 => "amd64",
        TargetArchitecture.X86 => "x86",
        TargetArchitecture.Arm64 => "arm64",
        _ => throw new PlatformNotSupportedException($"{Value} is not a Windows RID.")
    };

    /// <summary>Windows architecture label used in diagnostics.</summary>
    public string WindowsArchitecture => Architecture switch
    {
        TargetArchitecture.X64 => "x64",
        TargetArchitecture.X86 => "x86",
        TargetArchitecture.Arm64 => "arm64",
        _ => throw new PlatformNotSupportedException($"{Value} is not a Windows RID.")
    };

    /// <summary>C compiler executable for Linux builds.</summary>
    public string LinuxCompiler => Architecture == TargetArchitecture.Arm ? "arm-linux-gnueabihf-gcc" : "gcc";

    /// <summary>readelf executable for Linux dependency inspection.</summary>
    public string LinuxReadElf => Architecture == TargetArchitecture.Arm ? "arm-linux-gnueabihf-readelf" : "readelf";

    /// <summary>strip executable for Linux binaries.</summary>
    public string LinuxStrip => Architecture == TargetArchitecture.Arm ? "arm-linux-gnueabihf-strip" : "strip";

    /// <summary>macOS clang architecture argument.</summary>
    public string MacArchitecture => Architecture switch
    {
        TargetArchitecture.X64 => "x86_64",
        TargetArchitecture.Arm64 => "arm64",
        _ => throw new PlatformNotSupportedException($"{Value} is not a macOS RID.")
    };

    /// <summary>Parses a supported runtime identifier string.</summary>
    public static TargetRid Parse(string rid) =>
        rid switch
        {
            "win-x64" => new(rid, TargetOperatingSystem.Windows, TargetArchitecture.X64),
            "win-x86" => new(rid, TargetOperatingSystem.Windows, TargetArchitecture.X86),
            "win-arm64" => new(rid, TargetOperatingSystem.Windows, TargetArchitecture.Arm64),
            "linux-x64" => new(rid, TargetOperatingSystem.Linux, TargetArchitecture.X64),
            "linux-arm64" => new(rid, TargetOperatingSystem.Linux, TargetArchitecture.Arm64),
            "linux-arm" => new(rid, TargetOperatingSystem.Linux, TargetArchitecture.Arm),
            "osx-x64" => new(rid, TargetOperatingSystem.MacOS, TargetArchitecture.X64),
            "osx-arm64" => new(rid, TargetOperatingSystem.MacOS, TargetArchitecture.Arm64),
            _ => throw new ArgumentException($"Unsupported RID '{rid}'.")
        };

    /// <summary>Detects the current process runtime identifier.</summary>
    public static TargetRid Current() =>
        Current(HostInfo.Current());

    /// <summary>Formats the native library file name for this runtime identifier.</summary>
    public string LibraryFileName(string libraryName) =>
        OperatingSystem switch
        {
            TargetOperatingSystem.Windows => libraryName + ".dll",
            TargetOperatingSystem.Linux => "lib" + libraryName + ".so",
            TargetOperatingSystem.MacOS => "lib" + libraryName + ".dylib",
            _ => throw new PlatformNotSupportedException()
        };

    /// <summary>Detects the current runtime identifier from supplied host data.</summary>
    internal static TargetRid Current(HostInfo host)
    {
        var architecture = FromRuntimeArchitecture(host.Architecture);
        if (host.IsWindows)
            return new("win-" + ArchSuffix(architecture), TargetOperatingSystem.Windows, architecture);
        if (host.IsLinux)
            return new("linux-" + ArchSuffix(architecture), TargetOperatingSystem.Linux, architecture);
        if (host.IsMacOS)
            return new("osx-" + ArchSuffix(architecture), TargetOperatingSystem.MacOS, architecture);
        throw new PlatformNotSupportedException("Unsupported operating system.");
    }

    /// <summary>Maps runtime architecture values to package architecture values.</summary>
    private static TargetArchitecture FromRuntimeArchitecture(Architecture architecture) =>
        architecture switch
        {
            System.Runtime.InteropServices.Architecture.X64 => TargetArchitecture.X64,
            System.Runtime.InteropServices.Architecture.X86 => TargetArchitecture.X86,
            System.Runtime.InteropServices.Architecture.Arm64 => TargetArchitecture.Arm64,
            System.Runtime.InteropServices.Architecture.Arm => TargetArchitecture.Arm,
            var other => throw new PlatformNotSupportedException($"Unsupported architecture: {other}")
        };

    /// <summary>Formats the architecture suffix used in .NET runtime identifiers.</summary>
    private static string ArchSuffix(TargetArchitecture architecture) =>
        architecture switch
        {
            TargetArchitecture.X64 => "x64",
            TargetArchitecture.X86 => "x86",
            TargetArchitecture.Arm64 => "arm64",
            TargetArchitecture.Arm => "arm",
            _ => throw new PlatformNotSupportedException()
        };
}
