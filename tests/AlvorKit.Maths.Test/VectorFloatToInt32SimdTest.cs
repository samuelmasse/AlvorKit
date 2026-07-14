namespace AlvorKit.Maths.Test;

/// <summary>Tests exact Int32 results for retained packed float-vector conversions.</summary>
[TestClass]
public sealed class VectorFloatToInt32SimdTest
{
    /// <summary>Vec3 conversions match scalar casts and rounding for ordinary and exceptional values.</summary>
    [TestMethod]
    public void Vec3Conversions_MatchScalarResults()
    {
        foreach (var value in Values3())
        {
            Assert.AreEqual(new Vec3i((int)value.X, (int)value.Y, (int)value.Z), value.TruncateToVec3i());
            Assert.AreEqual(new Vec3i((int)ScalarMath.Floor(value.X), (int)ScalarMath.Floor(value.Y), (int)ScalarMath.Floor(value.Z)),
                value.FloorToVec3i());
            Assert.AreEqual(new Vec3i((int)ScalarMath.Ceiling(value.X), (int)ScalarMath.Ceiling(value.Y), (int)ScalarMath.Ceiling(value.Z)),
                value.CeilingToVec3i());
            Assert.AreEqual(new Vec3i((int)ScalarMath.Round(value.X), (int)ScalarMath.Round(value.Y), (int)ScalarMath.Round(value.Z)),
                value.RoundToVec3i());
        }
    }

    /// <summary>Vec4 conversions match scalar casts and rounding for ordinary and exceptional values.</summary>
    [TestMethod]
    public void Vec4Conversions_MatchScalarResults()
    {
        foreach (var value in Values4())
        {
            Assert.AreEqual(new Vec4i((int)value.X, (int)value.Y, (int)value.Z, (int)value.W), value.TruncateToVec4i());
            Assert.AreEqual(new Vec4i(
                (int)ScalarMath.Floor(value.X),
                (int)ScalarMath.Floor(value.Y),
                (int)ScalarMath.Floor(value.Z),
                (int)ScalarMath.Floor(value.W)), value.FloorToVec4i());
            Assert.AreEqual(new Vec4i(
                (int)ScalarMath.Ceiling(value.X),
                (int)ScalarMath.Ceiling(value.Y),
                (int)ScalarMath.Ceiling(value.Z),
                (int)ScalarMath.Ceiling(value.W)), value.CeilingToVec4i());
            Assert.AreEqual(new Vec4i(
                (int)ScalarMath.Round(value.X),
                (int)ScalarMath.Round(value.Y),
                (int)ScalarMath.Round(value.Z),
                (int)ScalarMath.Round(value.W)), value.RoundToVec4i());
        }
    }

    private static Vec3[] Values3() =>
    [
        (-1.75f, 2.5f, -0f),
        (float.NaN, float.PositiveInfinity, float.NegativeInfinity),
        (float.MaxValue, float.MinValue, int.MaxValue),
        (int.MinValue, 0.49999997f, -0.50000006f),
    ];

    private static Vec4[] Values4() =>
    [
        (-1.75f, 2.5f, -0f, 0f),
        (float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.MaxValue),
        (float.MinValue, int.MaxValue, int.MinValue, 0.49999997f),
        (-0.50000006f, 1.5f, 2.5f, -3.5f),
    ];
}
