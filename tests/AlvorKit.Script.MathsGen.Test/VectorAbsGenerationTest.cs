namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests generated packed single-precision absolute-value operations.</summary>
[TestClass]
public sealed class VectorAbsGenerationTest
{
    /// <summary>Float vectors use matching System.Numerics operations while other scalar families remain component-wise.</summary>
    [TestMethod]
    public void VectorEmitter_UsesSystemNumericsForFloatAbs()
    {
        AssertFloatAbs(VectorFileEmitter.Emit(new(2, VectorCatalog.Float)), "Vec2", "Vector2");
        AssertFloatAbs(VectorFileEmitter.Emit(new(3, VectorCatalog.Float)), "Vec3", "Vector3");
        AssertFloatAbs(VectorFileEmitter.Emit(new(4, VectorCatalog.Float)), "Vec4", "Vector4");

        var vec3d = VectorFileEmitter.Emit(new(3, VectorCatalog.Double));
        StringAssert.Contains(vec3d, $"public static Vec3d Abs(Vec3d value) =>{Environment.NewLine}" +
            $"        new({Environment.NewLine}" +
            $"            ScalarMath.Abs(value.X),{Environment.NewLine}" +
            $"            ScalarMath.Abs(value.Y),{Environment.NewLine}" +
            "            ScalarMath.Abs(value.Z));");
    }

    private static void AssertFloatAbs(string source, string vectorType, string systemType) =>
        StringAssert.Contains(source, $"public static {vectorType} Abs({vectorType} value) =>{Environment.NewLine}" +
            $"        new(System.Numerics.{systemType}.Abs(value.packed));");
}
