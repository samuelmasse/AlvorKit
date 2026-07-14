namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Protects the complete-register Int64 and UInt64 vector generation paths.</summary>
[TestClass]
public sealed class Int64VectorGenerationTest
{
    /// <summary>Vec2i64 and Vec4i64 emit the screened signed operations through their natural registers.</summary>
    [TestMethod]
    public void SignedNaturalRegisterVectors_UseFixedWidthExpressions()
    {
        AssertScreenedOperations(VectorFileEmitter.Emit(new(2, VectorCatalog.Int64)), "Vec2i64", "long", "Vector128", signed: true);
        AssertScreenedOperations(VectorFileEmitter.Emit(new(4, VectorCatalog.Int64)), "Vec4i64", "long", "Vector256", signed: true);
    }

    /// <summary>Vec2u64 and Vec4u64 emit the screened unsigned operations through their natural registers.</summary>
    [TestMethod]
    public void UnsignedNaturalRegisterVectors_UseFixedWidthExpressions()
    {
        AssertScreenedOperations(VectorFileEmitter.Emit(new(2, VectorCatalog.UInt64)), "Vec2u64", "ulong", "Vector128", signed: false);
        AssertScreenedOperations(VectorFileEmitter.Emit(new(4, VectorCatalog.UInt64)), "Vec4u64", "ulong", "Vector256", signed: false);
    }

    /// <summary>Vec3, division, remainder, comparisons, vector-count shifts, and scalar overloads remain component-wise.</summary>
    [TestMethod]
    public void NonTargetInt64Operations_RemainComponentWise()
    {
        var vec3i64 = VectorFileEmitter.Emit(new(3, VectorCatalog.Int64));
        var vec2i64 = VectorFileEmitter.Emit(new(2, VectorCatalog.Int64));
        var vec4u64 = VectorFileEmitter.Emit(new(4, VectorCatalog.UInt64));

        Assert.IsFalse(vec3i64.Contains("System.Runtime.Intrinsics.Vector", StringComparison.Ordinal));
        StringAssert.Contains(vec2i64, "(long)left.X / (long)right.X");
        StringAssert.Contains(vec2i64, "(long)left.X % (long)right.X");
        StringAssert.Contains(vec2i64, "(long)left.X << right.X");
        StringAssert.Contains(vec2i64, "(long)left.X >> right.X");
        StringAssert.Contains(vec2i64, "(long)left.X >>> right.X");
        StringAssert.Contains(vec2i64, "(long)left.X + (long)right");
        StringAssert.Contains(vec2i64, "left.X < right.X");
        StringAssert.Contains(vec2i64, "left.X == right.X");
        StringAssert.Contains(vec4u64, "(ulong)left.X / (ulong)right.X");
        StringAssert.Contains(vec4u64, "(ulong)left.X % (ulong)right.X");
        StringAssert.Contains(vec4u64, "(ulong)left.X << right.X");
    }

    private static void AssertScreenedOperations(string source, string vector, string scalar, string registerClass, bool signed)
    {
        foreach (var op in new[] { "+", "-", "*", "&", "|", "^" })
            AssertBinary(source, vector, scalar, registerClass, op);

        if (signed)
            AssertUnary(source, vector, scalar, registerClass, "-");
        AssertUnary(source, vector, scalar, registerClass, "~");
        foreach (var op in new[] { "<<", ">>", ">>>" })
            AssertShift(source, vector, scalar, registerClass, op);
        AssertBounds(source, vector, scalar, registerClass);
    }

    private static void AssertBinary(string source, string vector, string scalar, string registerClass, string op) =>
        StringAssert.Contains(source,
            $"public static {vector} operator {op}({vector} left, {vector} right) =>{Environment.NewLine}" +
            $"        {FromRegister(vector, scalar, registerClass, $"{ToRegister(vector, scalar, registerClass, "left")} {op} " +
                ToRegister(vector, scalar, registerClass, "right"))};");

    private static void AssertUnary(string source, string vector, string scalar, string registerClass, string op) =>
        StringAssert.Contains(source,
            $"public static {vector} operator {op}({vector} value) =>{Environment.NewLine}" +
            $"        {FromRegister(vector, scalar, registerClass, $"{op}{ToRegister(vector, scalar, registerClass, "value")}")};");

    private static void AssertShift(string source, string vector, string scalar, string registerClass, string op) =>
        StringAssert.Contains(source,
            $"public static {vector} operator {op}({vector} left, int right) =>{Environment.NewLine}" +
            $"        {FromRegister(vector, scalar, registerClass, $"{ToRegister(vector, scalar, registerClass, "left")} {op} (right & 63)")};");

    private static void AssertBounds(string source, string vector, string scalar, string registerClass)
    {
        var left = ToRegister(vector, scalar, registerClass, "left");
        var right = ToRegister(vector, scalar, registerClass, "right");
        var value = ToRegister(vector, scalar, registerClass, "value");
        var min = ToRegister(vector, scalar, registerClass, "min");
        var max = ToRegister(vector, scalar, registerClass, "max");
        StringAssert.Contains(source, FromRegister(vector, scalar, registerClass,
            $"System.Runtime.Intrinsics.{registerClass}.Min({left}, {right})"));
        StringAssert.Contains(source, FromRegister(vector, scalar, registerClass,
            $"System.Runtime.Intrinsics.{registerClass}.Max({left}, {right})"));
        StringAssert.Contains(source, FromRegister(vector, scalar, registerClass,
            $"System.Runtime.Intrinsics.{registerClass}.Clamp({value}, {min}, {max})"));
    }

    private static string ToRegister(string vector, string scalar, string registerClass, string expression) =>
        $"Unsafe.BitCast<{vector}, System.Runtime.Intrinsics.{registerClass}<{scalar}>>({expression})";

    private static string FromRegister(string vector, string scalar, string registerClass, string expression) =>
        $"Unsafe.BitCast<System.Runtime.Intrinsics.{registerClass}<{scalar}>, {vector}>({expression})";
}
