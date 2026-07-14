namespace AlvorKit.Maths.Test;

/// <summary>Protects exact natural-register Vec2/Vec4 signed and unsigned 64-bit behavior.</summary>
[TestClass]
public sealed class VectorInt64SimdTest
{
    private static readonly int[] ShiftCounts = [0, 1, 63, 64, 65, 127, -1, -65];

    /// <summary>Signed arithmetic preserves unchecked Int64 wraparound for two- and four-component vectors.</summary>
    [TestMethod]
    public void SignedArithmetic_PreservesUncheckedWraparound()
    {
        var left2 = new Vec2i64(long.MaxValue, long.MinValue);
        var right2 = new Vec2i64(3, -1);
        Assert.AreEqual(new Vec2i64(unchecked(left2.X + right2.X), unchecked(left2.Y + right2.Y)), left2 + right2);
        Assert.AreEqual(new Vec2i64(unchecked(left2.X - right2.X), unchecked(left2.Y - right2.Y)), left2 - right2);
        Assert.AreEqual(new Vec2i64(unchecked(left2.X * right2.X), unchecked(left2.Y * right2.Y)), left2 * right2);
        Assert.AreEqual(new Vec2i64(unchecked(-left2.X), unchecked(-left2.Y)), -left2);

        var left4 = new Vec4i64(long.MaxValue, long.MinValue, unchecked((long)0x5555555555555555UL), -1);
        var right4 = new Vec4i64(1, -1, 3, long.MinValue);
        Assert.AreEqual(Add(left4, right4), left4 + right4);
        Assert.AreEqual(Subtract(left4, right4), left4 - right4);
        Assert.AreEqual(Multiply(left4, right4), left4 * right4);
        Assert.AreEqual(Negate(left4), -left4);
    }

    /// <summary>Unsigned arithmetic preserves unchecked UInt64 wraparound for two- and four-component vectors.</summary>
    [TestMethod]
    public void UnsignedArithmetic_PreservesUncheckedWraparound()
    {
        var left2 = new Vec2u64(ulong.MaxValue, 0);
        var right2 = new Vec2u64(3, 1);
        Assert.AreEqual(new Vec2u64(unchecked(left2.X + right2.X), unchecked(left2.Y + right2.Y)), left2 + right2);
        Assert.AreEqual(new Vec2u64(unchecked(left2.X - right2.X), unchecked(left2.Y - right2.Y)), left2 - right2);
        Assert.AreEqual(new Vec2u64(unchecked(left2.X * right2.X), unchecked(left2.Y * right2.Y)), left2 * right2);

        var left4 = new Vec4u64(ulong.MaxValue, 0, 0x5555555555555555UL, 0x8000000000000000UL);
        var right4 = new Vec4u64(1, 1, 3, 2);
        Assert.AreEqual(Add(left4, right4), left4 + right4);
        Assert.AreEqual(Subtract(left4, right4), left4 - right4);
        Assert.AreEqual(Multiply(left4, right4), left4 * right4);
    }

