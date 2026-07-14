namespace AlvorKit.Maths.Test;

/// <summary>Tests single-precision vector-pair division and vector-right-scalar multiplication across generated dimensions.</summary>
[TestClass]
public sealed class VectorPairDivisionScaleTest
{
    /// <summary>Vec2, Vec3, and Vec4 pair division and right-scalar multiplication produce the expected ordinary component values.</summary>
    [TestMethod]
    public void PairDivisionAndScale_ProduceExpectedOrdinaryValues()
    {
        Assert.AreEqual(new Vec2(0.5f, -1.5f), new Vec2(1.25f, -7.5f) / new Vec2(2.5f, 5f));
        Assert.AreEqual(new Vec3(4f, -5f, 6f), new Vec3(10f, -12.5f, 15f) / new Vec3(2.5f));
        Assert.AreEqual(new Vec4(1f, -2f, 3f, -4f), new Vec4(-3f, 6f, -9f, 12f) / new Vec4(-3f));

        Assert.AreEqual(new Vec2(3.125f, -18.75f), new Vec2(1.25f, -7.5f) * 2.5f);
        Assert.AreEqual(new Vec3(25f, -31.25f, 37.5f), new Vec3(10f, -12.5f, 15f) * 2.5f);
        Assert.AreEqual(new Vec4(9f, -18f, 27f, -36f), new Vec4(-3f, 6f, -9f, 12f) * -3f);
    }

    /// <summary>Vec2, Vec3, and Vec4 pair division and right-scalar multiplication preserve System.Numerics IEEE 754 result bits.</summary>
    [TestMethod]
    public void PairDivisionAndScale_MatchSystemNumericsExceptionalResultBits()
    {
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

        AssertSameBits(
            new Vec2(-0f, positivePayloadNaN) / new Vec2(-3f, negativePayloadNaN),
            new System.Numerics.Vector2(-0f, positivePayloadNaN) / new System.Numerics.Vector2(-3f, negativePayloadNaN));
        AssertSameBits(
            new Vec3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) /
                new Vec3(float.PositiveInfinity, -0f, positivePayloadNaN),
            new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) /
                new System.Numerics.Vector3(float.PositiveInfinity, -0f, positivePayloadNaN));
        AssertSameBits(
            new Vec4(-7f, 0f, -0f, positivePayloadNaN) /
                new Vec4(float.NegativeInfinity, float.PositiveInfinity, -0f, negativePayloadNaN),
            new System.Numerics.Vector4(-7f, 0f, -0f, positivePayloadNaN) /
                new System.Numerics.Vector4(float.NegativeInfinity, float.PositiveInfinity, -0f, negativePayloadNaN));

        AssertSameBits(
            new Vec2(-0f, positivePayloadNaN) * -2f,
            new System.Numerics.Vector2(-0f, positivePayloadNaN) * -2f);
        AssertSameBits(
            new Vec3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) * -0f,
            new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) * -0f);
        AssertSameBits(
            new Vec4(-7f, 0f, -0f, positivePayloadNaN) * float.NegativeInfinity,
            new System.Numerics.Vector4(-7f, 0f, -0f, positivePayloadNaN) * float.NegativeInfinity);
    }

    private static void AssertSameBits(Vec2 actual, System.Numerics.Vector2 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
    }

    private static void AssertSameBits(Vec3 actual, System.Numerics.Vector3 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
    }

    private static void AssertSameBits(Vec4 actual, System.Numerics.Vector4 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.W), BitConverter.SingleToInt32Bits(actual.W));
    }
}
