namespace AlvorKit.Script.MathsGen;

/// <summary>Builds equal-size <c>Vector128</c> expressions for complete-register Int32 and UInt32 vectors.</summary>
internal static class Int32Vector128Expression
{
    private const int ShiftMask = 31;

    /// <summary>Gets whether a vector exactly fills one Int32 or UInt32 128-bit register.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Dimension == 4 && vector.Scalar.Kind is ScalarKind.Int or ScalarKind.UInt;

    /// <summary>Returns an expression that applies a unary intrinsic operator.</summary>
    public static string Unary(VectorSpec vector, string op, string value) =>
        FromRegister(vector, $"{op}{ToRegister(vector, value)}");

    /// <summary>Returns an expression that applies a binary intrinsic operator.</summary>
    public static string Binary(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} {ToRegister(vector, right)}");

    /// <summary>Returns an expression that applies an intrinsic shift with the C# Int32 count mask.</summary>
    public static string Shift(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} ({right} & {ShiftMask.ToString(CultureInfo.InvariantCulture)})");

    /// <summary>Returns an expression that invokes a static <c>Vector128</c> function.</summary>
    public static string Function(VectorSpec vector, string method, IReadOnlyList<string> arguments) =>
        FromRegister(vector,
            $"System.Runtime.Intrinsics.Vector128.{method}({string.Join(", ", arguments.Select(argument => ToRegister(vector, argument)))})");

    /// <summary>Reinterprets a public vector as its equal-size register representation.</summary>
    private static string ToRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{vector.TypeName}, {RegisterType(vector)}>({expression})";

    /// <summary>Reinterprets a register result as its equal-size public vector representation.</summary>
    private static string FromRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{RegisterType(vector)}, {vector.TypeName}>({expression})";

    /// <summary>Returns the signed or unsigned Int32 register type for a supported vector.</summary>
    private static string RegisterType(VectorSpec vector) =>
        $"System.Runtime.Intrinsics.Vector128<{vector.Scalar.CSharpName}>";
}
