namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one scalar type emitted across vector dimensions.</summary>
internal sealed record ScalarSpec(
    ScalarKind Kind,
    string CSharpName,
    string Suffix,
    string Description,
    string OneLiteral,
    string TwoLiteral,
    string ThreeLiteral,
    int SizeBytes)
{
    /// <summary>Gets whether the scalar is a Boolean mask component.</summary>
    public bool IsBool => Kind == ScalarKind.Bool;

    /// <summary>Gets whether the scalar is a floating-point type.</summary>
    public bool IsFloating => Kind is ScalarKind.Float or ScalarKind.Double or ScalarKind.Half;

    /// <summary>Gets whether the scalar is an integer type.</summary>
    public bool IsInteger => Kind is
        ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or
        ScalarKind.Int or ScalarKind.UInt or ScalarKind.Int64 or ScalarKind.UInt64 or
        ScalarKind.Int128 or ScalarKind.UInt128;

    /// <summary>Gets whether the scalar is signed.</summary>
    public bool IsSigned => Kind is
        ScalarKind.Float or ScalarKind.Double or ScalarKind.Half or ScalarKind.Int8 or
        ScalarKind.Int16 or ScalarKind.Int or ScalarKind.Int64 or ScalarKind.Int128;

    /// <summary>Gets whether integer arithmetic is promoted and must be cast back to this scalar.</summary>
    public bool RequiresArithmeticCast => Kind is ScalarKind.Half or ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16;

    /// <summary>Gets the number of meaningful integer bits for bit helpers.</summary>
    public int BitWidth => Kind switch
    {
        ScalarKind.Int8 or ScalarKind.UInt8 => 8,
        ScalarKind.Int16 or ScalarKind.UInt16 => 16,
        ScalarKind.Int or ScalarKind.UInt => 32,
        ScalarKind.Int64 or ScalarKind.UInt64 => 64,
        ScalarKind.Int128 or ScalarKind.UInt128 => 128,
        _ => 0,
    };

    /// <summary>Gets the zero literal for generated expressions.</summary>
    public string ZeroLiteral => Kind switch
    {
        ScalarKind.Half => "(Half)0",
        ScalarKind.Int8 => "(sbyte)0",
        ScalarKind.UInt8 => "(byte)0",
        ScalarKind.Int16 => "(short)0",
        ScalarKind.UInt16 => "(ushort)0",
        ScalarKind.Float => "0f",
        ScalarKind.Double => "0d",
        ScalarKind.UInt => "0u",
        ScalarKind.Int64 => "0L",
        ScalarKind.UInt64 => "0UL",
        ScalarKind.Int128 => "(Int128)0",
        ScalarKind.UInt128 => "(UInt128)0",
        _ => "0",
    };

    /// <summary>Returns an expression cast back to this scalar when C# numeric promotion requires it.</summary>
    public string CastArithmetic(string expression) => RequiresArithmeticCast ? $"({CSharpName})({expression})" : expression;

    /// <summary>Gets the vector type name for this scalar and dimension.</summary>
    public string VectorName(int dimension) => $"Vec{dimension}{Suffix}";

    /// <summary>Gets the matrix type name for this scalar and shape.</summary>
    public string MatrixName(int columns, int rows) =>
        columns == rows ? $"Mat{columns}{Suffix}" : $"Mat{columns}x{rows}{Suffix}";

    /// <summary>Gets the quaternion type name for this scalar.</summary>
    public string QuaternionName() => $"Quat{Suffix}";

    /// <summary>Gets the box type name for this scalar and dimension.</summary>
    public string BoxName(int dimension) => $"Box{dimension}{Suffix}";
}
