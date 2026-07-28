namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Identifies the ECMA-335 header encoding used by a loaded method body.</summary>
public enum LoadedMethodBodyHeaderKind
{
    /// <summary>The one-byte tiny header encoding.</summary>
    Tiny,

    /// <summary>The variable-size fat header encoding.</summary>
    Fat
}
