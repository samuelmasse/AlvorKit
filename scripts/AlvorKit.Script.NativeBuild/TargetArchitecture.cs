namespace AlvorKit.Script.NativeBuild;

/// <summary>CPU architectures supported by native runtime packages.</summary>
internal enum TargetArchitecture
{
    /// <summary>64-bit x86 architecture.</summary>
    X64,

    /// <summary>32-bit x86 architecture.</summary>
    X86,

    /// <summary>64-bit ARM architecture.</summary>
    Arm64,

    /// <summary>32-bit ARM architecture.</summary>
    Arm
}
