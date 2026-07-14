namespace AlvorKit.Maths.Test;

/// <summary>Tests scalar helpers shared by vector math.</summary>
[TestClass]
public sealed class ScalarMathTest
{
    /// <summary>Common numeric helpers preserve native Min/Max and regular System vector Clamp/Abs semantics.</summary>
    [TestMethod]
    public void CommonHelpers_Work()
    {
        var floatNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC12345));
        var doubleNaN = BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8123456789ABC));
        var halfNaN = BitConverter.Int16BitsToHalf(unchecked((short)0xFE11));

        AssertSameBits(float.Min(floatNaN, 1f), ScalarMath.Min(floatNaN, 1f));
        AssertSameBits(float.Min(1f, floatNaN), ScalarMath.Min(1f, floatNaN));
        AssertSameBits(float.Max(-0f, 0f), ScalarMath.Max(-0f, 0f));
        AssertSameBits(double.Min(doubleNaN, 1d), ScalarMath.Min(doubleNaN, 1d));
        AssertSameBits(Half.Max(halfNaN, (Half)1f), ScalarMath.Max(halfNaN, (Half)1f));
        Assert.AreEqual(0, BitConverter.SingleToInt32Bits(ScalarMath.Abs(-0f)));
        Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(ScalarMath.Abs(-0d)));
        Assert.AreEqual((short)0, BitConverter.HalfToInt16Bits(ScalarMath.Abs((Half)(-0f))));
        Assert.AreEqual(int.MinValue, ScalarMath.Abs(int.MinValue));
        AssertSameBits(SystemClamp(floatNaN, 0f, 1f), ScalarMath.Clamp(floatNaN, 0f, 1f));
        AssertSameBits(SystemClamp(2f, 3f, 1f), ScalarMath.Clamp(2f, 3f, 1f));
        AssertSameBits(SystemClamp(-0d, 0d, 1d), ScalarMath.Clamp(-0d, 0d, 1d));
        AssertSameBits(SystemClamp(doubleNaN, 0d, 1d), ScalarMath.Saturate(doubleNaN));
        Assert.AreEqual(0f, ScalarMath.Saturate(-1f));
        Assert.AreEqual(0.5f, ScalarMath.Saturate(0.5f));
        Assert.AreEqual(1f, ScalarMath.Saturate(2f));
        Assert.AreEqual((Half)1f, ScalarMath.Saturate((Half)2f));
    }

    /// <summary>Floating helpers expose interpolation, rounding, modulo, and special-value operations.</summary>
    [TestMethod]
    public void FloatingHelpers_Work()
    {
        Assert.AreEqual(5f, ScalarMath.Lerp(0f, 10f, 0.5f));
        Assert.AreEqual(3f, ScalarMath.Barycentric(1f, 5f, 9f, 0.25f, 0.125f));
        Assert.AreEqual(-2f, ScalarMath.Floor(-1.25f));
        Assert.AreEqual(0.75f, ScalarMath.FractionalPart(-1.25f));
        Assert.AreEqual(2f, ScalarMath.Modulo(-1f, 3f));
        Assert.AreEqual(2f, ScalarMath.Mod(-1f, 3f));
        Assert.AreEqual(0f, ScalarMath.Step(0.5f, 0.25f));
        Assert.AreEqual(1f, ScalarMath.Step(0.5f, 0.5f));
        Assert.AreEqual(0.5f, ScalarMath.SmoothStep(0f, 1f, 0.5f));
        Assert.AreEqual(0.5f, ScalarMath.InverseSqrt(4f));
        Assert.AreEqual(1d, ScalarMath.Cos(0d));
        Assert.AreEqual(0d, ScalarMath.Asin(0d));
        Assert.AreEqual(0d, ScalarMath.Atan(0d));
        Assert.AreEqual(3d, ScalarMath.Log2(8d));
        Assert.IsTrue(ScalarMath.IsNaN(float.NaN));
        Assert.IsTrue(ScalarMath.IsInfinity(float.PositiveInfinity));
        Assert.IsFalse(ScalarMath.IsFinite(float.PositiveInfinity));
    }

    /// <summary>Integer bit helpers respect each scalar type's native bit width and signed power-of-two rules.</summary>
    [TestMethod]
    public void BitHelpers_Work()
    {
        var highUInt128 = UInt128.One << 127;

        Assert.AreEqual(3, ScalarMath.BitCount(0b1011));
        Assert.AreEqual(0, ScalarMath.LeadingZeroCount(int.MinValue));
        Assert.AreEqual(32, ScalarMath.TrailingZeroCount(0));
        Assert.AreEqual(-1, ScalarMath.FindLeastSignificantBit(0));
        Assert.AreEqual(3, ScalarMath.FindLeastSignificantBit(8));
        Assert.AreEqual(-1, ScalarMath.FindMostSignificantBit(0));
        Assert.AreEqual(31, ScalarMath.FindMostSignificantBit(int.MinValue));
        Assert.AreEqual(127, ScalarMath.FindMostSignificantBit(highUInt128));
        Assert.IsTrue(ScalarMath.IsPowerOfTwo(16));
        Assert.IsTrue(ScalarMath.IsPowerOfTwo(highUInt128));
        Assert.IsFalse(ScalarMath.IsPowerOfTwo(-2));
        Assert.IsFalse(ScalarMath.IsPowerOfTwo(int.MinValue));
    }

    private static float SystemClamp(float value, float min, float max) =>
        System.Runtime.Intrinsics.Vector128.Clamp(
            System.Runtime.Intrinsics.Vector128.Create(value),
            System.Runtime.Intrinsics.Vector128.Create(min),
            System.Runtime.Intrinsics.Vector128.Create(max))[0];

    private static double SystemClamp(double value, double min, double max) =>
        System.Runtime.Intrinsics.Vector128.Clamp(
            System.Runtime.Intrinsics.Vector128.Create(value),
            System.Runtime.Intrinsics.Vector128.Create(min),
            System.Runtime.Intrinsics.Vector128.Create(max))[0];

    private static void AssertSameBits(float expected, float actual) =>
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));

    private static void AssertSameBits(double expected, double actual) =>
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual));

    private static void AssertSameBits(Half expected, Half actual) =>
        Assert.AreEqual(BitConverter.HalfToInt16Bits(expected), BitConverter.HalfToInt16Bits(actual));
}
