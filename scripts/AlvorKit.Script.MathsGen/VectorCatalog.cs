namespace AlvorKit.Script.MathsGen;

/// <summary>Defines the vector dimensions, scalar types, and conversion policy emitted by the generator.</summary>
internal static class VectorCatalog
{
    /// <summary>Coordinate component names in canonical order.</summary>
    public static readonly string[] Components = ["X", "Y", "Z", "W"];

    /// <summary>Primary constructor parameter names in canonical order.</summary>
    public static readonly string[] Parameters = ["x", "y", "z", "w"];

    /// <summary>All scalar types emitted by the generator.</summary>
    public static IReadOnlyList<ScalarSpec> Scalars { get; } =
    [
        new(ScalarKind.Float, "float", "", "single-precision floating-point", "1f", "2f", "3f", 4),
        new(ScalarKind.Double, "double", "d", "double-precision floating-point", "1d", "2d", "3d", 8),
        new(ScalarKind.Half, "Half", "h", "half-precision floating-point", "(Half)1", "(Half)2", "(Half)3", 2),
        new(ScalarKind.Bool, "bool", "b", "Boolean", "true", "true", "true", 4),
        new(ScalarKind.Int8, "sbyte", "i8", "signed 8-bit integer", "(sbyte)1", "(sbyte)2", "(sbyte)3", 1),
        new(ScalarKind.UInt8, "byte", "u8", "unsigned 8-bit integer", "(byte)1", "(byte)2", "(byte)3", 1),
        new(ScalarKind.Int16, "short", "i16", "signed 16-bit integer", "(short)1", "(short)2", "(short)3", 2),
        new(ScalarKind.UInt16, "ushort", "u16", "unsigned 16-bit integer", "(ushort)1", "(ushort)2", "(ushort)3", 2),
        new(ScalarKind.Int, "int", "i", "signed 32-bit integer", "1", "2", "3", 4),
        new(ScalarKind.UInt, "uint", "u", "unsigned 32-bit integer", "1u", "2u", "3u", 4),
        new(ScalarKind.Int64, "long", "i64", "signed 64-bit integer", "1L", "2L", "3L", 8),
        new(ScalarKind.UInt64, "ulong", "u64", "unsigned 64-bit integer", "1UL", "2UL", "3UL", 8),
        new(ScalarKind.Int128, "Int128", "i128", "signed 128-bit integer", "(Int128)1", "(Int128)2", "(Int128)3", 16),
        new(ScalarKind.UInt128, "UInt128", "u128", "unsigned 128-bit integer", "(UInt128)1", "(UInt128)2", "(UInt128)3", 16),
    ];

    /// <summary>The generated Boolean scalar spec.</summary>
    public static ScalarSpec Bool { get; } = Scalars.Single(scalar => scalar.Kind == ScalarKind.Bool);

    /// <summary>The generated 32-bit integer scalar spec.</summary>
    public static ScalarSpec Int { get; } = Scalars.Single(scalar => scalar.Kind == ScalarKind.Int);

    /// <summary>All vector specifications emitted by the generator.</summary>
    public static IReadOnlyList<VectorSpec> Vectors { get; } =
        Scalars.SelectMany(scalar => new[] { 2, 3, 4 }.Select(dimension => new VectorSpec(dimension, scalar))).ToArray();

    /// <summary>Returns whether conversion from <paramref name="source"/> to <paramref name="target"/> is implicit.</summary>
    public static bool IsImplicitConversion(ScalarSpec source, ScalarSpec target) =>
        (source.Kind, target.Kind) is
            (ScalarKind.Float, ScalarKind.Double) or
            (ScalarKind.Int8, ScalarKind.Int16) or
            (ScalarKind.Int8, ScalarKind.Int) or
            (ScalarKind.Int8, ScalarKind.Int64) or
            (ScalarKind.Int8, ScalarKind.Int128) or
            (ScalarKind.Int8, ScalarKind.Float) or
            (ScalarKind.Int8, ScalarKind.Double) or
            (ScalarKind.UInt8, ScalarKind.Int16) or
            (ScalarKind.UInt8, ScalarKind.UInt16) or
            (ScalarKind.UInt8, ScalarKind.Int) or
            (ScalarKind.UInt8, ScalarKind.UInt) or
            (ScalarKind.UInt8, ScalarKind.Int64) or
            (ScalarKind.UInt8, ScalarKind.UInt64) or
            (ScalarKind.UInt8, ScalarKind.Int128) or
            (ScalarKind.UInt8, ScalarKind.UInt128) or
            (ScalarKind.UInt8, ScalarKind.Float) or
            (ScalarKind.UInt8, ScalarKind.Double) or
            (ScalarKind.Int16, ScalarKind.Int) or
            (ScalarKind.Int16, ScalarKind.Int64) or
            (ScalarKind.Int16, ScalarKind.Int128) or
            (ScalarKind.Int16, ScalarKind.Float) or
            (ScalarKind.Int16, ScalarKind.Double) or
            (ScalarKind.UInt16, ScalarKind.Int) or
            (ScalarKind.UInt16, ScalarKind.UInt) or
            (ScalarKind.UInt16, ScalarKind.Int64) or
            (ScalarKind.UInt16, ScalarKind.UInt64) or
            (ScalarKind.UInt16, ScalarKind.Int128) or
            (ScalarKind.UInt16, ScalarKind.UInt128) or
            (ScalarKind.UInt16, ScalarKind.Float) or
            (ScalarKind.UInt16, ScalarKind.Double) or
            (ScalarKind.Int, ScalarKind.Float) or
            (ScalarKind.Int, ScalarKind.Double) or
            (ScalarKind.Int, ScalarKind.Int64) or
            (ScalarKind.Int, ScalarKind.Int128) or
            (ScalarKind.UInt, ScalarKind.Float) or
            (ScalarKind.UInt, ScalarKind.Double) or
            (ScalarKind.UInt, ScalarKind.Int64) or
            (ScalarKind.UInt, ScalarKind.UInt64) or
            (ScalarKind.UInt, ScalarKind.Int128) or
            (ScalarKind.UInt, ScalarKind.UInt128) or
            (ScalarKind.Int64, ScalarKind.Float) or
            (ScalarKind.Int64, ScalarKind.Double) or
            (ScalarKind.Int64, ScalarKind.Int128) or
            (ScalarKind.UInt64, ScalarKind.Float) or
            (ScalarKind.UInt64, ScalarKind.Double) or
            (ScalarKind.UInt64, ScalarKind.Int128) or
            (ScalarKind.UInt64, ScalarKind.UInt128);
}
