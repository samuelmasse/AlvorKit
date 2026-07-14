namespace AlvorKit.Maths.Test;

/// <summary>Tests single-precision vector-right-scalar division behavior across generated dimensions.</summary>
[TestClass]
public sealed class VectorScalarDivisionTest
{
    /// <summary>Vec2, Vec3, and Vec4 division by a scalar produces the expected ordinary component quotients.</summary>
    [TestMethod]
    public void Division_ProducesExpectedOrdinaryValues()
    {
        Assert.AreEqual(new Vec2(0.5f, -3f), new Vec2(1.25f, -7.5f) / 2.5f);
        Assert.AreEqual(new Vec3(4f, -5f, 6f), new Vec3(10f, -12.5f, 15f) / 2.5f);
        Assert.AreEqual(new Vec4(1f, -2f, 3f, -4f), new Vec4(-3f, 6f, -9f, 12f) / -3f);
    }

    /// <summary>Vec2, Vec3, and Vec4 division by a scalar preserves System.Numerics IEEE 754 result bits for exceptional operands.</summary>
    [TestMethod]
    public void Division_MatchesSystemNumericsExceptionalResultBits()
    {
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

        AssertSameBits(
            new Vec2(-0f, 0f) / -2f,
            new System.Numerics.Vector2(-0f, 0f) / -2f);
        AssertSameBits(
            new Vec3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) / -0f,
            new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN) / -0f);
        AssertSameBits(
            new Vec4(-7f, 0f, -0f, positivePayloadNaN) / float.NegativeInfinity,
            new System.Numerics.Vector4(-7f, 0f, -0f, positivePayloadNaN) / float.NegativeInfinity);
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
