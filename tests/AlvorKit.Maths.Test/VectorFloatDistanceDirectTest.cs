namespace AlvorKit.Maths.Test;

/// <summary>Tests exact ordered semantics of direct float-vector distance kernels.</summary>
[TestClass]
public sealed class VectorFloatDistanceDirectTest
{
    private static readonly float PayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));

    /// <summary>Vec2, Vec3, and Vec4 distance results match the original subtract-then-dot ordering bit-for-bit.</summary>
    [TestMethod]
    public void Distance_MatchesSubtractThenDotBits()
    {
        AssertDistance(new Vec2(PayloadNaN, -0f), new Vec2(3f, 0f));
        AssertDistance(new Vec2(float.PositiveInfinity, float.NegativeInfinity), new Vec2(float.PositiveInfinity, 1f));
        AssertDistance(new Vec3(PayloadNaN, float.Epsilon, -0f), new Vec3(-2f, -float.Epsilon, 0f));
        AssertDistance(new Vec3(float.MaxValue, float.MinValue, 1.5f), new Vec3(-float.MaxValue, 0f, -2.5f));
        AssertDistance(new Vec4(PayloadNaN, -0f, float.PositiveInfinity, float.Epsilon), new Vec4(7f, 0f, 1f, -float.Epsilon));
        AssertDistance(new Vec4(float.MaxValue, float.MinValue, 1.5f, -2.5f), new Vec4(-float.MaxValue, 0f, -2.5f, 3.5f));
    }

    private static void AssertDistance(Vec2 left, Vec2 right)
    {
        var delta = left - right;
        var squared = (delta.X * delta.X) + (delta.Y * delta.Y);
        AssertBits(squared, Vec2.DistanceSquared(left, right));
        AssertBits(ScalarMath.Sqrt(squared), Vec2.Distance(left, right));
    }

    private static void AssertDistance(Vec3 left, Vec3 right)
    {
        var delta = left - right;
        var squared = (delta.X * delta.X) + (delta.Y * delta.Y) + (delta.Z * delta.Z);
        AssertBits(squared, Vec3.DistanceSquared(left, right));
        AssertBits(ScalarMath.Sqrt(squared), Vec3.Distance(left, right));
    }

    private static void AssertDistance(Vec4 left, Vec4 right)
    {
        var delta = left - right;
        var squared = (delta.X * delta.X) + (delta.Y * delta.Y) + (delta.Z * delta.Z) + (delta.W * delta.W);
        AssertBits(squared, Vec4.DistanceSquared(left, right));
        AssertBits(ScalarMath.Sqrt(squared), Vec4.Distance(left, right));
    }

    private static void AssertBits(float expected, float actual) =>
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
