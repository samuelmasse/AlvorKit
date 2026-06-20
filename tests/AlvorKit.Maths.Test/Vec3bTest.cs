namespace AlvorKit.Maths.Test;

/// <summary>Tests Boolean three-component vector mask behavior.</summary>
[TestClass]
public sealed class Vec3bTest
{
    /// <summary>Constructors, aliases, indexing, deconstruction, and aggregate properties expose mask state.</summary>
    [TestMethod]
    public void CoreMembers_Work()
    {
        var value = new Vec3b(true, false, true)
        {
            G = true,
            P = false,
            [0] = false
        };
        value[1] = true;
        value[2] = false;
        var (x, y, z) = value;

        Assert.AreEqual(new Vec3b(false, true, false), value);
        Assert.AreEqual(Vec3b.True, new Vec3b(true));
        Assert.AreEqual(Vec3b.False, default);
        Assert.IsTrue(Vec3b.True.All);
        Assert.IsFalse(Vec3b.False.Any);
        Assert.IsTrue(Vec3b.False.None);
        Assert.IsFalse(Vec3b.True.None);
        Assert.IsFalse(Vec3b.False.All);
        Assert.IsTrue(value.G);
        Assert.IsTrue(value.T);
        Assert.IsFalse(value.R);
        Assert.IsFalse(value.B);
        Assert.IsFalse(value.S);
        Assert.IsFalse(value.P);
        Assert.IsFalse(value[0]);
        Assert.IsTrue(value[1]);
        Assert.IsFalse(value[2]);
        Assert.AreEqual((false, true, false), (x, y, z));
        Assert.IsFalse(value.All);
        Assert.IsTrue(value.Any);
        Assert.IsFalse(value.None);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => value[3] = true);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _ = value[3]);
    }

    /// <summary>Tuple conversions preserve Boolean components without ceremony.</summary>
    [TestMethod]
    public void TupleConversions_Work()
    {
        Vec3b fromTuple = (true, false, true);
        (bool X, bool Y, bool Z) tuple = fromTuple;

        Assert.AreEqual(new Vec3b(true, false, true), fromTuple);
        Assert.AreEqual((true, false, true), tuple);
    }

    /// <summary>Logical operators apply to each component independently.</summary>
    [TestMethod]
    public void LogicalOperators_Work()
    {
        var left = new Vec3b(true, false, true);
        var right = new Vec3b(true, true, false);

        Assert.AreEqual(new Vec3b(false, true, false), !left);
        Assert.AreEqual(new Vec3b(true, false, false), left & right);
        Assert.AreEqual(new Vec3b(true, false, true), true & left);
        Assert.AreEqual(new Vec3b(true, true, true), left | right);
        Assert.AreEqual(new Vec3b(false, true, true), left ^ right);
        Assert.AreEqual(new Vec3b(false, true, false), left ^ true);
        Assert.AreEqual(new Vec3b(false, true, false), true ^ left);
        Assert.AreEqual(new Vec3b(true, false, true), left & true);
        Assert.AreEqual(new Vec3b(true, true, true), true | left);
        Assert.AreEqual(new Vec3b(true, false, false), Vec3b.Equal(left, right));
        Assert.AreEqual(new Vec3b(false, true, true), Vec3b.NotEqual(left, right));
        Assert.IsTrue(left == new Vec3b(true, false, true));
        Assert.IsTrue(left != right);
    }

    /// <summary>Mask selection chooses each output component from the matching source vector.</summary>
    [TestMethod]
    public void Select_ChoosesComponents()
    {
        var mask = new Vec3b(true, false, true);
        var inverse = new Vec3b(false, true, false);

        Assert.AreEqual(new Vec3(1f, 20f, 3f), mask.Select(new Vec3(1f, 2f, 3f), new Vec3(10f, 20f, 30f)));
        Assert.AreEqual(new Vec3d(1d, 20d, 3d), mask.Select(new Vec3d(1d, 2d, 3d), new Vec3d(10d, 20d, 30d)));
        Assert.AreEqual(new Vec3i(1, 20, 3), mask.Select(new Vec3i(1, 2, 3), new Vec3i(10, 20, 30)));
        Assert.AreEqual(
            new Vec3u128((UInt128)1, (UInt128)20, (UInt128)3),
            mask.Select(
                new Vec3u128((UInt128)1, (UInt128)2, (UInt128)3),
                new Vec3u128((UInt128)10, (UInt128)20, (UInt128)30)));
        Assert.AreEqual(new Vec3b(true, true, false), mask.Select(new Vec3b(true, false, false), new Vec3b(false, true, true)));
        Assert.AreEqual(new Vec3(10f, 2f, 30f), inverse.Select(new Vec3(1f, 2f, 3f), new Vec3(10f, 20f, 30f)));
        Assert.AreEqual(new Vec3i(10, 2, 30), inverse.Select(new Vec3i(1, 2, 3), new Vec3i(10, 20, 30)));
    }

    /// <summary>Equality, hashing, and tuple-style formatting expose stable value semantics.</summary>
    [TestMethod]
    public void ValueSemantics_Work()
    {
        var value = new Vec3b(true, false, true);

        Assert.IsTrue(value.Equals((object)new Vec3b(true, false, true)));
        Assert.IsFalse(value.Equals(new object()));
        Assert.AreEqual(value.GetHashCode(), new Vec3b(true, false, true).GetHashCode());
        Assert.AreEqual("(True, False, True)", value.ToString());
    }
}
