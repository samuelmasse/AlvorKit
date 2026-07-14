namespace AlvorKit.Maths.Test;

/// <summary>Verifies packed float-vector equality preserves the established scalar value semantics.</summary>
[TestClass]
public sealed class FloatVectorEqualityExactnessTest
{
    private static readonly float PositivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
    private static readonly float NegativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

    /// <summary>Vec2 equality treats all NaNs and signed zero exactly like component-wise float.Equals.</summary>
    [TestMethod]
    public void Vec2_Equals_PreservesScalarSemantics()
    {
        var left = new Vec2(PositivePayloadNaN, -0f);
        var equal = new Vec2(NegativePayloadNaN, 0f);
        var different = new Vec2(NegativePayloadNaN, 1f);

        Assert.IsTrue(left.Equals(equal));
        Assert.IsTrue(left == equal);
        Assert.IsFalse(left.Equals(different));
        Assert.IsTrue(left != different);
    }

    /// <summary>Vec3 equality preserves NaN, signed-zero, infinity, and finite-lane behavior.</summary>
    [TestMethod]
    public void Vec3_Equals_PreservesScalarSemantics()
    {
        var left = new Vec3(PositivePayloadNaN, -0f, float.PositiveInfinity);
        var equal = new Vec3(NegativePayloadNaN, 0f, float.PositiveInfinity);
        var different = new Vec3(NegativePayloadNaN, 0f, float.NegativeInfinity);

        Assert.IsTrue(left.Equals(equal));
        Assert.IsTrue(left == equal);
        Assert.IsFalse(left.Equals(different));
        Assert.IsTrue(left != different);
    }

    /// <summary>Vec4 equality preserves exceptional components while checking every lane.</summary>
    [TestMethod]
    public void Vec4_Equals_PreservesScalarSemantics()
    {
        var left = new Vec4(PositivePayloadNaN, -0f, float.PositiveInfinity, 17f);
        var equal = new Vec4(NegativePayloadNaN, 0f, float.PositiveInfinity, 17f);
        var different = new Vec4(NegativePayloadNaN, 0f, float.PositiveInfinity, 19f);

        Assert.IsTrue(left.Equals(equal));
        Assert.IsTrue(left == equal);
        Assert.IsFalse(left.Equals(different));
        Assert.IsTrue(left != different);
    }
}
