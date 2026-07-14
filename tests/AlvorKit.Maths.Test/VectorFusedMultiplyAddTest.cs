namespace AlvorKit.Maths.Test;

/// <summary>Tests SIMD-backed single-precision vector fused multiply-add operations.</summary>
[TestClass]
public sealed class VectorFusedMultiplyAddTest
{
    /// <summary>Fused multiply-add produces the same ordinary component values and bits as System.Numerics.</summary>
    [TestMethod]
    public void FusedMultiplyAdd_MatchesSystemNumericsForOrdinaryValues()
    {
        AssertSameBits(
            Vec2.FusedMultiplyAdd(new(2f, -3f), new(5f, 7f), new(-1f, 4f)),
            System.Numerics.Vector2.FusedMultiplyAdd(new(2f, -3f), new(5f, 7f), new(-1f, 4f)));
        AssertSameBits(
            Vec3.FusedMultiplyAdd(new(2f, -3f, 5f), new(5f, 7f, -11f), new(-1f, 4f, 13f)),
            System.Numerics.Vector3.FusedMultiplyAdd(new(2f, -3f, 5f), new(5f, 7f, -11f), new(-1f, 4f, 13f)));
        AssertSameBits(
            Vec4.FusedMultiplyAdd(new(2f, -3f, 5f, -7f), new(5f, 7f, -11f, 13f), new(-1f, 4f, 13f, 17f)),
            System.Numerics.Vector4.FusedMultiplyAdd(new(2f, -3f, 5f, -7f), new(5f, 7f, -11f, 13f), new(-1f, 4f, 13f, 17f)));
    }

    /// <summary>Fused multiply-add preserves System.Numerics bits for signed zero, subnormals, infinities, and NaN payloads.</summary>
    [TestMethod]
    public void FusedMultiplyAdd_MatchesSystemNumericsForIeeeValues()
    {
        var payloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7fc12345));
        var smallestSubnormal = BitConverter.Int32BitsToSingle(1);

        AssertSameBits(
            Vec2.FusedMultiplyAdd(new(-0f, smallestSubnormal), new(1f, 1f), new(-0f, smallestSubnormal)),
            System.Numerics.Vector2.FusedMultiplyAdd(new(-0f, smallestSubnormal), new(1f, 1f), new(-0f, smallestSubnormal)));
        AssertSameBits(
            Vec3.FusedMultiplyAdd(
                new(float.PositiveInfinity, payloadNaN, float.NegativeInfinity),
                new(2f, 1f, -0f),
                new(float.NegativeInfinity, 0f, 1f)),
            System.Numerics.Vector3.FusedMultiplyAdd(
                new(float.PositiveInfinity, payloadNaN, float.NegativeInfinity),
                new(2f, 1f, -0f),
                new(float.NegativeInfinity, 0f, 1f)));
        AssertSameBits(
            Vec4.FusedMultiplyAdd(
                new(-0f, 0f, smallestSubnormal, payloadNaN),
                new(1f, -1f, -1f, 1f),
                new(-0f, 0f, smallestSubnormal, 0f)),
            System.Numerics.Vector4.FusedMultiplyAdd(
                new(-0f, 0f, smallestSubnormal, payloadNaN),
                new(1f, -1f, -1f, 1f),
                new(-0f, 0f, smallestSubnormal, 0f)));
    }

    /// <summary>Fused multiply-add uses one rounding for cancellation and avoids a spurious intermediate overflow.</summary>
    [TestMethod]
    public void FusedMultiplyAdd_UsesOneRounding()
    {
        var actual = Vec2.FusedMultiplyAdd(
            new(4097f, float.MaxValue),
            new(4097f, 2f),
            new(-16785408f, -float.MaxValue));

        Assert.AreEqual(1f, actual.X);
        Assert.AreEqual(float.MaxValue, actual.Y);
        Assert.AreEqual(0f, (4097f * 4097f) - 16785408f);
        Assert.AreEqual(float.PositiveInfinity, (float.MaxValue * 2f) - float.MaxValue);
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
