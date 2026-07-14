namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests retained semantics-preserving double-to-Int32 conversion generation.</summary>
[TestClass]
public sealed class DoubleToInt32GenerationTest
{
    /// <summary>Vec2d and Vec4d use x86 conversion kernels with exact normalization and fallbacks.</summary>
    [TestMethod]
    public void Vec2AndVec4Double_UsePackedConversions()
    {
        var vec2d = VectorFileEmitter.Emit(new(2, VectorCatalog.Double));
        var vec4d = VectorFileEmitter.Emit(new(4, VectorCatalog.Double));
        var vec2i = VectorFileEmitter.Emit(new(2, VectorCatalog.Int));
        var vec4i = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));

        StringAssert.Contains(vec2d, "Vec2d.ConvertToInt32Packed(this, 0)");
        StringAssert.Contains(vec2d, "Sse2.ConvertToVector128Int32WithTruncation");
        StringAssert.Contains(vec4d, "Vec4d.ConvertToInt32Packed(this, 3)");
        StringAssert.Contains(vec4d, "Avx.ConvertToVector128Int32WithTruncation");
        StringAssert.Contains(vec2d, "Vector128.ConditionalSelect(");
        StringAssert.Contains(vec4d, "Vector256.ConditionalSelect(");
        StringAssert.Contains(vec2i, "Vec2d.ConvertToInt32Packed(value, 0)");
        StringAssert.Contains(vec4i, "Vec4d.ConvertToInt32Packed(value, 0)");
    }

    /// <summary>Vec3d retains its measured faster scalar conversions.</summary>
    [TestMethod]
    public void Vec3Double_RetainsScalarConversions()
    {
        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));
        var vec3i = VectorFileEmitter.Emit(new(3, VectorCatalog.Int));

        Assert.IsFalse(vec3d.Contains("ConvertToInt32Packed", StringComparison.Ordinal));
        Assert.IsFalse(vec3i.Contains("Vec3d.ConvertToInt32Packed", StringComparison.Ordinal));
        StringAssert.Contains(vec3d, "(int)ScalarMath.Floor(X)");
    }
}
