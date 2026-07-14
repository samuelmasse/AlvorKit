namespace AlvorKit.Maths.Test;

/// <summary>Protects exact Vec4i and Vec4u behavior implemented through complete-register SIMD.</summary>
[TestClass]
public sealed class VectorInt32SimdTest
{
    private static readonly int[] ShiftCounts = [0, 1, 31, 32, 33, 63, -1, -33];

    /// <summary>Vec2i and Vec2u equal-size bitwise paths preserve every lane bit.</summary>
    [TestMethod]
    public void Vec2Int32_BitwiseOperationsPreserveEveryBit()
    {
        var signedLeft = new Vec2i(unchecked((int)0xAAAAAAAA), int.MinValue);
        var signedRight = new Vec2i(0x55555555, -1);
        Assert.AreEqual(new Vec2i(0, int.MinValue), signedLeft & signedRight);
        Assert.AreEqual(new Vec2i(-1, -1), signedLeft | signedRight);
        Assert.AreEqual(new Vec2i(-1, int.MaxValue), signedLeft ^ signedRight);
        Assert.AreEqual(new Vec2i(0x55555555, int.MaxValue), ~signedLeft);

        var unsignedLeft = new Vec2u(0xAAAAAAAAu, 0x80000000u);
        var unsignedRight = new Vec2u(0x55555555u, uint.MaxValue);
        Assert.AreEqual(new Vec2u(0u, 0x80000000u), unsignedLeft & unsignedRight);
        Assert.AreEqual(new Vec2u(uint.MaxValue, uint.MaxValue), unsignedLeft | unsignedRight);
        Assert.AreEqual(new Vec2u(uint.MaxValue, 0x7FFFFFFFu), unsignedLeft ^ unsignedRight);
        Assert.AreEqual(new Vec2u(0x55555555u, 0x7FFFFFFFu), ~unsignedLeft);
    }

    /// <summary>Selected direct Vec3 bounds preserve signed and unsigned boundary ordering.</summary>
    [TestMethod]
    public void Vec3Int32_SelectedBoundsPreserveOrdering()
    {
        Assert.AreEqual(
            new Vec3i(int.MinValue, -1, int.MinValue),
            Vec3i.Min(new Vec3i(int.MinValue, -1, int.MaxValue), new Vec3i(int.MaxValue, 0, int.MinValue)));
        Assert.AreEqual(
            new Vec3u(uint.MaxValue, 0x80000000u, uint.MaxValue),
            Vec3u.Max(new Vec3u(0u, 0x80000000u, uint.MaxValue), new Vec3u(uint.MaxValue, 1u, 0u)));
    }

    /// <summary>Vec4i arithmetic retains unchecked Int32 overflow and negation behavior.</summary>
    [TestMethod]
    public void Vec4i_ArithmeticPreservesUncheckedWraparound()
    {
        var left = new Vec4i(int.MaxValue, int.MinValue, unchecked((int)0x55555555), -1);
        var right = new Vec4i(1, -1, 3, int.MinValue);

        Assert.AreEqual(new Vec4i(int.MinValue, int.MaxValue, unchecked((int)0x55555558), int.MaxValue), left + right);
        Assert.AreEqual(new Vec4i(int.MaxValue - 1, int.MinValue + 1, unchecked((int)0x55555552), int.MaxValue), left - right);
        Assert.AreEqual(new Vec4i(int.MaxValue, int.MinValue, -1, int.MinValue), left * right);
        Assert.AreEqual(new Vec4i(-int.MaxValue, int.MinValue, unchecked((int)0xAAAAAAAB), 1), -left);
    }

    /// <summary>Vec4u arithmetic retains unchecked UInt32 wraparound behavior.</summary>
    [TestMethod]
    public void Vec4u_ArithmeticPreservesUncheckedWraparound()
    {
        var left = new Vec4u(uint.MaxValue, 0u, 0x55555555u, 0x80000000u);
        var right = new Vec4u(1u, 1u, 3u, 2u);

        Assert.AreEqual(new Vec4u(0u, 1u, 0x55555558u, 0x80000002u), left + right);
        Assert.AreEqual(new Vec4u(uint.MaxValue - 1, uint.MaxValue, 0x55555552u, 0x7FFFFFFEu), left - right);
        Assert.AreEqual(new Vec4u(uint.MaxValue, 0u, 0xFFFFFFFFu, 0u), left * right);
    }

