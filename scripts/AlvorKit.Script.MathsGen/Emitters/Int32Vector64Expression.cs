namespace AlvorKit.Script.MathsGen;

/// <summary>Builds measured equal-size Vector64 expressions for two-lane Int32 bitwise operations.</summary>
internal static class Int32Vector64Expression
{
    /// <summary>Gets whether the vector exactly fills a two-lane Int32 or UInt32 register.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Dimension == 2 && vector.Scalar.Kind is ScalarKind.Int or ScalarKind.UInt;

    /// <summary>Returns an expression that applies a unary register operator.</summary>
    public static string Unary(VectorSpec vector, string op, string value) =>
        FromRegister(vector, $"{op}{ToRegister(vector, value)}");

    /// <summary>Returns an expression that applies a binary register operator.</summary>
    public static string Binary(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} {ToRegister(vector, right)}");

    private static string ToRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{vector.TypeName}, {RegisterType(vector)}>({expression})";

    private static string FromRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{RegisterType(vector)}, {vector.TypeName}>({expression})";

    private static string RegisterType(VectorSpec vector) =>
        $"System.Runtime.Intrinsics.Vector64<{vector.Scalar.CSharpName}>";
}
