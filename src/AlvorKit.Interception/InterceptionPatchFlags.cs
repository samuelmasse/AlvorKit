namespace AlvorKit.Interception;

/// <summary>Code generation policy for one replacement body.</summary>
[Flags]
public enum InterceptionPatchFlags : uint
{
    /// <summary>Use ordinary CoreCLR code-generation policy.</summary>
    None = 0,

    /// <summary>Prevent the active replacement body itself from being inlined.</summary>
    DisableInlining = 1 << 0
}
