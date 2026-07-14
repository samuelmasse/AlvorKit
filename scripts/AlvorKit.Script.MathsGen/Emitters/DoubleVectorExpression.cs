namespace AlvorKit.Script.MathsGen;

/// <summary>Builds equal-size fixed-width expressions for complete-register double vectors.</summary>
internal static class DoubleVectorExpression
{
    /// <summary>Gets whether a vector exactly fills a supported fixed-width double register.</summary>
    public static bool Supports(VectorSpec vector) =>
        vector.Scalar.Kind == ScalarKind.Double && vector.Dimension is 2 or 4;

    /// <summary>Gets whether a screened vector-pair operator benefits from the fixed-width representation.</summary>
    public static bool SupportsBinary(VectorSpec vector, string op) =>
        Supports(vector) && (op == "/" || vector.Dimension == 2 && op == "+");

    /// <summary>Gets whether a screened vector-right-scalar operator benefits from the fixed-width representation.</summary>
    public static bool SupportsVectorScalar(VectorSpec vector, string op) =>
        Supports(vector) && op == "/";

    /// <summary>Gets whether a screened unary operator benefits from the fixed-width representation.</summary>
    public static bool SupportsUnary(VectorSpec vector, string op) =>
        vector.Scalar.Kind == ScalarKind.Double && vector.Dimension == 2 && op == "-";

    /// <summary>Returns an expression that applies a binary fixed-width operator.</summary>
    public static string Binary(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} {ToRegister(vector, right)}");

    /// <summary>Returns an expression that applies a fixed-width vector-right-scalar operator.</summary>
    public static string VectorScalar(VectorSpec vector, string left, string op, string right) =>
        FromRegister(vector, $"{ToRegister(vector, left)} {op} {RegisterClass(vector)}.Create({right})");

    /// <summary>Returns an expression that applies a unary fixed-width operator.</summary>
    public static string Unary(VectorSpec vector, string op, string value) =>
        FromRegister(vector, $"{op}{ToRegister(vector, value)}");

    /// <summary>Returns an expression that invokes a fixed-width function with vector arguments.</summary>
    public static string Function(VectorSpec vector, string method, IReadOnlyList<string> arguments) =>
        FromRegister(vector,
            $"{RegisterClass(vector)}.{method}({string.Join(", ", arguments.Select(argument => ToRegister(vector, argument)))})");

    /// <summary>Returns an expression that computes component-wise square roots.</summary>
    public static string Sqrt(VectorSpec vector, string value) =>
        FromRegister(vector, $"{RegisterClass(vector)}.Sqrt({ToRegister(vector, value)})");

    /// <summary>Returns an expression that divides one by component-wise square roots without reassociation.</summary>
    public static string InverseSqrt(VectorSpec vector, string value) =>
        FromRegister(vector,
            $"{RegisterClass(vector)}.Create(1d) / {RegisterClass(vector)}.Sqrt({ToRegister(vector, value)})");

    /// <summary>Returns an exact packed vector-edge Step expression.</summary>
    public static string Step(VectorSpec vector, string edge, string value)
    {
        var register = RegisterClass(vector);
        return FromRegister(vector,
            $"{register}.ConditionalSelect({register}.LessThan({ToRegister(vector, value)}, {ToRegister(vector, edge)}), " +
            $"{register}.Create(0d), {register}.Create(1d))");
    }

    /// <summary>Returns a packed Saturate expression with regular System vector clamp semantics.</summary>
    public static string Saturate(VectorSpec vector, string value)
    {
        var register = RegisterClass(vector);
        return FromRegister(vector,
            $"{register}.Clamp({ToRegister(vector, value)}, {register}.Create(0d), {register}.Create(1d))");
    }

    /// <summary>Reinterprets a public vector as its equal-size register representation.</summary>
    private static string ToRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{vector.TypeName}, {RegisterType(vector)}>({expression})";

    /// <summary>Reinterprets a register result as its equal-size public vector representation.</summary>
    private static string FromRegister(VectorSpec vector, string expression) =>
        $"Unsafe.BitCast<{RegisterType(vector)}, {vector.TypeName}>({expression})";

    /// <summary>Returns the fixed-width static vector class for a supported vector.</summary>
    private static string RegisterClass(VectorSpec vector) =>
        $"System.Runtime.Intrinsics.Vector{vector.Dimension * 64}";

    /// <summary>Returns the fixed-width register type for a supported vector.</summary>
    private static string RegisterType(VectorSpec vector) =>
        $"{RegisterClass(vector)}<double>";
}
