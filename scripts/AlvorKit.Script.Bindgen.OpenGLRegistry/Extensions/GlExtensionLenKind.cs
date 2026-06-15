namespace AlvorKit.Script.Bindgen;

/// <summary>Recognized shapes of OpenGL registry len attributes.</summary>
internal enum GlExtensionLenKind
{
    /// <summary>No len attribute is present.</summary>
    None,

    /// <summary>The len attribute is a literal integer.</summary>
    Literal,

    /// <summary>The len attribute references another parameter, optionally with a multiplier.</summary>
    ParamRef,

    /// <summary>The len attribute uses COMPSIZE metadata.</summary>
    CompSize,

    /// <summary>The len attribute is present but not recognized by the overload generator.</summary>
    Unknown
}
