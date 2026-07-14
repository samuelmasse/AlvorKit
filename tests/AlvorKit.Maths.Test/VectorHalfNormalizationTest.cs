namespace AlvorKit.Maths.Test;

/// <summary>Tests exact Half-vector cached normalization behavior.</summary>
[TestClass]
public sealed class VectorHalfNormalizationTest
{
    /// <summary>Successful normalization preserves the established component bits in every Half dimension.</summary>
    [TestMethod]
    public void NormalizedOr_SuccessMatchesCachedScalarFormulaBits()
    {
        Vec2h value2 = ((Half)3, (Half)4);
        Vec3h value3 = ((Half)2, (Half)(-3), (Half)6);
        Vec4h value4 = ((Half)1, (Half)(-2), (Half)3, (Half)(-4));

        AssertBits(Expected(value2), value2.NormalizedOr(Vec2h.One));
        AssertBits(Expected(value3), value3.NormalizedOr(Vec3h.One));
        AssertBits(Expected(value4), value4.NormalizedOr(Vec4h.One));
    }

    /// <summary>Zero-length input returns the supplied fallback unchanged in every Half dimension.</summary>
    [TestMethod]
    public void NormalizedOr_ZeroReturnsFallbackBits()
    {
        Vec2h fallback2 = (Half.NaN, Half.NegativeZero);
        Vec3h fallback3 = (Half.NaN, Half.NegativeZero, Half.PositiveInfinity);
        Vec4h fallback4 = (Half.NaN, Half.NegativeZero, Half.PositiveInfinity, Half.NegativeInfinity);

        AssertBits(fallback2, Vec2h.Zero.NormalizedOr(fallback2));
        AssertBits(fallback3, Vec3h.Zero.NormalizedOr(fallback3));
        AssertBits(fallback4, Vec4h.Zero.NormalizedOr(fallback4));
    }

    private static Vec2h Expected(Vec2h value)
    {
        var lengthSquared = value.LengthSquared;
        return value / ScalarMath.Sqrt(lengthSquared);
    }

    private static Vec3h Expected(Vec3h value)
    {
        var lengthSquared = value.LengthSquared;
        return value / ScalarMath.Sqrt(lengthSquared);
    }

    private static Vec4h Expected(Vec4h value)
    {
        var lengthSquared = value.LengthSquared;
        return value / ScalarMath.Sqrt(lengthSquared);
    }

    private static void AssertBits(Vec2h expected, Vec2h actual)
    {
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.X), BitConverter.HalfToInt16Bits(actual.X));
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.Y), BitConverter.HalfToInt16Bits(actual.Y));
    }

    private static void AssertBits(Vec3h expected, Vec3h actual)
    {
        AssertBits((Vec2h)expected, (Vec2h)actual);
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.Z), BitConverter.HalfToInt16Bits(actual.Z));
    }

    private static void AssertBits(Vec4h expected, Vec4h actual)
    {
        AssertBits((Vec3h)expected, (Vec3h)actual);
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.W), BitConverter.HalfToInt16Bits(actual.W));
    }
}
