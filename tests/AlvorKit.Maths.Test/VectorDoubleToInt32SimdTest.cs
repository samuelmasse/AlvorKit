namespace AlvorKit.Maths.Test;

/// <summary>Tests exact results of retained double-to-Int32 conversion kernels.</summary>
[TestClass]
public sealed class VectorDoubleToInt32SimdTest
{
    /// <summary>Vec2d and Vec4d conversions match scalar saturation, NaN, and rounding behavior.</summary>
    [TestMethod]
    public void Conversions_MatchScalarResults()
    {
        foreach (var value in Values2())
        {
            var truncated = new Vec2i((int)value.X, (int)value.Y);
            Assert.AreEqual(truncated, value.TruncateToVec2i());
            Assert.AreEqual(truncated, (Vec2i)value);
            Assert.AreEqual(new Vec2i((int)ScalarMath.Floor(value.X), (int)ScalarMath.Floor(value.Y)), value.FloorToVec2i());
            Assert.AreEqual(new Vec2i((int)ScalarMath.Ceiling(value.X), (int)ScalarMath.Ceiling(value.Y)), value.CeilingToVec2i());
            Assert.AreEqual(new Vec2i((int)ScalarMath.Round(value.X), (int)ScalarMath.Round(value.Y)), value.RoundToVec2i());
        }

        foreach (var value in Values4())
        {
            var truncated = new Vec4i((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
            Assert.AreEqual(truncated, value.TruncateToVec4i());
            Assert.AreEqual(truncated, (Vec4i)value);
            Assert.AreEqual(Map(value, ScalarMath.Floor), value.FloorToVec4i());
            Assert.AreEqual(Map(value, ScalarMath.Ceiling), value.CeilingToVec4i());
            Assert.AreEqual(Map(value, ScalarMath.Round), value.RoundToVec4i());
        }
    }

    private static Vec4i Map(Vec4d value, Func<double, double> function) =>
        new((int)function(value.X), (int)function(value.Y), (int)function(value.Z), (int)function(value.W));

    private static Vec2d[] Values2() =>
    [
        (double.NaN, double.PositiveInfinity),
        (double.NegativeInfinity, double.MaxValue),
        ((double)int.MaxValue + 1d, (double)int.MinValue - 1d),
        (-0d, 2.5d),
        (-2.5d, 123.875d),
    ];

    private static Vec4d[] Values4() =>
    [
        (double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue),
        ((double)int.MaxValue + 1d, (double)int.MinValue - 1d, int.MaxValue, int.MinValue),
        (-0d, 0.5d, 1.5d, 2.5d),
        (-0.5d, -1.5d, -2.5d, 123.875d),
    ];
}
