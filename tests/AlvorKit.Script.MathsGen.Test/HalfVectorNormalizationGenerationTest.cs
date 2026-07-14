namespace AlvorKit.Script.MathsGen.Test;

/// <summary>Tests the measured cached Half normalization emission.</summary>
[TestClass]
public sealed class HalfVectorNormalizationGenerationTest
{
    /// <summary>Half vectors cache length squared while float vectors retain the established expression.</summary>
    [TestMethod]
    public void VectorEmitter_CachesLengthSquaredOnlyForHalfNormalizedOr()
    {
        foreach (var dimension in new[] { 2, 3, 4 })
        {
            var half = VectorFileEmitter.Emit(new(dimension, VectorCatalog.Half));
            StringAssert.Contains(half, "[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            StringAssert.Contains(half, "var lengthSquared = LengthSquared;");
            StringAssert.Contains(half,
                "return lengthSquared > (Half)0 ? this / ScalarMath.Sqrt(lengthSquared) : fallback;");
        }

        var single = VectorFileEmitter.Emit(new(3, VectorCatalog.Float));
        StringAssert.Contains(single, "LengthSquared > 0f ? this / Length : fallback");
        Assert.IsFalse(single.Contains(
            "return lengthSquared > 0f ? this / ScalarMath.Sqrt(lengthSquared) : fallback;",
            StringComparison.Ordinal));
    }
}
