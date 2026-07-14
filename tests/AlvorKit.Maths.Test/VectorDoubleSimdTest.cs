namespace AlvorKit.Maths.Test;

/// <summary>Tests exact semantics of fixed-width double-vector implementations.</summary>
[TestClass]
public sealed class VectorDoubleSimdTest
{
    private static readonly double PositivePayloadNaN = BitConverter.Int64BitsToDouble(unchecked((long)0x7FF8123456789ABC));
    private static readonly double NegativePayloadNaN = BitConverter.Int64BitsToDouble(unchecked((long)0xFFF8ABCDEF012345));
    private static readonly double SmallestSubnormal = BitConverter.Int64BitsToDouble(1);

    /// <summary>Vec2d addition and negation preserve scalar double result bits for finite and exceptional values.</summary>
    [TestMethod]
    public void Vec2dAdditionAndNegation_MatchScalarBits()
    {
        var left = new Vec2d(-0d, PositivePayloadNaN);
        var right = new Vec2d(SmallestSubnormal, NegativePayloadNaN);

        AssertSameBits(left + right, new(left.X + right.X, left.Y + right.Y));
        AssertSameBits(-left, new(-left.X, -left.Y));
        AssertSameBits(-new Vec2d(double.PositiveInfinity, double.NegativeInfinity),
            new(double.NegativeInfinity, double.PositiveInfinity));
    }

    /// <summary>Vec2d and Vec4d pair and scalar division preserve scalar double operation order and result bits.</summary>
    [TestMethod]
    public void Division_MatchesScalarBits()
    {
        var left2 = new Vec2d(-0d, PositivePayloadNaN);
        var right2 = new Vec2d(-3d, NegativePayloadNaN);
        AssertSameBits(left2 / right2, new(left2.X / right2.X, left2.Y / right2.Y));
        AssertSameBits(left2 / -0d, new(left2.X / -0d, left2.Y / -0d));

        var left4 = new Vec4d(SmallestSubnormal, double.PositiveInfinity, -0d, PositivePayloadNaN);
        var right4 = new Vec4d(2d, double.NegativeInfinity, -0d, NegativePayloadNaN);
        AssertSameBits(left4 / right4,
            new(left4.X / right4.X, left4.Y / right4.Y, left4.Z / right4.Z, left4.W / right4.W));
        AssertSameBits(left4 / double.NegativeInfinity,
            new(
                left4.X / double.NegativeInfinity,
                left4.Y / double.NegativeInfinity,
                left4.Z / double.NegativeInfinity,
                left4.W / double.NegativeInfinity));
    }

    /// <summary>Min, Max, and Clamp follow regular System vector semantics.</summary>
    [TestMethod]
    public void Bounds_MatchSelectedSystemSemantics()
    {
        var left2 = new Vec2d(PositivePayloadNaN, 0d);
        var right2 = new Vec2d(4d, -0d);
        AssertSameBits(Vec2d.Min(left2, right2),
            new(ScalarMath.Min(left2.X, right2.X), ScalarMath.Min(left2.Y, right2.Y)));

        var left4 = new Vec4d(NegativePayloadNaN, -0d, double.NegativeInfinity, SmallestSubnormal);
        var right4 = new Vec4d(7d, 0d, double.NegativeInfinity, -SmallestSubnormal);
        AssertSameBits(Vec4d.Max(left4, right4),
            new(
                ScalarMath.Max(left4.X, right4.X),
                ScalarMath.Max(left4.Y, right4.Y),
                ScalarMath.Max(left4.Z, right4.Z),
                ScalarMath.Max(left4.W, right4.W)));

        var value = new Vec4d(PositivePayloadNaN, -0d, double.PositiveInfinity, double.NegativeInfinity);
        var min = new Vec4d(1d, 0d, double.NegativeInfinity, double.NegativeInfinity);
        var max = new Vec4d(2d, double.PositiveInfinity, NegativePayloadNaN, double.PositiveInfinity);
        AssertSameBits(Vec4d.Clamp(value, min, max),
            new(
                ScalarMath.Clamp(value.X, min.X, max.X),
                ScalarMath.Clamp(value.Y, min.Y, max.Y),
                ScalarMath.Clamp(value.Z, min.Z, max.Z),
                ScalarMath.Clamp(value.W, min.W, max.W)));
    }

