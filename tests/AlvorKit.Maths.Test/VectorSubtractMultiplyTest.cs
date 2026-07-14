namespace AlvorKit.Maths.Test;

/// <summary>Tests single-precision vector subtraction and multiplication behavior across generated dimensions.</summary>
[TestClass]
public sealed class VectorSubtractMultiplyTest
{
    /// <summary>Vec2, Vec3, and Vec4 subtraction and multiplication produce the expected ordinary component values.</summary>
    [TestMethod]
    public void SubtractionAndMultiplication_ProduceExpectedOrdinaryValues()
    {
        Assert.AreEqual(new Vec2(-2f, -12f), new Vec2(1.25f, -7f) - new Vec2(3.25f, 5f));
        Assert.AreEqual(new Vec3(-3f, 3f, -2.5f), new Vec3(1f, -2f, 3.5f) - new Vec3(4f, -5f, 6f));
        Assert.AreEqual(new Vec4(-4f, 4f, -4f, 4f), new Vec4(1f, -2f, 3f, -4f) - new Vec4(5f, -6f, 7f, -8f));

        Assert.AreEqual(new Vec2(4.0625f, -35f), new Vec2(1.25f, -7f) * new Vec2(3.25f, 5f));
        Assert.AreEqual(new Vec3(4f, 10f, 21f), new Vec3(1f, -2f, 3.5f) * new Vec3(4f, -5f, 6f));
        Assert.AreEqual(new Vec4(5f, 12f, 21f, 32f), new Vec4(1f, -2f, 3f, -4f) * new Vec4(5f, -6f, 7f, -8f));
    }

    /// <summary>Vec2, Vec3, and Vec4 subtraction and multiplication preserve System.Numerics IEEE 754 result bits.</summary>
    [TestMethod]
    public void SubtractionAndMultiplication_MatchSystemNumericsExceptionalResultBits()
    {
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

        AssertSameBits(
            new Vec2(-0f, positivePayloadNaN) - new Vec2(0f, negativePayloadNaN),
            new System.Numerics.Vector2(-0f, positivePayloadNaN) - new System.Numerics.Vector2(0f, negativePayloadNaN));
        AssertSameBits(
            new Vec3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) -
                new Vec3(float.PositiveInfinity, float.NegativeInfinity, positivePayloadNaN),
            new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) -
                new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, positivePayloadNaN));
        AssertSameBits(
            new Vec4(-0f, 0f, float.PositiveInfinity, positivePayloadNaN) -
                new Vec4(-0f, -0f, float.NegativeInfinity, negativePayloadNaN),
            new System.Numerics.Vector4(-0f, 0f, float.PositiveInfinity, positivePayloadNaN) -
                new System.Numerics.Vector4(-0f, -0f, float.NegativeInfinity, negativePayloadNaN));

        AssertSameBits(
            new Vec2(-0f, positivePayloadNaN) * new Vec2(-7f, negativePayloadNaN),
            new System.Numerics.Vector2(-0f, positivePayloadNaN) * new System.Numerics.Vector2(-7f, negativePayloadNaN));
        AssertSameBits(
            new Vec3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) * new Vec3(0f, -0f, positivePayloadNaN),
            new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) *
                new System.Numerics.Vector3(0f, -0f, positivePayloadNaN));
        AssertSameBits(
            new Vec4(-0f, 0f, float.PositiveInfinity, positivePayloadNaN) *
                new Vec4(float.NegativeInfinity, float.PositiveInfinity, -2f, negativePayloadNaN),
            new System.Numerics.Vector4(-0f, 0f, float.PositiveInfinity, positivePayloadNaN) *
                new System.Numerics.Vector4(float.NegativeInfinity, float.PositiveInfinity, -2f, negativePayloadNaN));
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
