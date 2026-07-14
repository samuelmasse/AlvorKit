namespace AlvorKit.Script.MathsGen;

/// <summary>Builds equal-layout intrinsic expressions for generated float and double quaternions.</summary>
internal static class QuaternionSimdExpression
{
    /// <summary>Returns a packed quaternion binary expression.</summary>
    public static string Binary(QuaternionSpec quaternion, string left, string op, string right) =>
        quaternion.Scalar.Kind == ScalarKind.Float
            ? $"FromPacked({left}.packed {op} {right}.packed)"
            : FromRegister(quaternion, $"{ToRegister(quaternion, left)} {op} {ToRegister(quaternion, right)}");

    /// <summary>Returns a packed quaternion and scalar expression.</summary>
    public static string VectorScalar(QuaternionSpec quaternion, string left, string op, string right) =>
        quaternion.Scalar.Kind == ScalarKind.Float && op == "*"
            ? $"FromPacked({left}.packed * {right})"
            : FromRegister(quaternion, $"{ToRegister(quaternion, left)} {op} {Create(quaternion, right)}");

    /// <summary>Returns a packed scalar and quaternion expression.</summary>
    public static string ScalarVector(QuaternionSpec quaternion, string left, string op, string right) =>
        FromRegister(quaternion, $"{Create(quaternion, left)} {op} {ToRegister(quaternion, right)}");

    /// <summary>Returns a packed quaternion unary expression.</summary>
    public static string Unary(QuaternionSpec quaternion, string op, string value) =>
        quaternion.Scalar.Kind == ScalarKind.Float
            ? $"FromPacked({op}{value}.packed)"
            : FromRegister(quaternion, $"{op}{ToRegister(quaternion, value)}");

    /// <summary>Returns an exact sign-bit conjugation expression.</summary>
    public static string Conjugate(QuaternionSpec quaternion, string value) =>
        FromRegister(quaternion, $"{ToRegister(quaternion, value)} ^ {ConjugateMask(quaternion)}");

    /// <summary>Returns the complete intrinsic register type for a quaternion.</summary>
    public static string RegisterType(QuaternionSpec quaternion) => quaternion.Scalar.Kind switch
    {
        ScalarKind.Float => "System.Runtime.Intrinsics.Vector128<float>",
        ScalarKind.Double => "System.Runtime.Intrinsics.Vector256<double>",
        _ => throw new ArgumentOutOfRangeException(nameof(quaternion)),
    };

    /// <summary>Returns the static intrinsic helper owning the quaternion register width.</summary>
    public static string RegisterApi(QuaternionSpec quaternion) => quaternion.Scalar.Kind switch
    {
        ScalarKind.Float => "System.Runtime.Intrinsics.Vector128",
        ScalarKind.Double => "System.Runtime.Intrinsics.Vector256",
        _ => throw new ArgumentOutOfRangeException(nameof(quaternion)),
    };

    private static string ToRegister(QuaternionSpec quaternion, string expression) =>
        $"Unsafe.BitCast<{quaternion.TypeName}, {RegisterType(quaternion)}>({expression})";

    private static string FromRegister(QuaternionSpec quaternion, string expression) =>
        $"Unsafe.BitCast<{RegisterType(quaternion)}, {quaternion.TypeName}>({expression})";

    private static string Create(QuaternionSpec quaternion, string expression) =>
        $"{RegisterApi(quaternion)}.Create({expression})";

    private static string ConjugateMask(QuaternionSpec quaternion) => quaternion.Scalar.Kind switch
    {
        ScalarKind.Float =>
            "Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<int>, System.Runtime.Intrinsics.Vector128<float>>(" +
            "System.Runtime.Intrinsics.Vector128.Create(int.MinValue, int.MinValue, int.MinValue, 0))",
        ScalarKind.Double =>
            "Unsafe.BitCast<System.Runtime.Intrinsics.Vector256<long>, System.Runtime.Intrinsics.Vector256<double>>(" +
            "System.Runtime.Intrinsics.Vector256.Create(long.MinValue, long.MinValue, long.MinValue, 0L))",
        _ => throw new ArgumentOutOfRangeException(nameof(quaternion)),
    };
}
