namespace AlvorKit.Maths.Test;

/// <summary>Tests regular System vector semantics of packed double-vector Saturate.</summary>
[TestClass]
public sealed class VectorDoubleSaturateSimdTest
{
    private static readonly double PayloadNaN = BitConverter.Int64BitsToDouble(unchecked((long)0x7FF8123456789ABC));

    /// <summary>Vec2d, Vec3d, and Vec4d match regular System clamp component bits.</summary>
    [TestMethod]
    public void Saturate_MatchesSystemClampBits()
    {
        AssertBits(new Vec2d(PayloadNaN, -0d));
        AssertBits(new Vec2d(double.NegativeInfinity, double.PositiveInfinity));
        AssertBits(new Vec3d(PayloadNaN, -0d, 1d));
        AssertBits(new Vec3d(-1d, 0.5d, double.PositiveInfinity));
        AssertBits(new Vec4d(PayloadNaN, -0d, double.NegativeInfinity, double.PositiveInfinity));
        AssertBits(new Vec4d(-1d, 0d, 0.5d, 1d));
    }

    private static void AssertBits(Vec2d value) =>
        AssertBits(new(ScalarMath.Saturate(value.X), ScalarMath.Saturate(value.Y)), Vec2d.Saturate(value));

    private static void AssertBits(Vec3d value) =>
        AssertBits(new(
            ScalarMath.Saturate(value.X),
            ScalarMath.Saturate(value.Y),
            ScalarMath.Saturate(value.Z)), Vec3d.Saturate(value));

    private static void AssertBits(Vec4d value) =>
        AssertBits(new(
            ScalarMath.Saturate(value.X),
            ScalarMath.Saturate(value.Y),
            ScalarMath.Saturate(value.Z),
            ScalarMath.Saturate(value.W)), Vec4d.Saturate(value));

    private static void AssertBits(Vec2d expected, Vec2d actual)
    {
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.X), BitConverter.DoubleToInt64Bits(actual.X));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Y), BitConverter.DoubleToInt64Bits(actual.Y));
    }

    private static void AssertBits(Vec3d expected, Vec3d actual)
    {
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.X), BitConverter.DoubleToInt64Bits(actual.X));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Y), BitConverter.DoubleToInt64Bits(actual.Y));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Z), BitConverter.DoubleToInt64Bits(actual.Z));
    }

    private static void AssertBits(Vec4d expected, Vec4d actual)
    {
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.X), BitConverter.DoubleToInt64Bits(actual.X));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Y), BitConverter.DoubleToInt64Bits(actual.Y));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Z), BitConverter.DoubleToInt64Bits(actual.Z));
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.W), BitConverter.DoubleToInt64Bits(actual.W));
    }
}
