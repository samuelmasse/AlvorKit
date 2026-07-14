namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests measured fixed-width generation for complete-register double vectors.</summary>
[TestClass]
public sealed class DoubleVectorIntrinsicGenerationTest
{
    /// <summary>Vec2d emits Vector128 expressions only for the screened winning operation set.</summary>
    [TestMethod]
    public void Vec2d_UsesVector128ForScreenedOperations()
    {
        var source = VectorFileEmitter.Emit(new(2, VectorCatalog.Double));

        AssertBinary(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "+");
        AssertBinary(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "/");
        AssertVectorScalarDivide(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "Vector128");
        AssertUnary(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "-");
        AssertFunction(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "Vector128", "Min", "left", "right");
        AssertFunction(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "Vector128", "Sqrt", "value");
        AssertStep(source, "Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "Vector128");
        StringAssert.Contains(source, FromRegister("Vec2d", "System.Runtime.Intrinsics.Vector128<double>",
            "System.Runtime.Intrinsics.Vector128.Create(1d) / System.Runtime.Intrinsics.Vector128.Sqrt(" +
            ToRegister("Vec2d", "System.Runtime.Intrinsics.Vector128<double>", "value") + ")"));
    }

    /// <summary>Vec4d emits Vector256 expressions only for the screened winning operation set.</summary>
    [TestMethod]
    public void Vec4d_UsesVector256ForScreenedOperations()
    {
        var source = VectorFileEmitter.Emit(new(4, VectorCatalog.Double));

        AssertBinary(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "/");
        AssertVectorScalarDivide(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "Vector256");
        AssertFunction(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "Vector256", "Max", "left", "right");
        AssertFunction(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "Vector256", "Clamp", "value", "min", "max");
        AssertFunction(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "Vector256", "Sqrt", "value");
        AssertFunction(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "Vector256", "Truncate", "value");
        AssertStep(source, "Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "Vector256");
        StringAssert.Contains(source, FromRegister("Vec4d", "System.Runtime.Intrinsics.Vector256<double>",
            "System.Runtime.Intrinsics.Vector256.Create(1d) / System.Runtime.Intrinsics.Vector256.Sqrt(" +
            ToRegister("Vec4d", "System.Runtime.Intrinsics.Vector256<double>", "value") + ")"));
    }

    /// <summary>Rejected double operations and every Vec3d operation remain component-wise scalar expressions.</summary>
    [TestMethod]
    public void RejectedDoubleOperations_RemainComponentWise()
    {
        var vec2d = VectorFileEmitter.Emit(new(2, VectorCatalog.Double));
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));
        var vec4d = VectorFileEmitter.Emit(new(4, VectorCatalog.Double));

        Assert.IsFalse(vec3d.Contains("Unsafe.BitCast<Vec3d, System.Runtime.Intrinsics.Vector", StringComparison.Ordinal));
        StringAssert.Contains(vec2d, "(double)left.X - (double)right.X");
        StringAssert.Contains(vec2d, "(double)left.X * (double)right.X");
        StringAssert.Contains(vec2d, "(double)left.X * (double)right");
        StringAssert.Contains(vec2d, "(double)left * (double)right.X");
        StringAssert.Contains(vec2d, "(double)left / (double)right.X");
        StringAssert.Contains(vec2d, "ScalarMath.Max(left.X, right.X)");
        StringAssert.Contains(vec2d, "ScalarMath.Clamp(value.X, min.X, max.X)");
        StringAssert.Contains(vec2d, "ScalarMath.Truncate(value.X)");
        StringAssert.Contains(vec2d, "ScalarMath.Round(value.X)");
        StringAssert.Contains(vec2d, "ScalarMath.FusedMultiplyAdd(left.X, right.X, addend.X)");
        StringAssert.Contains(vec2d, "left.X * right.X + left.Y * right.Y");
        StringAssert.Contains(vec4d, "(double)left.X + (double)right.X");
        StringAssert.Contains(vec4d, "(double)left.X - (double)right.X");
        StringAssert.Contains(vec4d, "(double)left.X * (double)right.X");
        StringAssert.Contains(vec4d, "(double)left.X * (double)right");
        StringAssert.Contains(vec4d, "(double)left * (double)right.X");
        StringAssert.Contains(vec4d, "(double)left / (double)right.X");
        StringAssert.Contains(vec4d, "ScalarMath.Min(left.X, right.X)");
        StringAssert.Contains(vec4d, "public static Vec4d operator -(Vec4d value)");
        StringAssert.Contains(vec4d, "-value.X");
        StringAssert.Contains(vec4d, "ScalarMath.Round(value.X)");
        StringAssert.Contains(vec4d, "ScalarMath.FusedMultiplyAdd(left.X, right.X, addend.X)");
        StringAssert.Contains(vec4d, "left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W");
    }

    private static void AssertBinary(string source, string vector, string register, string op) =>
        StringAssert.Contains(source, FromRegister(vector, register,
            $"{ToRegister(vector, register, "left")} {op} {ToRegister(vector, register, "right")}"));

    private static void AssertVectorScalarDivide(string source, string vector, string register, string registerClass) =>
        StringAssert.Contains(source, FromRegister(vector, register,
            $"{ToRegister(vector, register, "left")} / System.Runtime.Intrinsics.{registerClass}.Create(right)"));

    private static void AssertUnary(string source, string vector, string register, string op) =>
        StringAssert.Contains(source, FromRegister(vector, register, $"{op}{ToRegister(vector, register, "value")}"));

    private static void AssertFunction(
        string source,
        string vector,
        string register,
        string registerClass,
        string method,
        params string[] arguments) =>
        StringAssert.Contains(source, FromRegister(vector, register,
            $"System.Runtime.Intrinsics.{registerClass}.{method}(" +
            string.Join(", ", arguments.Select(argument => ToRegister(vector, register, argument))) + ")"));

    private static void AssertStep(string source, string vector, string register, string registerClass) =>
        StringAssert.Contains(source, FromRegister(vector, register,
            $"System.Runtime.Intrinsics.{registerClass}.ConditionalSelect(" +
            $"System.Runtime.Intrinsics.{registerClass}.LessThan(" +
            $"{ToRegister(vector, register, "value")}, {ToRegister(vector, register, "edge")}), " +
            $"System.Runtime.Intrinsics.{registerClass}.Create(0d), System.Runtime.Intrinsics.{registerClass}.Create(1d))"));

    private static string ToRegister(string vector, string register, string expression) =>
        $"Unsafe.BitCast<{vector}, {register}>({expression})";

    private static string FromRegister(string vector, string register, string expression) =>
        $"Unsafe.BitCast<{register}, {vector}>({expression})";
}