    /// <summary>Bitwise operations preserve every bit for signed and unsigned two- and four-component vectors.</summary>
    [TestMethod]
    public void BitwiseOperations_PreserveBoundaryPatterns()
    {
        var signed2 = new Vec2i64(unchecked((long)0xAAAAAAAAAAAAAAAAUL), long.MinValue);
        var signedOther2 = new Vec2i64(0x5555555555555555L, -1);
        Assert.AreEqual(new Vec2i64(0, long.MinValue), signed2 & signedOther2);
        Assert.AreEqual(new Vec2i64(-1, -1), signed2 | signedOther2);
        Assert.AreEqual(new Vec2i64(-1, long.MaxValue), signed2 ^ signedOther2);
        Assert.AreEqual(new Vec2i64(0x5555555555555555L, long.MaxValue), ~signed2);

        var unsigned2 = new Vec2u64(0xAAAAAAAAAAAAAAAAUL, 0x8000000000000000UL);
        var unsignedOther2 = new Vec2u64(0x5555555555555555UL, ulong.MaxValue);
        Assert.AreEqual(new Vec2u64(0, 0x8000000000000000UL), unsigned2 & unsignedOther2);
        Assert.AreEqual(new Vec2u64(ulong.MaxValue, ulong.MaxValue), unsigned2 | unsignedOther2);
        Assert.AreEqual(new Vec2u64(ulong.MaxValue, 0x7FFFFFFFFFFFFFFFUL), unsigned2 ^ unsignedOther2);
        Assert.AreEqual(new Vec2u64(0x5555555555555555UL, 0x7FFFFFFFFFFFFFFFUL), ~unsigned2);

        var signed4 = new Vec4i64(signed2.X, signed2.Y, signedOther2.X, signedOther2.Y);
        var signedOther4 = new Vec4i64(signedOther2.X, signedOther2.Y, signed2.X, signed2.Y);
        Assert.AreEqual(BitwiseAnd(signed4, signedOther4), signed4 & signedOther4);
        Assert.AreEqual(BitwiseOr(signed4, signedOther4), signed4 | signedOther4);
        Assert.AreEqual(BitwiseXor(signed4, signedOther4), signed4 ^ signedOther4);
        Assert.AreEqual(OnesComplement(signed4), ~signed4);

        var unsigned4 = new Vec4u64(unsigned2.X, unsigned2.Y, unsignedOther2.X, unsignedOther2.Y);
        var unsignedOther4 = new Vec4u64(unsignedOther2.X, unsignedOther2.Y, unsigned2.X, unsigned2.Y);
        Assert.AreEqual(BitwiseAnd(unsigned4, unsignedOther4), unsigned4 & unsignedOther4);
        Assert.AreEqual(BitwiseOr(unsigned4, unsignedOther4), unsigned4 | unsignedOther4);
        Assert.AreEqual(BitwiseXor(unsigned4, unsignedOther4), unsigned4 ^ unsignedOther4);
        Assert.AreEqual(OnesComplement(unsigned4), ~unsigned4);
    }

    /// <summary>Min, max, and clamp preserve signed and unsigned ordering at Int64 boundaries.</summary>
    [TestMethod]
    public void Bounds_PreserveSignedAndUnsignedOrdering()
    {
        var signed2Left = new Vec2i64(long.MinValue, long.MaxValue);
        var signed2Right = new Vec2i64(long.MaxValue, long.MinValue);
        Assert.AreEqual(new Vec2i64(long.MinValue), Vec2i64.Min(signed2Left, signed2Right));
        Assert.AreEqual(new Vec2i64(long.MaxValue), Vec2i64.Max(signed2Left, signed2Right));
        Assert.AreEqual(new Vec2i64(-10, 10), Vec2i64.Clamp(signed2Left, new Vec2i64(-10), new Vec2i64(10)));

        var unsigned2Left = new Vec2u64(0, ulong.MaxValue);
        var unsigned2Right = new Vec2u64(ulong.MaxValue, 0);
        Assert.AreEqual(new Vec2u64(0), Vec2u64.Min(unsigned2Left, unsigned2Right));
        Assert.AreEqual(new Vec2u64(ulong.MaxValue), Vec2u64.Max(unsigned2Left, unsigned2Right));
        Assert.AreEqual(new Vec2u64(1, ulong.MaxValue - 1),
            Vec2u64.Clamp(unsigned2Left, new Vec2u64(1), new Vec2u64(ulong.MaxValue - 1)));

        var signed4Left = new Vec4i64(long.MinValue, -1, 0, long.MaxValue);
        var signed4Right = new Vec4i64(long.MaxValue, 0, -1, long.MinValue);
        Assert.AreEqual(Min(signed4Left, signed4Right), Vec4i64.Min(signed4Left, signed4Right));
        Assert.AreEqual(Max(signed4Left, signed4Right), Vec4i64.Max(signed4Left, signed4Right));
        Assert.AreEqual(Clamp(signed4Left, new Vec4i64(-10), new Vec4i64(10)),
            Vec4i64.Clamp(signed4Left, new Vec4i64(-10), new Vec4i64(10)));

        var unsigned4Left = new Vec4u64(0, 1, 0x8000000000000000UL, ulong.MaxValue);
        var unsigned4Right = new Vec4u64(ulong.MaxValue, 0x8000000000000000UL, 1, 0);
        Assert.AreEqual(Min(unsigned4Left, unsigned4Right), Vec4u64.Min(unsigned4Left, unsigned4Right));
        Assert.AreEqual(Max(unsigned4Left, unsigned4Right), Vec4u64.Max(unsigned4Left, unsigned4Right));
        Assert.AreEqual(Clamp(unsigned4Left, new Vec4u64(1), new Vec4u64(ulong.MaxValue - 1)),
            Vec4u64.Clamp(unsigned4Left, new Vec4u64(1), new Vec4u64(ulong.MaxValue - 1)));
    }

