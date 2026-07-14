namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Protects the measured complete-register Boolean selection paths.</summary>
[TestClass]
public sealed class BooleanSelectGenerationTest
{
    /// <summary>Vec4 Boolean masks use native selection only for the screened float and Int32 payloads.</summary>
    [TestMethod]
    public void Vec4b_UsesVector128ForFloatAndInt32Selection()
    {
        var source = VectorFileEmitter.Emit(new(4, VectorCatalog.Bool));

        StringAssert.Contains(source, NativeSelect("Vec4", "float", true));
        StringAssert.Contains(source, NativeSelect("Vec4i", "int", false));
        StringAssert.Contains(source, "public readonly Vec4d Select(Vec4d whenTrue, Vec4d whenFalse) =>");
        StringAssert.Contains(source, "new(ScalarMath.Select(X, whenTrue.X, whenFalse.X)");
    }

    /// <summary>Partial-register Boolean vectors retain component selection because mask packing lost the screen.</summary>
    [TestMethod]
    public void Vec2bAndVec3b_RetainComponentSelection()
    {
        var vec2 = VectorFileEmitter.Emit(new(2, VectorCatalog.Bool));
        var vec3 = VectorFileEmitter.Emit(new(3, VectorCatalog.Bool));

        Assert.IsFalse(vec2.Contains("Vector128.ConditionalSelect", StringComparison.Ordinal));
        Assert.IsFalse(vec3.Contains("Vector128.ConditionalSelect", StringComparison.Ordinal));
        StringAssert.Contains(vec2, "new(ScalarMath.Select(X, whenTrue.X, whenFalse.X)");
        StringAssert.Contains(vec3, "new(ScalarMath.Select(X, whenTrue.X, whenFalse.X)");
    }

    private static string NativeSelect(string payloadType, string scalar, bool reinterpretMask) =>
        $"Unsafe.BitCast<System.Runtime.Intrinsics.Vector128<{scalar}>, {payloadType}>(" +
        "System.Runtime.Intrinsics.Vector128.ConditionalSelect(" +
        (reinterpretMask ? "System.Runtime.Intrinsics.Vector128.As<int, float>(" : "") +
        "System.Runtime.Intrinsics.Vector128.Create(X ? -1 : 0, Y ? -1 : 0, Z ? -1 : 0, W ? -1 : 0)" +
        (reinterpretMask ? ")" : "");
}
