namespace AlvorKit.Maths.Test;

/// <summary>Tests SIMD-composed single-precision vector interpolation formulas.</summary>
[TestClass]
public sealed class VectorInterpolationTest
{
    private static readonly float PayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));

    /// <summary>Scalar-amount linear interpolation matches the established component formula bit for bit.</summary>
    [TestMethod]
    public void ScalarLerp_MatchesComponentFormulaBits()
    {
        AssertSameBits(
            Vec2.Lerp(new(16_777_216f, 4_097f), new(16_777_218f, -4_097f), 0.5f),
            ScalarLerp2(new(16_777_216f, 4_097f), new(16_777_218f, -4_097f), 0.5f));
        AssertSameBits(
            Vec3.Lerp(new(-0f, float.PositiveInfinity, PayloadNaN), new(0f, float.PositiveInfinity, -1f), -0f),
            ScalarLerp3(new(-0f, float.PositiveInfinity, PayloadNaN), new(0f, float.PositiveInfinity, -1f), -0f));
        AssertSameBits(
            Vec4.Lerp(new(1e-30f, -1e-30f, float.MaxValue, -0f), new(-1e-30f, 1e-30f, -float.MaxValue, 0f), 0.5f),
            ScalarLerp4(new(1e-30f, -1e-30f, float.MaxValue, -0f), new(-1e-30f, 1e-30f, -float.MaxValue, 0f), 0.5f));
    }

    /// <summary>Three-component Half interpolation preserves the established per-operation rounding and exceptional bits.</summary>
    [TestMethod]
    public void HalfScalarLerp_MatchesComponentFormulaBits()
    {
        AssertSameBits(
            Vec3h.Lerp(
                new(Half.NegativeZero, Half.PositiveInfinity, Half.NaN),
                new(Half.Zero, Half.PositiveInfinity, (Half)(-1)),
                Half.NegativeZero),
            ScalarLerp3h(
                new(Half.NegativeZero, Half.PositiveInfinity, Half.NaN),
                new(Half.Zero, Half.PositiveInfinity, (Half)(-1)),
                Half.NegativeZero));
        AssertSameBits(
            Vec3h.Lerp(new(Half.Epsilon, Half.MaxValue, Half.MinValue), new((Half)(-1), Half.MinValue, Half.MaxValue), (Half)0.375f),
            ScalarLerp3h(new(Half.Epsilon, Half.MaxValue, Half.MinValue), new((Half)(-1), Half.MinValue, Half.MaxValue), (Half)0.375f));
    }

    /// <summary>Component-amount linear interpolation matches the established component formula bit for bit.</summary>
    [TestMethod]
    public void VectorLerp_MatchesComponentFormulaBits()
    {
        AssertSameBits(
            Vec2.Lerp(new(4_097f, -0f), new(-4_097f, 0f), new Vec2(0.49999997f, -0f)),
            ScalarVectorLerp2(new(4_097f, -0f), new(-4_097f, 0f), new(0.49999997f, -0f)));
        AssertSameBits(
            Vec3.Lerp(
                new(float.PositiveInfinity, PayloadNaN, 16_777_216f),
                new(float.NegativeInfinity, 2f, 16_777_218f),
                new Vec3(0f, 1f, 0.5f)),
            ScalarVectorLerp3(
                new(float.PositiveInfinity, PayloadNaN, 16_777_216f),
                new(float.NegativeInfinity, 2f, 16_777_218f),
                new(0f, 1f, 0.5f)));
        AssertSameBits(
            Vec4.Lerp(new(1f, -1f, -0f, float.NegativeInfinity), new(-1f, 1f, 0f, float.NegativeInfinity),
                new Vec4(1.0000001f, -0.0000001f, -0f, 0.5f)),
            ScalarVectorLerp4(new(1f, -1f, -0f, float.NegativeInfinity), new(-1f, 1f, 0f, float.NegativeInfinity),
                new(1.0000001f, -0.0000001f, -0f, 0.5f)));
    }

    /// <summary>Barycentric interpolation matches the established left-associated component formula bit for bit.</summary>
    [TestMethod]
    public void Barycentric_MatchesComponentFormulaBits()
    {
        AssertSameBits(
            Vec2.Barycentric(new(16_777_216f, 4_097f), new(16_777_218f, -4_097f), new(16_777_220f, 8_193f), 0.5f, -0.25f),
            ScalarBarycentric2(new(16_777_216f, 4_097f), new(16_777_218f, -4_097f), new(16_777_220f, 8_193f), 0.5f, -0.25f));
        AssertSameBits(
            Vec3.Barycentric(
                new(-0f, float.PositiveInfinity, PayloadNaN),
                new(0f, float.PositiveInfinity, -1f),
                new(-0f, float.NegativeInfinity, 1f),
                -0f,
                0.5f),
            ScalarBarycentric3(
                new(-0f, float.PositiveInfinity, PayloadNaN),
                new(0f, float.PositiveInfinity, -1f),
                new(-0f, float.NegativeInfinity, 1f),
                -0f,
                0.5f));
        AssertSameBits(
            Vec4.Barycentric(
                new(1e-30f, -1e-30f, float.MaxValue, -0f),
                new(-1e-30f, 1e-30f, -float.MaxValue, 0f),
                new(2e-30f, -2e-30f, float.MaxValue, -0f),
                0.5f,
                0.50000006f),
            ScalarBarycentric4(
                new(1e-30f, -1e-30f, float.MaxValue, -0f),
                new(-1e-30f, 1e-30f, -float.MaxValue, 0f),
                new(2e-30f, -2e-30f, float.MaxValue, -0f),
                0.5f,
                0.50000006f));
    }

    private static Vec2 ScalarLerp2(Vec2 from, Vec2 to, float amount) =>
        new(ScalarMath.Lerp(from.X, to.X, amount), ScalarMath.Lerp(from.Y, to.Y, amount));

    private static Vec3 ScalarLerp3(Vec3 from, Vec3 to, float amount) =>
        new(ScalarMath.Lerp(from.X, to.X, amount), ScalarMath.Lerp(from.Y, to.Y, amount), ScalarMath.Lerp(from.Z, to.Z, amount));

    private static Vec4 ScalarLerp4(Vec4 from, Vec4 to, float amount) =>
        new(ScalarMath.Lerp(from.X, to.X, amount), ScalarMath.Lerp(from.Y, to.Y, amount),
            ScalarMath.Lerp(from.Z, to.Z, amount), ScalarMath.Lerp(from.W, to.W, amount));

    private static Vec3h ScalarLerp3h(Vec3h from, Vec3h to, Half amount) =>
        new(
            from.X + ((to.X - from.X) * amount),
            from.Y + ((to.Y - from.Y) * amount),
            from.Z + ((to.Z - from.Z) * amount));

    private static Vec2 ScalarVectorLerp2(Vec2 from, Vec2 to, Vec2 amount) =>
        new(ScalarMath.Lerp(from.X, to.X, amount.X), ScalarMath.Lerp(from.Y, to.Y, amount.Y));

    private static Vec3 ScalarVectorLerp3(Vec3 from, Vec3 to, Vec3 amount) =>
        new(ScalarMath.Lerp(from.X, to.X, amount.X), ScalarMath.Lerp(from.Y, to.Y, amount.Y), ScalarMath.Lerp(from.Z, to.Z, amount.Z));

    private static Vec4 ScalarVectorLerp4(Vec4 from, Vec4 to, Vec4 amount) =>
        new(ScalarMath.Lerp(from.X, to.X, amount.X), ScalarMath.Lerp(from.Y, to.Y, amount.Y),
            ScalarMath.Lerp(from.Z, to.Z, amount.Z), ScalarMath.Lerp(from.W, to.W, amount.W));

    private static Vec2 ScalarBarycentric2(Vec2 a, Vec2 b, Vec2 c, float u, float v) =>
        new(ScalarMath.Barycentric(a.X, b.X, c.X, u, v), ScalarMath.Barycentric(a.Y, b.Y, c.Y, u, v));

    private static Vec3 ScalarBarycentric3(Vec3 a, Vec3 b, Vec3 c, float u, float v) =>
        new(ScalarMath.Barycentric(a.X, b.X, c.X, u, v), ScalarMath.Barycentric(a.Y, b.Y, c.Y, u, v),
            ScalarMath.Barycentric(a.Z, b.Z, c.Z, u, v));

    private static Vec4 ScalarBarycentric4(Vec4 a, Vec4 b, Vec4 c, float u, float v) =>
        new(ScalarMath.Barycentric(a.X, b.X, c.X, u, v), ScalarMath.Barycentric(a.Y, b.Y, c.Y, u, v),
            ScalarMath.Barycentric(a.Z, b.Z, c.Z, u, v), ScalarMath.Barycentric(a.W, b.W, c.W, u, v));

    private static void AssertSameBits(Vec2 actual, Vec2 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
    }

    private static void AssertSameBits(Vec3 actual, Vec3 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
    }

    private static void AssertSameBits(Vec4 actual, Vec4 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.W), BitConverter.SingleToInt32Bits(actual.W));
    }

    private static void AssertSameBits(Vec3h actual, Vec3h expected)
    {
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.X), BitConverter.HalfToInt16Bits(actual.X));
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.Y), BitConverter.HalfToInt16Bits(actual.Y));
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected.Z), BitConverter.HalfToInt16Bits(actual.Z));
    }
}