    /// <summary>Vec2d and Vec4d roots preserve scalar bits for zeros, subnormals, infinities, and payload NaNs.</summary>
    [TestMethod]
    public void Roots_MatchScalarBits()
    {
        var value2 = new Vec2d(-0d, SmallestSubnormal);
        AssertSameBits(Vec2d.Sqrt(value2), new(ScalarMath.Sqrt(value2.X), ScalarMath.Sqrt(value2.Y)));
        AssertSameBits(Vec2d.InverseSqrt(value2),
            new(ScalarMath.InverseSqrt(value2.X), ScalarMath.InverseSqrt(value2.Y)));

        var value4 = new Vec4d(double.PositiveInfinity, -1d, PositivePayloadNaN, NegativePayloadNaN);
        AssertSameBits(Vec4d.Sqrt(value4),
            new(
                ScalarMath.Sqrt(value4.X),
                ScalarMath.Sqrt(value4.Y),
                ScalarMath.Sqrt(value4.Z),
                ScalarMath.Sqrt(value4.W)));
        AssertSameBits(Vec4d.InverseSqrt(value4),
            new(
                ScalarMath.InverseSqrt(value4.X),
                ScalarMath.InverseSqrt(value4.Y),
                ScalarMath.InverseSqrt(value4.Z),
                ScalarMath.InverseSqrt(value4.W)));
    }

    /// <summary>Vec4d truncate preserves scalar result bits for signs, infinities, subnormals, and payload NaNs.</summary>
    [TestMethod]
    public void Vec4dTruncate_MatchesScalarBits()
    {
        var value = new Vec4d(-0d, -1.75d, double.PositiveInfinity, PositivePayloadNaN);
        AssertSameBits(Vec4d.Truncate(value),
            new(
                ScalarMath.Truncate(value.X),
                ScalarMath.Truncate(value.Y),
                ScalarMath.Truncate(value.Z),
                ScalarMath.Truncate(value.W)));
        var subnormal = new Vec4d(SmallestSubnormal, -SmallestSubnormal, double.NegativeInfinity, NegativePayloadNaN);
        AssertSameBits(Vec4d.Truncate(subnormal),
            new(
                ScalarMath.Truncate(subnormal.X),
                ScalarMath.Truncate(subnormal.Y),
                ScalarMath.Truncate(subnormal.Z),
                ScalarMath.Truncate(subnormal.W)));
    }

    /// <summary>Normalization retains scalar ordered length calculation and gains only through selected division operators.</summary>
    [TestMethod]
    public void Normalize_MatchesScalarOrderedFormulaBits()
    {
        var value2 = new Vec2d(3d, -4d);
        var lengthSquared2 = (value2.X * value2.X) + (value2.Y * value2.Y);
        var length2 = ScalarMath.Sqrt(lengthSquared2);
        AssertSameBits(Vec2d.Normalize(value2), new(value2.X / length2, value2.Y / length2));

        var value4 = new Vec4d(SmallestSubnormal, 2d, -3d, 6d);
        var lengthSquared4 =
            (value4.X * value4.X) + (value4.Y * value4.Y) + (value4.Z * value4.Z) + (value4.W * value4.W);
        var length4 = ScalarMath.Sqrt(lengthSquared4);
        AssertSameBits(Vec4d.Normalize(value4),
            new(value4.X / length4, value4.Y / length4, value4.Z / length4, value4.W / length4));
    }

    /// <summary>Vector-edge Step preserves ordered comparison behavior for finite values, signed zero, and NaN.</summary>
    [TestMethod]
    public void VectorStep_MatchesScalarOrderedComparisons()
    {
        var edge2 = new Vec2d(0d, PositivePayloadNaN);
        var value2 = new Vec2d(-0d, 1d);
        AssertSameBits(Vec2d.Step(edge2, value2),
            new(ScalarMath.Step(edge2.X, value2.X), ScalarMath.Step(edge2.Y, value2.Y)));

        var edge4 = new Vec4d(-1d, 0d, double.PositiveInfinity, NegativePayloadNaN);
        var value4 = new Vec4d(double.NegativeInfinity, -0d, double.PositiveInfinity, PositivePayloadNaN);
        AssertSameBits(Vec4d.Step(edge4, value4),
            new(
                ScalarMath.Step(edge4.X, value4.X),
                ScalarMath.Step(edge4.Y, value4.Y),
                ScalarMath.Step(edge4.Z, value4.Z),
                ScalarMath.Step(edge4.W, value4.W)));
    }

    private static void AssertSameBits(Vec2d actual, Vec2d expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
    }

    private static void AssertSameBits(Vec4d actual, Vec4d expected)
    {
        AssertSameBits(actual.X, expected.X);
        AssertSameBits(actual.Y, expected.Y);
        AssertSameBits(actual.Z, expected.Z);
        AssertSameBits(actual.W, expected.W);
    }

    private static void AssertSameBits(double actual, double expected) =>
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual));
}
