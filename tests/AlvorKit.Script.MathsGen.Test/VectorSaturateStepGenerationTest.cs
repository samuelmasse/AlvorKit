namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated SIMD saturation and step operations.</summary>
[TestClass]
public sealed class VectorSaturateStepGenerationTest
{
    /// <summary>Single-precision saturation and step use matching System.Numerics operations while other floating families remain component-wise.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsOnlyForFloatSaturateAndStep()
    {
        AssertSaturateAndStep(VectorFileEmitter.Emit(new(2, VectorCatalog.Float)), "Vec2", "Vector2");
        AssertSaturateAndStep(VectorFileEmitter.Emit(new(3, VectorCatalog.Float)), "Vec3", "Vector3");
        AssertSaturateAndStep(VectorFileEmitter.Emit(new(4, VectorCatalog.Float)), "Vec4", "Vector4");

        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));
        StringAssert.Contains(vec3d, $"public static Vec3d Saturate(Vec3d value) =>{Environment.NewLine}" +
            "        SaturatePacked(value);");
        StringAssert.Contains(vec3d, $"public static Vec3d Step(Vec3d edge, Vec3d value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.Step(edge.X, value.X),{Environment.NewLine}" +
            $"            ScalarMath.Step(edge.Y, value.Y),{Environment.NewLine}" +
            "            ScalarMath.Step(edge.Z, value.Z));");
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.ClampNative", StringComparison.Ordinal));
        Assert.IsFalse(vec3d.Contains("System.Numerics.Vector3.ConditionalSelect", StringComparison.Ordinal));
    }

    private static void AssertSaturateAndStep(string source, string vectorType, string systemType)
    {
        StringAssert.Contains(source, $"public static {vectorType} Saturate({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Clamp(value.packed, System.Numerics.{systemType}.Zero, " +
            $"System.Numerics.{systemType}.One));");
        StringAssert.Contains(source,
            $"public static {vectorType} Step({vectorType} edge, {vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.ConditionalSelect(System.Numerics.{systemType}.LessThan(value.packed, edge.packed), " +
            $"System.Numerics.{systemType}.Zero, System.Numerics.{systemType}.One));");
        StringAssert.Contains(source,
            $"public static {vectorType} Step(float edge, {vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.ConditionalSelect(System.Numerics.{systemType}.LessThan(value.packed, " +
            $"new System.Numerics.{systemType}(edge)), System.Numerics.{systemType}.Zero, System.Numerics.{systemType}.One));");
    }
}
