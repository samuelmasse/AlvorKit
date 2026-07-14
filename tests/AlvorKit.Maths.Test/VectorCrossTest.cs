namespace AlvorKit.Maths.Test;

/// <summary>Tests the single-precision three-component cross product.</summary>
[TestClass]
public sealed class VectorCrossTest
{
    /// <summary>Cross produces the same ordinary result bits as the scalar formula and System.Numerics.</summary>
    [TestMethod]
    public void Cross_MatchesOrdinaryResultBits()
    {
        Vec3 left = (1.25f, -7.5f, 3.75f);
        Vec3 right = (-4.5f, 2.25f, 8.5f);
        var expectedScalar = ScalarCross(left, right);
        var expectedSystem = System.Numerics.Vector3.Cross(left, right);

        AssertSameBits(expectedScalar, expectedSystem);
        AssertSameBits(Vec3.Cross(left, right), expectedSystem);
    }

    /// <summary>Cross preserves System.Numerics result bits for signed zero, infinities, and payload NaNs.</summary>
    [TestMethod]
    public void Cross_MatchesSystemNumericsExceptionalResultBits()
    {
        var positivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
        var negativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));
        Vec3 left = (-0f, float.PositiveInfinity, negativePayloadNaN);
        Vec3 right = (float.NegativeInfinity, 0f, positivePayloadNaN);

        AssertSameBits(Vec3.Cross(left, right), System.Numerics.Vector3.Cross(left, right));
    }

    private static Vec3 ScalarCross(Vec3 left, Vec3 right) =>
        new(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X));

    private static void AssertSameBits(Vec3 actual, System.Numerics.Vector3 expected)
    {
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.X), BitConverter.SingleToInt32Bits(actual.X));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Y), BitConverter.SingleToInt32Bits(actual.Y));
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected.Z), BitConverter.SingleToInt32Bits(actual.Z));
    }
}
