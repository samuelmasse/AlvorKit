namespace AlvorKit.Maths.Test;

/// <summary>Tests single-precision vector negation and scalar-left multiplication.</summary>
[TestClass]
public sealed class VectorNegateScalarLeftMultiplyTest
{
    /// <summary>Negation produces the same ordinary component values as System.Numerics.</summary>
    [TestMethod]
    public void Negate_ProducesExpectedOrdinaryValues()
    {
        Assert.AreEqual((Vec2)(-new System.Numerics.Vector2(2f, -3f)), -new Vec2(2f, -3f));
        Assert.AreEqual((Vec3)(-new System.Numerics.Vector3(2f, -3f, 5f)), -new Vec3(2f, -3f, 5f));
        Assert.AreEqual((Vec4)(-new System.Numerics.Vector4(2f, -3f, 5f, -7f)), -new Vec4(2f, -3f, 5f, -7f));
    }

    /// <summary>Negation preserves System.Numerics IEEE 754 result bits for signed zero, infinities, and payload NaNs.</summary>
    [TestMethod]
    public void Negate_PreservesSystemNumericsIeeeBits()
    {
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7fc12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xffc54321));

        AssertSameBits(-new Vec2(-0f, positivePayloadNaN), -new System.Numerics.Vector2(-0f, positivePayloadNaN));
        AssertSameBits(
            -new Vec3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN),
            -new System.Numerics.Vector3(float.PositiveInfinity, float.NegativeInfinity, negativePayloadNaN));
        AssertSameBits(
            -new Vec4(-0f, 0f, positivePayloadNaN, negativePayloadNaN),
            -new System.Numerics.Vector4(-0f, 0f, positivePayloadNaN, negativePayloadNaN));
    }

    /// <summary>Scalar-left multiplication produces the same ordinary component values as System.Numerics.</summary>
    [TestMethod]
    public void ScalarLeftMultiply_ProducesExpectedOrdinaryValues()
    {
        Assert.AreEqual((Vec2)(-3f * new System.Numerics.Vector2(2f, -3f)), -3f * new Vec2(2f, -3f));
        Assert.AreEqual((Vec3)(-3f * new System.Numerics.Vector3(2f, -3f, 5f)), -3f * new Vec3(2f, -3f, 5f));
        Assert.AreEqual((Vec4)(-3f * new System.Numerics.Vector4(2f, -3f, 5f, -7f)), -3f * new Vec4(2f, -3f, 5f, -7f));
    }

    /// <summary>Scalar-left multiplication preserves System.Numerics IEEE 754 result bits for exceptional operands.</summary>
    [TestMethod]
    public void ScalarLeftMultiply_PreservesSystemNumericsIeeeBits()
    {
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7fc12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xffc54321));

        AssertSameBits(-0f * new Vec2(float.NegativeInfinity, positivePayloadNaN),
            -0f * new System.Numerics.Vector2(float.NegativeInfinity, positivePayloadNaN));
        AssertSameBits(float.PositiveInfinity * new Vec3(-0f, 2f, negativePayloadNaN),
            float.PositiveInfinity * new System.Numerics.Vector3(-0f, 2f, negativePayloadNaN));
        AssertSameBits(float.NegativeInfinity * new Vec4(0f, -0f, positivePayloadNaN, negativePayloadNaN),
            float.NegativeInfinity * new System.Numerics.Vector4(0f, -0f, positivePayloadNaN, negativePayloadNaN));
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
