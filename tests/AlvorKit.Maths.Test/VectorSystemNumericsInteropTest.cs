namespace AlvorKit.Maths.Test;

/// <summary>Tests the unmanaged layout and System.Numerics conversions of single-precision vectors.</summary>
[TestClass]
public sealed class VectorSystemNumericsInteropTest
{
    private static readonly float PositivePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0x7FC12345));
    private static readonly float NegativePayloadNaN = BitConverter.Int32BitsToSingle(unchecked((int)0xFFC54321));

    /// <summary>Vec2, Vec3, and Vec4 retain their tightly packed unmanaged sizes.</summary>
    [TestMethod]
    public unsafe void VectorSizes_AreTightlyPacked()
    {
        Assert.AreEqual(8, sizeof(Vec2));
        Assert.AreEqual(12, sizeof(Vec3));
        Assert.AreEqual(16, sizeof(Vec4));
    }

    /// <summary>Vec2 System.Numerics conversions preserve component order and exact IEEE 754 bits.</summary>
    [TestMethod]
    public void Vec2_SystemNumericsRoundTripPreservesBits()
    {
        AssertRoundTrip(new Vec2(1.25f, -123.5f));
        AssertRoundTrip(new Vec2(0f, -0f));
        AssertRoundTrip(new Vec2(float.PositiveInfinity, float.NegativeInfinity));
        AssertRoundTrip(new Vec2(PositivePayloadNaN, NegativePayloadNaN));
    }

    /// <summary>Vec3 System.Numerics conversions preserve component order and exact IEEE 754 bits.</summary>
    [TestMethod]
    public void Vec3_SystemNumericsRoundTripPreservesBits()
    {
        AssertRoundTrip(new Vec3(1.25f, -123.5f, 0.03125f));
        AssertRoundTrip(new Vec3(0f, -0f, 17f));
        AssertRoundTrip(new Vec3(float.PositiveInfinity, float.NegativeInfinity, -42f));
        AssertRoundTrip(new Vec3(PositivePayloadNaN, NegativePayloadNaN, 9f));
    }

    /// <summary>Vec4 System.Numerics conversions preserve component order and exact IEEE 754 bits.</summary>
    [TestMethod]
    public void Vec4_SystemNumericsRoundTripPreservesBits()
    {
        AssertRoundTrip(new Vec4(1.25f, -123.5f, 0.03125f, 65536f));
        AssertRoundTrip(new Vec4(0f, -0f, -0f, 0f));
        AssertRoundTrip(new Vec4(float.PositiveInfinity, float.NegativeInfinity, 19f, -23f));
        AssertRoundTrip(new Vec4(PositivePayloadNaN, NegativePayloadNaN, NegativePayloadNaN, PositivePayloadNaN));
    }

    private static void AssertRoundTrip(Vec2 value)
    {
        Vector2 system = value;
        AssertSameBits(value.X, system.X);
        AssertSameBits(value.Y, system.Y);

        Vec2 roundTrip = system;
        AssertSameBits(value.X, roundTrip.X);
        AssertSameBits(value.Y, roundTrip.Y);
    }

    private static void AssertRoundTrip(Vec3 value)
    {
        Vector3 system = value;
        AssertSameBits(value.X, system.X);
        AssertSameBits(value.Y, system.Y);
        AssertSameBits(value.Z, system.Z);

        Vec3 roundTrip = system;
        AssertSameBits(value.X, roundTrip.X);
        AssertSameBits(value.Y, roundTrip.Y);
        AssertSameBits(value.Z, roundTrip.Z);
    }

    private static void AssertRoundTrip(Vec4 value)
    {
        Vector4 system = value;
        AssertSameBits(value.X, system.X);
        AssertSameBits(value.Y, system.Y);
        AssertSameBits(value.Z, system.Z);
        AssertSameBits(value.W, system.W);

        Vec4 roundTrip = system;
        AssertSameBits(value.X, roundTrip.X);
        AssertSameBits(value.Y, roundTrip.Y);
        AssertSameBits(value.Z, roundTrip.Z);
        AssertSameBits(value.W, roundTrip.W);
    }

    private static void AssertSameBits(float expected, float actual) =>
        Assert.AreEqual(BitConverter.SingleToInt32Bits(expected), BitConverter.SingleToInt32Bits(actual));
}
