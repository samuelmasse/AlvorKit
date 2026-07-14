namespace AlvorKit.Maths.Test;

/// <summary>Tests SIMD-backed single-precision vector saturation and step operations.</summary>
[TestClass]
public sealed class VectorSaturateStepTest
{
    private static readonly float PositivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
    private static readonly float NegativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

    /// <summary>Saturate follows regular System.Numerics clamp bits for boundaries, infinities, signed zero, and payload NaNs.</summary>
    [TestMethod]
    public void Saturate_MatchesSystemNumericsClampSemanticsAndBits()
    {
        AssertSameBits(Vec2.Saturate(new(float.NegativeInfinity, float.PositiveInfinity)),
            (Vec2)System.Numerics.Vector2.Clamp(new(float.NegativeInfinity, float.PositiveInfinity),
                System.Numerics.Vector2.Zero, System.Numerics.Vector2.One));
        AssertSameBits(Vec3.Saturate(new(-2f, 0.5f, 2f)),
            (Vec3)System.Numerics.Vector3.Clamp(new(-2f, 0.5f, 2f), System.Numerics.Vector3.Zero, System.Numerics.Vector3.One));
        AssertSameBits(Vec4.Saturate(new(-0f, 0f, 1f, PositivePayloadNaN)),
            (Vec4)System.Numerics.Vector4.Clamp(new(-0f, 0f, 1f, PositivePayloadNaN),
                System.Numerics.Vector4.Zero, System.Numerics.Vector4.One));
    }

    /// <summary>Vector-edge step returns zero below the edge and one for equality, NaNs, and matching infinities.</summary>
    [TestMethod]
    public void Step_VectorEdgePreservesComparisonSemanticsAndBits()
    {
        AssertSameBits(Vec2.Step(new Vec2(0f, -0f), new Vec2(-0f, 0f)), new(1f, 1f));
        AssertSameBits(Vec3.Step(new Vec3(0f, 1f, 2f), new Vec3(-1f, 1f, 3f)), new(0f, 1f, 1f));
        AssertSameBits(
            Vec4.Step(
                new Vec4(PositivePayloadNaN, 0f, float.NegativeInfinity, float.PositiveInfinity),
                new Vec4(NegativePayloadNaN, float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity)),
            new(1f, 0f, 1f, 1f));
    }

    /// <summary>Scalar-edge step splats the edge without changing signed-zero, infinity, or payload-NaN comparison results.</summary>
    [TestMethod]
    public void Step_ScalarEdgePreservesComparisonSemanticsAndBits()
    {
        AssertSameBits(Vec2.Step(0f, new(NegativePayloadNaN, -1f)), new(1f, 0f));
        AssertSameBits(Vec2.Step(PositivePayloadNaN, new(float.NegativeInfinity, float.PositiveInfinity)), new(1f, 1f));
        AssertSameBits(Vec3.Step(-0f, new(-0f, 0f, 1f)), new(1f, 1f, 1f));
        AssertSameBits(
            Vec4.Step(0f, new(float.NegativeInfinity, -0f, 0f, float.PositiveInfinity)),
            new(0f, 1f, 1f, 1f));
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