    /// <summary>Scalar shifts use C#'s exact six-bit count mask with and without hardware intrinsics enabled.</summary>
    [TestMethod]
    public void ScalarShifts_UseCSharpCountMask()
    {
        var signed2 = new Vec2i64(long.MinValue, -1);
        var unsigned2 = new Vec2u64(0x8000000000000000UL, ulong.MaxValue);
        var signed4 = new Vec4i64(long.MinValue, -1, 0x4000000000000001L, 1);
        var unsigned4 = new Vec4u64(0x8000000000000000UL, ulong.MaxValue, 0x4000000000000001UL, 1);

        foreach (int count in ShiftCounts)
        {
            int masked = count & 63;
            Assert.AreEqual(new Vec2i64(signed2.X << masked, signed2.Y << masked), signed2 << count);
            Assert.AreEqual(new Vec2i64(signed2.X >> masked, signed2.Y >> masked), signed2 >> count);
            Assert.AreEqual(new Vec2i64(signed2.X >>> masked, signed2.Y >>> masked), signed2 >>> count);
            Assert.AreEqual(new Vec2u64(unsigned2.X << masked, unsigned2.Y << masked), unsigned2 << count);
            Assert.AreEqual(new Vec2u64(unsigned2.X >> masked, unsigned2.Y >> masked), unsigned2 >> count);
            Assert.AreEqual(new Vec2u64(unsigned2.X >>> masked, unsigned2.Y >>> masked), unsigned2 >>> count);
            Assert.AreEqual(ShiftLeft(signed4, masked), signed4 << count);
            Assert.AreEqual(ShiftRight(signed4, masked), signed4 >> count);
            Assert.AreEqual(ShiftRightLogical(signed4, masked), signed4 >>> count);
            Assert.AreEqual(ShiftLeft(unsigned4, masked), unsigned4 << count);
            Assert.AreEqual(ShiftRight(unsigned4, masked), unsigned4 >> count);
            Assert.AreEqual(ShiftRightLogical(unsigned4, masked), unsigned4 >>> count);
        }
    }

    private static Vec4i64 Add(Vec4i64 a, Vec4i64 b) =>
        new(unchecked(a.X + b.X), unchecked(a.Y + b.Y), unchecked(a.Z + b.Z), unchecked(a.W + b.W));

    private static Vec4u64 Add(Vec4u64 a, Vec4u64 b) =>
        new(unchecked(a.X + b.X), unchecked(a.Y + b.Y), unchecked(a.Z + b.Z), unchecked(a.W + b.W));

    private static Vec4i64 Subtract(Vec4i64 a, Vec4i64 b) =>
        new(unchecked(a.X - b.X), unchecked(a.Y - b.Y), unchecked(a.Z - b.Z), unchecked(a.W - b.W));

    private static Vec4u64 Subtract(Vec4u64 a, Vec4u64 b) =>
        new(unchecked(a.X - b.X), unchecked(a.Y - b.Y), unchecked(a.Z - b.Z), unchecked(a.W - b.W));

    private static Vec4i64 Multiply(Vec4i64 a, Vec4i64 b) =>
        new(unchecked(a.X * b.X), unchecked(a.Y * b.Y), unchecked(a.Z * b.Z), unchecked(a.W * b.W));

