namespace AlvorKit.Script.MathsGen;

/// <summary>Builds equal-size fixed-width expressions for complete-register Int64 and UInt64 vectors.</summary>
internal static class Int64VectorExpression
{
    private const int ShiftMask = 63;

    /// <summary>Gets whether a vector exactly fills one supported Int64 or UInt64 register.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Dimension is 2 or 4 && vector.Scalar.Kind is ScalarKind.Int64 or ScalarKind.UInt64;

    /// <summary>Returns an expression that applies a unary intrinsic operator.</summary>
    public static string Unary(VectorSpec vector, string op, string value) =>
        FromRegister(vector, $"{op}{ToRegister(vector, value)}");

    /// <summary>Returns an expression that applies a binary intrinsic operator.</summary>
    public static string Binary(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} {ToRegister(vector, right)}");

    /// <summary>Returns an expression that applies an intrinsic shift with the C# Int64 count mask.</summary>
    public static string Shift(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} ({right} & {ShiftMask.ToString(CultureInfo.InvariantCulture)})");

    /// <summary>Returns an expression that invokes a fixed-width static vector function.</summary>
    public static string Function(VectorSpec vector, string method, IReadOnlyList<string> arguments) =>
        FromRegister(vector,
            $"{RegisterClass(vector)}.{method}({string.Join(", ", arguments.Select(argument => ToRegister(vector, argument)))})");

    /// <summary>Reinterprets a public vector as its equal-size register representation.</summary>
    private static string ToRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{vector.TypeName}, {RegisterType(vector)}>({expression})";

    /// <summary>Reinterprets a register result as its equal-size public vector representation.</summary>
    private static string FromRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{RegisterType(vector)}, {vector.TypeName}>({expression})";

    /// <summary>Returns the fixed-width static vector class for a supported vector.</summary>
    private static string RegisterClass(VectorSpec vector) =>
        $"System.Runtime.Intrinsics.Vector{vector.Dimension * 64}";

    /// <summary>Returns the signed or unsigned Int64 register type for a supported vector.</summary>
    private static string RegisterType(VectorSpec vector) =>
        $"{RegisterClass(vector)}<{vector.Scalar.CSharpName}>";
}
