namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests the retained fixed-width float-to-Int32 conversion generation.</summary>
[TestClass]
public sealed class FloatToInt32GenerationTest
{
    /// <summary>Vec3 and Vec4 use packed conversion and rounding paths.</summary>
    [TestMethod]
    public void Vec3AndVec4_UseRetainedPackedConversions()
    {
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        var vec3i = VectorFileEmitter.Emit(new(3, VectorCatalog.Int));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Float));
        var vec4i = VectorFileEmitter.Emit(new(4, VectorCatalog.Int));

        StringAssert.Contains(vec3, "ConvertPackedFloatToInt32(System.Runtime.Intrinsics.Vector128.Floor(");
        StringAssert.Contains(vec3, "ConvertPackedFloatToInt32(System.Runtime.Intrinsics.Vector128.Ceiling(");
        StringAssert.Contains(vec3, "ConvertPackedFloatToInt32(System.Runtime.Intrinsics.Vector128.Round(");
        StringAssert.Contains(vec3i, "ConvertFloatToInt32(value)");
        StringAssert.Contains(vec3i, "System.Runtime.Intrinsics.Vector128.ConvertToInt32(");
        StringAssert.Contains(vec4, "Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<int>, Vec4i>(");
        StringAssert.Contains(vec4, "System.Runtime.Intrinsics.Vector128.Round(");
        StringAssert.Contains(vec4i, "System.Runtime.Intrinsics.Vector128.ConvertToInt32(");
    }

    /// <summary>Vec2 retains the measured faster scalar conversion paths.</summary>
    [TestMethod]
    public void Vec2_RetainsScalarConversions()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Float));
        var vec2i = VectorFileEmitter.Emit(new(2, VectorCatalog.Int));

        Assert.IsFalse(vec2.Contains("Vector64.ConvertToInt32", StringComparison.Ordinal));
        Assert.IsFalse(vec2i.Contains("Vector64.ConvertToInt32", StringComparison.Ordinal));
        StringAssert.Contains(vec2, "(int)ScalarMath.Floor(X)");
        StringAssert.Contains(vec2i, "(int)value.X");
    }
}
