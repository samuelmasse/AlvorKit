namespace AlvorKit.Maths.Test;

/// <summary>Tests signed integer three-component vector behavior.</summary>
[TestClass]
public sealed class Vec3iTest
{
    /// <summary>Constructors, aliases, indexing, and deconstruction expose the same component storage.</summary>
    [TestMethod]
    public void CoreMembers_Work()
    {
        var value = new Vec3i(1, 2, 3)
        {
            B = 7,
            S = 4,
            [1] = 5
        };
        value[0] = 4;
        value[2] = 7;
        var (x, y, z) = value;

        Assert.AreEqual(new Vec3i(4, 5, 7), value);
        Assert.AreEqual(new Vec3i(2), Vec3i.One + 1);
        Assert.AreEqual(Vec3i.Zero, default);
        Assert.AreEqual(4, value.R);
        Assert.AreEqual(5, value.G);
        Assert.AreEqual(5, value.T);
        Assert.AreEqual(7, value.P);
        Assert.AreEqual(4, value[0]);
        Assert.AreEqual(5, value[1]);
        Assert.AreEqual(7, value[2]);
        Assert.AreEqual((4, 5, 7), (x, y, z));
        Assert.AreEqual(Vec3i.ComponentCount, 3);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = value[-1]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => value[-1] = 0);
    }

    /// <summary>Tuple and cross-vector conversions follow widening and explicit mask rules.</summary>
    [TestMethod]
    public void Conversions_Work()
    {
        Vec3i fromTuple = (1, 2, 3);
        (int X, int Y, int Z) tuple = fromTuple;
        Vec3 widened = fromTuple;
        var fromMask = (Vec3i)new Vec3b(true, false, true);
        var fromInverseMask = (Vec3i)new Vec3b(false, true, false);
        var toMask = (Vec3b)new Vec3i(0, -2, 3);
        var toInverseMask = (Vec3b)new Vec3i(1, 0, 0);

        Assert.AreEqual((1, 2, 3), tuple);
        Assert.AreEqual(new Vec3(1f, 2f, 3f), widened);
        Assert.AreEqual(new Vec3i(1, 0, 1), fromMask);
        Assert.AreEqual(new Vec3i(0, 1, 0), fromInverseMask);
        Assert.AreEqual(new Vec3b(false, true, true), toMask);
        Assert.AreEqual(new Vec3b(true, false, false), toInverseMask);
    }

    /// <summary>Arithmetic operators support scalar and component-wise vector operations.</summary>
    [TestMethod]
    public void ArithmeticOperators_Work()
    {
        Vec3i left = (6, 8, 10);
        Vec3i right = (2, 4, 5);
        Assert.AreEqual(new Vec3i(8, 12, 15), left + right);
        Assert.AreEqual(new Vec3i(4, 4, 5), left - right);
        Assert.AreEqual(new Vec3i(12, 32, 50), left * right);
        Assert.AreEqual(new Vec3i(3, 2, 2), left / right);
        Assert.AreEqual(new Vec3i(0, 0, 0), left % right);
        Assert.AreEqual(new Vec3i(7, 9, 11), left + 1);
        Assert.AreEqual(new Vec3i(7, 9, 11), 1 + left);
        Assert.AreEqual(new Vec3i(5, 7, 9), left - 1);
        Assert.AreEqual(new Vec3i(4, 2, 0), 10 - left);
        Assert.AreEqual(new Vec3i(12, 16, 20), left * 2);
        Assert.AreEqual(new Vec3i(12, 16, 20), 2 * left);
        Assert.AreEqual(new Vec3i(3, 4, 5), left / 2);
        Assert.AreEqual(new Vec3i(12, 8, 6), 24 / new Vec3i(2, 3, 4));
        Assert.AreEqual(new Vec3i(0, 2, 1), left % 3);
        Assert.AreEqual(new Vec3i(-6, -8, -10), -left);
        Assert.AreEqual(left, +left);
        Assert.AreEqual(new Vec3i(7, 9, 11), ++left);
        Assert.AreEqual(new Vec3i(6, 8, 10), --left);
        Assert.AreEqual(new Vec3i(-7, -9, -11), ~left);
        Assert.IsTrue(left == (6, 8, 10));
        Assert.IsTrue(left != right);
    }

    /// <summary>Non-negative constant tuples resolve to the same-scalar operator without signed-unsigned ambiguity.</summary>
    [TestMethod]
    public void TupleOperands_ResolveWithoutSignedUnsignedAmbiguity()
    {
        var loc = new Vec3i(4, 5, 6);
        Assert.AreEqual(new Vec3i(4, 5, 5), loc - (0, 0, 1));
        Assert.AreEqual(new Vec3i(4, 5, 7), loc + (0, 0, 1));
        Assert.AreEqual(new Vec3b(false, false, false), loc < (1, 1, 1));
        Assert.AreEqual(new Vec3u(4u, 5u, 5u), new Vec3u(4u, 5u, 6u) - (0, 0, 1));
    }

    /// <summary>Bitwise and shift operators apply to each component independently.</summary>
    [TestMethod]
    public void BitwiseOperators_Work()
    {
        Vec3i left = (0b1100, 0b1010, 0b0110);
        Vec3i right = (0b1010, 0b0011, 0b0101);
        Assert.AreEqual(new Vec3i(0b1000, 0b0010, 0b0100), left & right);
        Assert.AreEqual(new Vec3i(0b1000, 0b1010, 0b0010), left & 0b1010);
        Assert.AreEqual(new Vec3i(0b1000, 0b1010, 0b0010), 0b1010 & left);
        Assert.AreEqual(new Vec3i(0b1110, 0b1011, 0b0111), left | right);
        Assert.AreEqual(new Vec3i(0b1101, 0b1011, 0b0111), left | 0b0001);
        Assert.AreEqual(new Vec3i(0b1101, 0b1011, 0b0111), 0b0001 | left);
        Assert.AreEqual(new Vec3i(0b0110, 0b1001, 0b0011), left ^ right);
        Assert.AreEqual(new Vec3i(0b0110, 0b0000, 0b1100), left ^ 0b1010);
        Assert.AreEqual(new Vec3i(0b0110, 0b0000, 0b1100), 0b1010 ^ left);
        Assert.AreEqual(new Vec3i(24, 20, 12), left << 1);
        Assert.AreEqual(new Vec3i(6, 5, 3), left >> 1);
    }

    /// <summary>Integer helpers return expected dot, cross, length, min, max, and comparison results.</summary>
    [TestMethod]
    public void Helpers_Work()
    {
        Vec3i value = (3, 4, 0);
        Assert.AreEqual(25, value.LengthSquared);
        Assert.AreEqual(5f, value.Length);
        Assert.AreEqual(32, Vec3i.Dot(new Vec3i(1, 2, 3), new Vec3i(4, 5, 6)));
        Assert.AreEqual(Vec3i.UnitZ, Vec3i.Cross(Vec3i.UnitX, Vec3i.UnitY));
        Assert.AreEqual(new Vec3i(2, 3, 4), Vec3i.Clamp(new Vec3i(1, 3, 9), new Vec3i(2), new Vec3i(4)));
        Assert.AreEqual(new Vec3i(2, 3, 4), Vec3i.Clamp(new Vec3i(1, 3, 9), 2, 4));
        Assert.AreEqual(25, Vec3i.DistanceSquared(Vec3i.Zero, value));
        Assert.AreEqual(5f, Vec3i.Distance(Vec3i.Zero, value));
        Assert.AreEqual(new Vec3i(1, 2, 3), Vec3i.Abs(new Vec3i(-1, -2, 3)));
        Assert.AreEqual(new Vec3b(false, true, false), Vec3i.Equal(new Vec3i(1, 3, 5), new Vec3i(2, 3, 4)));
        Assert.AreEqual(new Vec3b(true, false, true), Vec3i.NotEqual(new Vec3i(1, 3, 5), new Vec3i(2, 3, 4)));
        Assert.AreEqual(new Vec3b(true, false, false), Vec3i.LessThan(new Vec3i(1, 3, 5), new Vec3i(2, 3, 4)));
        Assert.AreEqual(new Vec3b(true, true, false), Vec3i.LessThanOrEqual(new Vec3i(1, 3, 5), new Vec3i(2, 3, 4)));
        Assert.AreEqual(new Vec3b(false, false, true), Vec3i.GreaterThan(new Vec3i(1, 3, 5), new Vec3i(2, 3, 4)));
        Assert.AreEqual(new Vec3b(false, true, true), Vec3i.GreaterThanOrEqual(new Vec3i(1, 3, 5), new Vec3i(2, 3, 4)));
    }

    /// <summary>Bit helpers expose component-wise 32-bit integer inspection without allocations.</summary>
    [TestMethod]
    public void BitHelpers_Work()
    {
        var highBit = unchecked((int)0x80000000);
        Vec3i value = (0b1011, 0, highBit);
        Assert.AreEqual(new Vec3i(3, 0, 1), Vec3i.BitCount(value));
        Assert.AreEqual(new Vec3i(28, 32, 0), Vec3i.LeadingZeroCount(value));
        Assert.AreEqual(new Vec3i(0, 32, 31), Vec3i.TrailingZeroCount(value));
        Assert.AreEqual(new Vec3i(0, -1, 31), Vec3i.FindLeastSignificantBit(value));
        Assert.AreEqual(new Vec3i(3, -1, 31), Vec3i.FindMostSignificantBit(value));
        Assert.AreEqual(new Vec3b(true, true, false), Vec3i.IsPowerOfTwo(new Vec3i(1, 16, 18)));
        Assert.AreEqual(new Vec3b(false, false, false), Vec3i.IsPowerOfTwo(new Vec3i(0, -2, highBit)));
    }

    /// <summary>Equality, hashing, and tuple-style formatting expose stable value semantics.</summary>
    [TestMethod]
    public void ValueSemantics_Work()
    {
        Vec3i value = (1, 2, 3);
        Assert.IsTrue(value.Equals((object)new Vec3i(1, 2, 3)));
        Assert.IsFalse(value.Equals(new object()));
        Assert.AreEqual(value.GetHashCode(), new Vec3i(1, 2, 3).GetHashCode());
        Assert.AreEqual("(1, 2, 3)", value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}
