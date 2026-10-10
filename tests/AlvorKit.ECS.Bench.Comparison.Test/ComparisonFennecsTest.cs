namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonFennecsTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        using var fixture = new RaceFennecsBaseContext();
        RaceFennecsCreate1Default.Run(fixture, count, 1);
        var after = ComparisonFennecsInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        using var fixture = new RaceFennecsBaseContext();
        RaceFennecsCreate2Default.Run(fixture, count, 1);
        var after = ComparisonFennecsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        using var fixture = new RaceFennecsBaseContext();
        RaceFennecsCreate3Default.Run(fixture, count, 1);
        var after = ComparisonFennecsInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1ForEach.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1ForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate1.FennecsContext(count, padding);
        var before = ComparisonFennecsInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFennecsInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFennecsUpdate1ForEach.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Raw.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Raw(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate1.FennecsContext(count, padding);
        var before = ComparisonFennecsInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFennecsInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFennecsUpdate1Raw.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2ForEach.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2ForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate2.FennecsContext(count, padding);
        var before = ComparisonFennecsInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFennecsInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFennecsUpdate2ForEach.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Raw.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Raw(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate2.FennecsContext(count, padding);
        var before = ComparisonFennecsInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFennecsInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFennecsUpdate2Raw.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3ForEach.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3ForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate3.FennecsContext(count, padding);
        var before = ComparisonFennecsInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFennecsInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFennecsUpdate3ForEach.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Raw.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Raw(int count, int padding)
    {
        using var fixture = new RaceFennecsUpdate3.FennecsContext(count, padding);
        var before = ComparisonFennecsInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFennecsInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFennecsUpdate3Raw.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedForEach.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedForEach(int count, int padding)
    {
        using var fixture = new RaceFennecsMixed.FennecsContext(count);
        var before = ComparisonFennecsInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceFennecsMixedForEach.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedRaw.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedRaw(int count, int padding)
    {
        using var fixture = new RaceFennecsMixed.FennecsContext(count);
        var before = ComparisonFennecsInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceFennecsMixedRaw.Run(fixture, count, 2);
        var after = ComparisonFennecsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }
}
