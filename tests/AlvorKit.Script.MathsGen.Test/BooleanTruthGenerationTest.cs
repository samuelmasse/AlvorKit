namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Protects the selected direct Boolean false-operator specialization.</summary>
[TestClass]
public sealed class BooleanTruthGenerationTest
{
    /// <summary>Only Vec3b false uses the measured direct reduction; other truth operators retain property forwarding.</summary>
    [TestMethod]
    public void BooleanVectors_EmitDirectTruthOperators()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Bool));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Bool));
        var vec4 = VectorFileEmitter.Emit(new(4, VectorCatalog.Bool));

        AssertForwardedTruth(vec2, "Vec2b");
        AssertForwardedTruth(vec4, "Vec4b");
        StringAssert.Contains(vec3,
            $"/// <summary>Returns whether every component is true.</summary>{Environment.NewLine}" +
            $"    public static bool operator true(Vec3b value) =>{Environment.NewLine}        value.All;");
        StringAssert.Contains(vec3,
            $"/// <summary>Returns whether every component is false.</summary>{Environment.NewLine}" +
            $"    [MethodImpl(MethodImplOptions.AggressiveInlining)]{Environment.NewLine}" +
            $"    public static bool operator false(Vec3b value) =>{Environment.NewLine}" +
            "        !value.X && !value.Y && !value.Z;");
    }

    private static void AssertForwardedTruth(string source, string type)
    {
        StringAssert.Contains(source,
            $"public static bool operator true({type} value) =>{Environment.NewLine}        value.All;");
        StringAssert.Contains(source,
            $"public static bool operator false({type} value) =>{Environment.NewLine}        value.None;");
    }
}
