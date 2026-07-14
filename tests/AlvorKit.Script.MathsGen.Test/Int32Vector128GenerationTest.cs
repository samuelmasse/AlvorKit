namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Protects the complete-register Int32 and UInt32 vector generation paths.</summary>
[TestClass]
public sealed class Int32Vector128GenerationTest
{
    /// <summary>Vec4i same-scalar arithmetic, bitwise, shift, and bound operations use equal-size Vector128 expressions.</summary>
    [TestMethod]
    public void Vec4i_UsesVector128ForScreenedOperations()
    {
        var source = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));

        AssertBinary(source, "Vec4i", "int", "+");
        AssertBinary(source, "Vec4i", "int", "-");
        AssertBinary(source, "Vec4i", "int", "*");
        AssertBinary(source, "Vec4i", "int", "&");
        AssertBinary(source, "Vec4i", "int", "|");
        AssertBinary(source, "Vec4i", "int", "^");
        AssertUnary(source, "Vec4i", "int", "-");
        AssertUnary(source, "Vec4i", "int", "~");
        AssertShift(source, "Vec4i", "int", "<<");
        AssertShift(source, "Vec4i", "int", ">>");
        AssertShift(source, "Vec4i", "int", ">>>");
        AssertBounds(source, "Vec4i", "int");
    }

    /// <summary>Vec4u same-scalar arithmetic, bitwise, shift, and bound operations use equal-size Vector128 expressions.</summary>
    [TestMethod]
    public void Vec4u_UsesVector128ForScreenedOperations()
    {
        var source = VectorFileEmitter.Emit(new(4, VectorCatalog.UInt));

        AssertBinary(source, "Vec4u", "uint", "+");
        AssertBinary(source, "Vec4u", "uint", "-");
        AssertBinary(source, "Vec4u", "uint", "*");
        AssertBinary(source, "Vec4u", "uint", "&");
        AssertBinary(source, "Vec4u", "uint", "|");
        AssertBinary(source, "Vec4u", "uint", "^");
        AssertUnary(source, "Vec4u", "uint", "~");
        AssertShift(source, "Vec4u", "uint", "<<");
        AssertShift(source, "Vec4u", "uint", ">>");
        AssertShift(source, "Vec4u", "uint", ">>>");
        AssertBounds(source, "Vec4u", "uint");
    }

    /// <summary>Non-target dimensions, division, remainder, and vector-count shifts retain component-wise generation.</summary>
    [TestMethod]
    public void NonTargetInt32Operations_RemainComponentWise()
    {
        var vec3i = VectorFileEmitter.Emit(new(3, VectorCatalog.Int));
        var vec4i = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));
        var vec4i64 = VectorFileEmitter.Emit(new(4, VectorCatalog.Int64));

        Assert.IsFalse(vec3i.Contains("System.Runtime.Intrinsics.Vector128<int>", StringComparison.Ordinal));
        Assert.IsFalse(vec4i64.Contains("System.Runtime.Intrinsics.Vector128<long>", StringComparison.Ordinal));
        StringAssert.Contains(vec4i, "(int)left.X / (int)right.X");
        StringAssert.Contains(vec4i, "(int)left.X % (int)right.X");
        StringAssert.Contains(vec4i, "(int)left.X << right.X");
        StringAssert.Contains(vec4i, "(int)left.X >> right.X");
        StringAssert.Contains(vec4i, "(int)left.X >>> right.X");
    }

    /// <summary>Vec2i and Vec2u use equal-size Vector64 only for the measured bitwise winners.</summary>
    [TestMethod]
    public void Vec2Int32_UsesVector64OnlyForBitwiseOperations()
    {
        AssertVector64Bitwise(VectorFileEmitter.Emit(new(2, VectorCatalog.Int)), "Vec2i", "int");
        AssertVector64Bitwise(VectorFileEmitter.Emit(new(2, VectorCatalog.UInt)), "Vec2u", "uint");
    }

    /// <summary>Only the Full-confirmed Vec3 Int32 bound helpers emit direct comparison expressions.</summary>
    [TestMethod]
    public void Vec3Int32_UsesDirectExpressionsForSelectedBounds()
    {
        var signed = VectorFileEmitter.Emit(new(3, VectorCatalog.Int));
        var unsigned = VectorFileEmitter.Emit(new(3, VectorCatalog.UInt));

        StringAssert.Contains(signed, "left.X < right.X ? left.X : right.X");
        StringAssert.Contains(unsigned, "left.X > right.X ? left.X : right.X");
        StringAssert.Contains(signed, "ScalarMath.Max(left.X, right.X)");
        StringAssert.Contains(unsigned, "ScalarMath.Min(left.X, right.X)");
        StringAssert.Contains(signed, "ScalarMath.Clamp(value.X, min.X, max.X)");
        StringAssert.Contains(unsigned, "ScalarMath.Clamp(value.X, min.X, max.X)");
    }

    private static void AssertBinary(string source, string vector, string scalar, string op) =>
        StringAssert.Contains(source,
            $"public static {vector} operator {op}({vector} left, {vector} right) =>{Environment.NewLine}" +
            $"        {FromRegister(vector, scalar, $"{ToRegister(vector, scalar, "left")} {op} {ToRegister(vector, scalar, "right")}")};");

    private static void AssertUnary(string source, string vector, string scalar, string op) =>
        StringAssert.Contains(source,
            $"public static {vector} operator {op}({vector} value) =>{Environment.NewLine}" +
            $"        {FromRegister(vector, scalar, $"{op}{ToRegister(vector, scalar, "value")}")};");

    private static void AssertShift(string source, string vector, string scalar, string op) =>
        StringAssert.Contains(source,
            $"public static {vector} operator {op}({vector} left, int right) =>{Environment.NewLine}" +
            $"        {FromRegister(vector, scalar, $"{ToRegister(vector, scalar, "left")} {op} (right & 31)")};");

    private static void AssertBounds(string source, string vector, string scalar)
    {
        var left = ToRegister(vector, scalar, "left");
        var right = ToRegister(vector, scalar, "right");
        var value = ToRegister(vector, scalar, "value");
        var min = ToRegister(vector, scalar, "min");
        var max = ToRegister(vector, scalar, "max");
        StringAssert.Contains(source, FromRegister(vector, scalar, $"System.Runtime.Intrinsics.Vector128.Min({left}, {right})"));
        StringAssert.Contains(source, FromRegister(vector, scalar, $"System.Runtime.Intrinsics.Vector128.Max({left}, {right})"));
        StringAssert.Contains(source, FromRegister(vector, scalar, $"System.Runtime.Intrinsics.Vector128.Clamp({value}, {min}, {max})"));
    }

    private static void AssertVector64Bitwise(string source, string vector, string scalar)
    {
        var left = $"Unsafe.BitCast<{vector}, System.Runtime.Intrinsics.Vector64<{scalar}>>(left)";
        var right = $"Unsafe.BitCast<{vector}, System.Runtime.Intrinsics.Vector64<{scalar}>>(right)";
        foreach (var op in new[] { "&", "|", "^" })
            StringAssert.Contains(source,
                $"Unsafe.BitCast<System.Runtime.Intrinsics.Vector64<{scalar}>, {vector}>({left} {op} {right})");
        StringAssert.Contains(source,
            $"Unsafe.BitCast<System.Runtime.Intrinsics.Vector64<{scalar}>, {vector}>(~" +
            $"Unsafe.BitCast<{vector}, System.Runtime.Intrinsics.Vector64<{scalar}>>(value))");
        Assert.IsFalse(source.Contains($"Vector64<{scalar}>>(left) +", StringComparison.Ordinal));
    }

    private static string ToRegister(string vector, string scalar, string expression) =>
        $"Unsafe.BitCast<{vector}, System.Runtime.Intrinsics.Vector128<{scalar}>>({expression})";

    private static string FromRegister(string vector, string scalar, string expression) =>
        $"Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<{scalar}>, {vector}>({expression})";
}