    private static Vec4u64 Multiply(Vec4u64 a, Vec4u64 b) =>
        new(unchecked(a.X * b.X), unchecked(a.Y * b.Y), unchecked(a.Z * b.Z), unchecked(a.W * b.W));

    private static Vec4i64 Negate(Vec4i64 value) =>
        new(unchecked(-value.X), unchecked(-value.Y), unchecked(-value.Z), unchecked(-value.W));

    private static Vec4i64 BitwiseAnd(Vec4i64 a, Vec4i64 b) => new(a.X & b.X, a.Y & b.Y, a.Z & b.Z, a.W & b.W);
    private static Vec4u64 BitwiseAnd(Vec4u64 a, Vec4u64 b) => new(a.X & b.X, a.Y & b.Y, a.Z & b.Z, a.W & b.W);
    private static Vec4i64 BitwiseOr(Vec4i64 a, Vec4i64 b) => new(a.X | b.X, a.Y | b.Y, a.Z | b.Z, a.W | b.W);
    private static Vec4u64 BitwiseOr(Vec4u64 a, Vec4u64 b) => new(a.X | b.X, a.Y | b.Y, a.Z | b.Z, a.W | b.W);
    private static Vec4i64 BitwiseXor(Vec4i64 a, Vec4i64 b) => new(a.X ^ b.X, a.Y ^ b.Y, a.Z ^ b.Z, a.W ^ b.W);
    private static Vec4u64 BitwiseXor(Vec4u64 a, Vec4u64 b) => new(a.X ^ b.X, a.Y ^ b.Y, a.Z ^ b.Z, a.W ^ b.W);
    private static Vec4i64 OnesComplement(Vec4i64 value) => new(~value.X, ~value.Y, ~value.Z, ~value.W);
    private static Vec4u64 OnesComplement(Vec4u64 value) => new(~value.X, ~value.Y, ~value.Z, ~value.W);
    private static Vec4i64 Min(Vec4i64 a, Vec4i64 b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z), Math.Min(a.W, b.W));
    private static Vec4u64 Min(Vec4u64 a, Vec4u64 b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z), Math.Min(a.W, b.W));
    private static Vec4i64 Max(Vec4i64 a, Vec4i64 b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z), Math.Max(a.W, b.W));
    private static Vec4u64 Max(Vec4u64 a, Vec4u64 b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z), Math.Max(a.W, b.W));
    private static Vec4i64 Clamp(Vec4i64 v, Vec4i64 min, Vec4i64 max) =>
        new(Math.Clamp(v.X, min.X, max.X), Math.Clamp(v.Y, min.Y, max.Y), Math.Clamp(v.Z, min.Z, max.Z), Math.Clamp(v.W, min.W, max.W));
    private static Vec4u64 Clamp(Vec4u64 v, Vec4u64 min, Vec4u64 max) =>
        new(Math.Clamp(v.X, min.X, max.X), Math.Clamp(v.Y, min.Y, max.Y), Math.Clamp(v.Z, min.Z, max.Z), Math.Clamp(v.W, min.W, max.W));
    private static Vec4i64 ShiftLeft(Vec4i64 v, int c) => new(v.X << c, v.Y << c, v.Z << c, v.W << c);
    private static Vec4u64 ShiftLeft(Vec4u64 v, int c) => new(v.X << c, v.Y << c, v.Z << c, v.W << c);
    private static Vec4i64 ShiftRight(Vec4i64 v, int c) => new(v.X >> c, v.Y >> c, v.Z >> c, v.W >> c);
    private static Vec4u64 ShiftRight(Vec4u64 v, int c) => new(v.X >> c, v.Y >> c, v.Z >> c, v.W >> c);
    private static Vec4i64 ShiftRightLogical(Vec4i64 v, int c) => new(v.X >>> c, v.Y >>> c, v.Z >>> c, v.W >>> c);
    private static Vec4u64 ShiftRightLogical(Vec4u64 v, int c) => new(v.X >>> c, v.Y >>> c, v.Z >>> c, v.W >>> c);
}
