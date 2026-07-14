namespace AlvorKit.Maths.Test;

/// <summary>Tests SIMD-backed single-precision vector absolute value.</summary>
[TestClass]
public sealed class VectorAbsSimdTest
{
    private static readonly float PositivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
    private static readonly float NegativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

    /// <summary>Absolute value matches System.Numerics bits for signed zero, infinities, and payload NaNs.</summary>
    [TestMethod]
    public void Abs_MatchesSystemNumericsSemanticsAndBits()
    {
        AssertSameBits(Vec2.Abs(new(-0f, PositivePayloadNaN)), SystemAbs(new Vec2(-0f, PositivePayloadNaN)));
        AssertSameBits(Vec3.Abs(new(float.NegativeInfinity, -2f, PositivePayloadNaN)),
            SystemAbs(new Vec3(float.NegativeInfinity, -2f, PositivePayloadNaN)));
        AssertSameBits(Vec4.Abs(new(-0f, 0f, NegativePayloadNaN, -float.Epsilon)),
            SystemAbs(new Vec4(-0f, 0f, NegativePayloadNaN, -float.Epsilon)));
    }

    /// <summary>Double and Half vectors use the same IEEE sign-clearing absolute-value contract.</summary>
    [TestMethod]
    public void OtherFloatingAbs_UsesSystemIeeeSemantics()
    {
        var negativeDoubleNaN = BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8123456789ABC));
        var negativeHalfNaN = BitConverter.Int16BitsToHalf(unchecked((short)0xFE11));
        var doubleResult = Vec2d.Abs(new(-0d, negativeDoubleNaN));
        var halfResult = Vec2h.Abs(new((Half)(-0f), negativeHalfNaN));

        Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(doubleResult.X));
        Assert.AreEqual(
            BitConverter.DoubleToInt64Bits(double.Abs(negativeDoubleNaN)),
            BitConverter.DoubleToInt64Bits(doubleResult.Y));
        Assert.AreEqual((short)0, BitConverter.HalfToInt16Bits(halfResult.X));
        Assert.AreEqual(
            BitConverter.HalfToInt16Bits(Half.Abs(negativeHalfNaN)),
            BitConverter.HalfToInt16Bits(halfResult.Y));
    }

    private static Vec2 SystemAbs(Vec2 value) => System.Numerics.Vector2.Abs(value);

    private static Vec3 SystemAbs(Vec3 value) => System.Numerics.Vector3.Abs(value);

    private static Vec4 SystemAbs(Vec4 value) => System.Numerics.Vector4.Abs(value);

    private static void AssertSameBits(Vec2 actual, Vec2 expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
    }

    private static void AssertSameBits(Vec3 actual, Vec3 expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
        AssertSameBits(actual.Z, expected.Z);
    }

    private static void AssertSameBits(Vec4 actual, Vec4 expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
        AssertSameBits(actual.Z, expected.Z);
        AssertSameBits(actual.W, expected.W);
    }

    private static void AssertSameBits(float actual, float expected) =>
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