    /// <summary>Vec4i and Vec4u bitwise operators preserve alternating-bit and boundary patterns.</summary>
    [TestMethod]
    public void Vec4Int32_BitwiseOperationsPreserveEveryBit()
    {
        var signedLeft = new Vec4i(unchecked((int)0xAAAAAAAA), 0x55555555, int.MinValue, int.MaxValue);
        var signedRight = new Vec4i(0x55555555, unchecked((int)0xAAAAAAAA), -1, 1);
        Assert.AreEqual(new Vec4i(0, 0, int.MinValue, 1), signedLeft & signedRight);
        Assert.AreEqual(new Vec4i(-1, -1, -1, int.MaxValue), signedLeft | signedRight);
        Assert.AreEqual(new Vec4i(-1, -1, int.MaxValue, int.MaxValue - 1), signedLeft ^ signedRight);
        Assert.AreEqual(new Vec4i(0x55555555, unchecked((int)0xAAAAAAAA), int.MaxValue, int.MinValue), ~signedLeft);

        var unsignedLeft = new Vec4u(0xAAAAAAAAu, 0x55555555u, 0x80000000u, 0x7FFFFFFFu);
        var unsignedRight = new Vec4u(0x55555555u, 0xAAAAAAAAu, uint.MaxValue, 1u);
        Assert.AreEqual(new Vec4u(0u, 0u, 0x80000000u, 1u), unsignedLeft & unsignedRight);
        Assert.AreEqual(new Vec4u(uint.MaxValue, uint.MaxValue, uint.MaxValue, 0x7FFFFFFFu), unsignedLeft | unsignedRight);
        Assert.AreEqual(new Vec4u(uint.MaxValue, uint.MaxValue, 0x7FFFFFFFu, 0x7FFFFFFEu), unsignedLeft ^ unsignedRight);
        Assert.AreEqual(new Vec4u(0x55555555u, 0xAAAAAAAAu, 0x7FFFFFFFu, 0x80000000u), ~unsignedLeft);
    }

    /// <summary>Vec4i and Vec4u min, max, and clamp preserve signed and unsigned ordering at their boundaries.</summary>
    [TestMethod]
    public void Vec4Int32_BoundsPreserveSignedAndUnsignedOrdering()
    {
        var signedLeft = new Vec4i(int.MinValue, -1, 0, int.MaxValue);
        var signedRight = new Vec4i(int.MaxValue, 0, -1, int.MinValue);
        Assert.AreEqual(new Vec4i(int.MinValue, -1, -1, int.MinValue), Vec4i.Min(signedLeft, signedRight));
        Assert.AreEqual(new Vec4i(int.MaxValue, 0, 0, int.MaxValue), Vec4i.Max(signedLeft, signedRight));
        Assert.AreEqual(new Vec4i(-10, -1, 0, 10), Vec4i.Clamp(signedLeft, new Vec4i(-10), new Vec4i(10)));

        var unsignedLeft = new Vec4u(0u, 1u, 0x80000000u, uint.MaxValue);
        var unsignedRight = new Vec4u(uint.MaxValue, 0x80000000u, 1u, 0u);
        Assert.AreEqual(new Vec4u(0u, 1u, 1u, 0u), Vec4u.Min(unsignedLeft, unsignedRight));
        Assert.AreEqual(new Vec4u(uint.MaxValue, 0x80000000u, 0x80000000u, uint.MaxValue), Vec4u.Max(unsignedLeft, unsignedRight));
        Assert.AreEqual(new Vec4u(1u, 1u, 0x80000000u, 0xFFFFFFFEu),
            Vec4u.Clamp(unsignedLeft, new Vec4u(1u), new Vec4u(0xFFFFFFFEu)));
    }

    /// <summary>Scalar-count shifts use the exact C# Int32 count mask for positive, large, and negative counts.</summary>
    [TestMethod]
    public void Vec4Int32_ScalarShiftsUseCSharpCountMask()
    {
        var signed = new Vec4i(int.MinValue, -1, 0x40000001, 1);
        var unsigned = new Vec4u(0x80000000u, uint.MaxValue, 0x40000001u, 1u);

        foreach (var count in ShiftCounts)
        {
            var masked = count & 31;
            Assert.AreEqual(new Vec4i(signed.X << masked, signed.Y << masked, signed.Z << masked, signed.W << masked), signed << count, $"Vec4i << {count}");
            Assert.AreEqual(new Vec4i(signed.X >> masked, signed.Y >> masked, signed.Z >> masked, signed.W >> masked), signed >> count, $"Vec4i >> {count}");
            Assert.AreEqual(new Vec4i(signed.X >>> masked, signed.Y >>> masked, signed.Z >>> masked, signed.W >>> masked), signed >>> count, $"Vec4i >>> {count}");
            Assert.AreEqual(new Vec4u(unsigned.X << masked, unsigned.Y << masked, unsigned.Z << masked, unsigned.W << masked), unsigned << count,
                $"Vec4u << {count}");
            Assert.AreEqual(new Vec4u(unsigned.X >> masked, unsigned.Y >> masked, unsigned.Z >> masked, unsigned.W >> masked), unsigned >> count,
                $"Vec4u >> {count}");
            Assert.AreEqual(new Vec4u(unsigned.X >>> masked, unsigned.Y >>> masked, unsigned.Z >>> masked, unsigned.W >>> masked), unsigned >>> count,
                $"Vec4u >>> {count}");
        }
    }
}
