namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonFlecsNetTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        using var fixture = new RaceFlecsNetBaseContext();
        RaceFlecsNetCreate1Default.Run(fixture, count, 1);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        using var fixture = new RaceFlecsNetBaseContext();
        RaceFlecsNetCreate2Default.Run(fixture, count, 1);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        using var fixture = new RaceFlecsNetBaseContext();
        RaceFlecsNetCreate3Default.Run(fixture, count, 1);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Each.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Each(int count, int padding)
    {
        using var fixture = new RaceFlecsNetUpdate1.FlecsContext(count, padding);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFlecsNetInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFlecsNetUpdate1Each.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Iter.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Iter(int count, int padding)
    {
        using var fixture = new RaceFlecsNetUpdate1.FlecsContext(count, padding);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFlecsNetInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFlecsNetUpdate1Iter.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Each.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Each(int count, int padding)
    {
        using var fixture = new RaceFlecsNetUpdate2.FlecsContext(count, padding);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFlecsNetInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFlecsNetUpdate2Each.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Iter.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Iter(int count, int padding)
    {
        using var fixture = new RaceFlecsNetUpdate2.FlecsContext(count, padding);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFlecsNetInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFlecsNetUpdate2Iter.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Each.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Each(int count, int padding)
    {
        using var fixture = new RaceFlecsNetUpdate3.FlecsContext(count, padding);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFlecsNetInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFlecsNetUpdate3Each.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Iter.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Iter(int count, int padding)
    {
        using var fixture = new RaceFlecsNetUpdate3.FlecsContext(count, padding);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFlecsNetInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFlecsNetUpdate3Iter.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedEach.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedEach(int count, int padding)
    {
        using var fixture = new RaceFlecsNetMixed.FlecsContext(count);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceFlecsNetMixedEach.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedIter.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedIter(int count, int padding)
    {
        using var fixture = new RaceFlecsNetMixed.FlecsContext(count);
        var before = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceFlecsNetMixedIter.Run(fixture, count, 2);
        var after = ComparisonFlecsNetInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }
}
