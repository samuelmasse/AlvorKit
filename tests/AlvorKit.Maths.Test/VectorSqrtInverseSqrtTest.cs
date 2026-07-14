namespace AlvorKit.Maths.Test;

/// <summary>Tests SIMD-backed single-precision vector square-root operations.</summary>
[TestClass]
public sealed class VectorSqrtInverseSqrtTest
{
    /// <summary>Square root matches the previous component-wise scalar formula for ordinary and IEEE 754 inputs.</summary>
    [TestMethod]
    public void Sqrt_MatchesScalarFormulaBits()
    {
        var smallestSubnormal = BitConverter.Int32BitsToSingle(1);
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7fc12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xffc54321));

        AssertSameBits(Vec2.Sqrt(new(4f, smallestSubnormal)), ScalarSqrt(new Vec2(4f, smallestSubnormal)));
        AssertSameBits(
            Vec3.Sqrt(new(-0f, float.PositiveInfinity, -1f)),
            ScalarSqrt(new Vec3(-0f, float.PositiveInfinity, -1f)));
        AssertSameBits(
            Vec4.Sqrt(new(0f, positivePayloadNaN, negativePayloadNaN, 9f)),
            ScalarSqrt(new Vec4(0f, positivePayloadNaN, negativePayloadNaN, 9f)));
    }

    /// <summary>Reciprocal square root matches one divided by each scalar square root without changing operation order.</summary>
    [TestMethod]
    public void InverseSqrt_MatchesScalarFormulaBits()
    {
        var smallestSubnormal = BitConverter.Int32BitsToSingle(1);
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7fc12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xffc54321));

        AssertSameBits(Vec2.InverseSqrt(new(4f, smallestSubnormal)), ScalarInverseSqrt(new Vec2(4f, smallestSubnormal)));
        AssertSameBits(
            Vec3.InverseSqrt(new(-0f, float.PositiveInfinity, -1f)),
            ScalarInverseSqrt(new Vec3(-0f, float.PositiveInfinity, -1f)));
        AssertSameBits(
            Vec4.InverseSqrt(new(0f, positivePayloadNaN, negativePayloadNaN, 9f)),
            ScalarInverseSqrt(new Vec4(0f, positivePayloadNaN, negativePayloadNaN, 9f)));
    }

    private static Vec2 ScalarSqrt(Vec2 value) =>
        new(ScalarMath.Sqrt(value.X), ScalarMath.Sqrt(value.Y));

    private static Vec3 ScalarSqrt(Vec3 value) =>
        new(ScalarMath.Sqrt(value.X), ScalarMath.Sqrt(value.Y), ScalarMath.Sqrt(value.Z));

    private static Vec4 ScalarSqrt(Vec4 value) =>
        new(ScalarMath.Sqrt(value.X), ScalarMath.Sqrt(value.Y), ScalarMath.Sqrt(value.Z), ScalarMath.Sqrt(value.W));

    private static Vec2 ScalarInverseSqrt(Vec2 value) =>
        new(ScalarMath.InverseSqrt(value.X), ScalarMath.InverseSqrt(value.Y));

    private static Vec3 ScalarInverseSqrt(Vec3 value) =>
        new(ScalarMath.InverseSqrt(value.X), ScalarMath.InverseSqrt(value.Y), ScalarMath.InverseSqrt(value.Z));

    private static Vec4 ScalarInverseSqrt(Vec4 value) =>
        new(
            ScalarMath.InverseSqrt(value.X),
            ScalarMath.InverseSqrt(value.Y),
            ScalarMath.InverseSqrt(value.Z),
            ScalarMath.InverseSqrt(value.W));

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
