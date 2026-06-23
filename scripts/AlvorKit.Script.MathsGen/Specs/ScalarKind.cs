namespace AlvorKit.Script.MathsGen;

/// <summary>Classifies scalar behavior needed by vector generation.</summary>
internal enum ScalarKind
{
    /// <summary>Single-precision floating-point scalar.</summary>
    Float,

    /// <summary>Double-precision floating-point scalar.</summary>
    Double,

    /// <summary>Half-precision floating-point scalar.</summary>
    Half,

    /// <summary>Boolean mask scalar.</summary>
    Bool,

    /// <summary>Signed 8-bit integer scalar.</summary>
    Int8,

    /// <summary>Unsigned 8-bit integer scalar.</summary>
    UInt8,

    /// <summary>Signed 16-bit integer scalar.</summary>
    Int16,

    /// <summary>Unsigned 16-bit integer scalar.</summary>
    UInt16,

    /// <summary>Signed 32-bit integer scalar.</summary>
    Int,

    /// <summary>Unsigned 32-bit integer scalar.</summary>
    UInt,

    /// <summary>Signed 64-bit integer scalar.</summary>
    Int64,

    /// <summary>Unsigned 64-bit integer scalar.</summary>
    UInt64,

    /// <summary>Signed 128-bit integer scalar.</summary>
    Int128,

    /// <summary>Unsigned 128-bit integer scalar.</summary>
    UInt128
}
