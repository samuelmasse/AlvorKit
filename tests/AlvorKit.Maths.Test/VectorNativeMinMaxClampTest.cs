namespace AlvorKit.Maths.Test;

/// <summary>Tests regular System single-precision vector minimum, maximum, and clamp operations.</summary>
[TestClass]
public sealed class VectorNativeMinMaxClampTest
{
    private static readonly float PositivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
    private static readonly float NegativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

    /// <summary>Minimum matches System.Numerics for ordinary values, NaNs, ties, signed zero, and infinities.</summary>
    [TestMethod]
    public void Min_MatchesSystemNumericsSemanticsAndBits()
    {
        AssertSameBits(Vec2.Min(new(-4f, 8f), new(2f, 3f)), new(-4f, 3f));
        AssertSameBits(
            Vec3.Min(new(PositivePayloadNaN, 0f, float.PositiveInfinity), new(4f, -0f, float.PositiveInfinity)),
            (Vec3)System.Numerics.Vector3.Min(
                new(PositivePayloadNaN, 0f, float.PositiveInfinity),
                new(4f, -0f, float.PositiveInfinity)));
        AssertSameBits(
            Vec4.Min(new(1f, float.NegativeInfinity, NegativePayloadNaN, -0f),
                new(NegativePayloadNaN, float.NegativeInfinity, 7f, 0f)),
            (Vec4)System.Numerics.Vector4.Min(
                new(1f, float.NegativeInfinity, NegativePayloadNaN, -0f),
                new(NegativePayloadNaN, float.NegativeInfinity, 7f, 0f)));
    }

    /// <summary>Maximum matches System.Numerics for ordinary values, NaNs, ties, signed zero, and infinities.</summary>
    [TestMethod]
    public void Max_MatchesSystemNumericsSemanticsAndBits()
    {
        AssertSameBits(Vec2.Max(new(-4f, 8f), new(2f, 3f)), new(2f, 8f));
        AssertSameBits(
            Vec3.Max(new(PositivePayloadNaN, -0f, float.NegativeInfinity), new(4f, 0f, float.NegativeInfinity)),
            (Vec3)System.Numerics.Vector3.Max(
                new(PositivePayloadNaN, -0f, float.NegativeInfinity),
                new(4f, 0f, float.NegativeInfinity)));
        AssertSameBits(
            Vec4.Max(new(1f, float.PositiveInfinity, NegativePayloadNaN, 0f),
                new(NegativePayloadNaN, float.PositiveInfinity, 7f, -0f)),
            (Vec4)System.Numerics.Vector4.Max(
                new(1f, float.PositiveInfinity, NegativePayloadNaN, 0f),
                new(NegativePayloadNaN, float.PositiveInfinity, 7f, -0f)));
    }

    /// <summary>Clamp follows System.Numerics result bits for ordinary values, NaNs, signed zero, and infinities.</summary>
    [TestMethod]
    public void Clamp_MatchesSystemNumericsSemanticsAndBits()
    {
        AssertSameBits(
            Vec2.Clamp(new Vec2(-4f, 8f), new Vec2(-2f, -2f), new Vec2(2f, 3f)),
            (Vec2)System.Numerics.Vector2.Clamp(new(-4f, 8f), new(-2f, -2f), new(2f, 3f)));
        AssertSameBits(
            Vec3.Clamp(
                new Vec3(PositivePayloadNaN, 5f, -5f),
                new Vec3(1f, NegativePayloadNaN, float.NegativeInfinity),
                new Vec3(2f, 3f, float.PositiveInfinity)),
            (Vec3)System.Numerics.Vector3.Clamp(
                new(PositivePayloadNaN, 5f, -5f),
                new(1f, NegativePayloadNaN, float.NegativeInfinity),
                new(2f, 3f, float.PositiveInfinity)));
        AssertSameBits(
            Vec4.Clamp(
                new Vec4(0f, -0f, float.PositiveInfinity, float.NegativeInfinity),
                new Vec4(-0f, 0f, float.NegativeInfinity, float.NegativeInfinity),
                new Vec4(float.PositiveInfinity, float.PositiveInfinity, NegativePayloadNaN, float.PositiveInfinity)),
            (Vec4)System.Numerics.Vector4.Clamp(
                new(0f, -0f, float.PositiveInfinity, float.NegativeInfinity),
                new(-0f, 0f, float.NegativeInfinity, float.NegativeInfinity),
                new(float.PositiveInfinity, float.PositiveInfinity, NegativePayloadNaN, float.PositiveInfinity)));
    }

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
