namespace AlvorKit.Maths.Test;

/// <summary>Tests scalar helpers shared by generated vector math.</summary>
[TestClass]
public sealed class ScalarMathTest
{
    /// <summary>Common numeric helpers preserve existing vector component semantics.</summary>
    [TestMethod]
    public void CommonHelpers_Work()
    {
        Assert.AreEqual(1f, ScalarMath.Min(float.NaN, 1f));
        Assert.IsTrue(float.IsNaN(ScalarMath.Min(1f, float.NaN)));
        Assert.AreEqual(-0f, ScalarMath.Abs(-0f));
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
}
