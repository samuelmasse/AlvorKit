namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonMorpehTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Direct.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        RaceMorpehCreate1Direct.Run(fixture, count, 1);
        var after = ComparisonMorpehInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Stash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        RaceMorpehCreate1Stash.Run(fixture, count, 1);
        var after = ComparisonMorpehInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Direct.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        RaceMorpehCreate2Direct.Run(fixture, count, 1);
        var after = ComparisonMorpehInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Stash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        RaceMorpehCreate2Stash.Run(fixture, count, 1);
        var after = ComparisonMorpehInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Direct.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        RaceMorpehCreate3Direct.Run(fixture, count, 1);
        var after = ComparisonMorpehInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Stash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehBaseContext();
        RaceMorpehCreate3Stash.Run(fixture, count, 1);
        var after = ComparisonMorpehInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Direct.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate1.MorpehContext(count, padding);
        var before = ComparisonMorpehInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonMorpehInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceMorpehUpdate1Direct.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Stash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate1.MorpehContext(count, padding);
        var before = ComparisonMorpehInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonMorpehInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceMorpehUpdate1Stash.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Direct.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate2.MorpehContext(count, padding);
        var before = ComparisonMorpehInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonMorpehInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceMorpehUpdate2Direct.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Stash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate2.MorpehContext(count, padding);
        var before = ComparisonMorpehInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonMorpehInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceMorpehUpdate2Stash.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Direct.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Direct(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate3.MorpehContext(count, padding);
        var before = ComparisonMorpehInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonMorpehInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceMorpehUpdate3Direct.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Stash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Stash(int count, int padding)
    {
        using var fixture = new RaceMorpehUpdate3.MorpehContext(count, padding);
        var before = ComparisonMorpehInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonMorpehInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceMorpehUpdate3Stash.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedDirect.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedDirect(int count, int padding)
    {
        using var fixture = new RaceMorpehMixed.MorpehContext(count);
        var before = ComparisonMorpehInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceMorpehMixedDirect.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedStash.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedStash(int count, int padding)
    {
        using var fixture = new RaceMorpehMixed.MorpehContext(count);
        var before = ComparisonMorpehInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceMorpehMixedStash.Run(fixture, count, 2);
        var after = ComparisonMorpehInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }
}
