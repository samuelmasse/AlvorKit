namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests System-compatible packed double-vector Saturate generation.</summary>
[TestClass]
public sealed class DoubleVectorSaturateGenerationTest
{
    /// <summary>All double dimensions use their measured fixed-width clamp shapes.</summary>
    [TestMethod]
    public void DoubleVectors_UsePackedSaturate()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Double));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Double));

        StringAssert.Contains(vec2, "System.Runtime.Intrinsics.Vector128.Clamp(");
        StringAssert.Contains(vec3, "SaturatePacked(value)");
        StringAssert.Contains(vec3, "System.Runtime.Intrinsics.Vector256.Clamp(");
        StringAssert.Contains(vec4, "System.Runtime.Intrinsics.Vector256.Clamp(");
    }
}
