namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonArchTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        using var fixture = new RaceArchBaseContext();
        RaceArchCreate1Default.Run(fixture, count, 1);
        var after = ComparisonArchInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        using var fixture = new RaceArchBaseContext();
        RaceArchCreate2Default.Run(fixture, count, 1);
        var after = ComparisonArchInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        using var fixture = new RaceArchBaseContext();
        RaceArchCreate3Default.Run(fixture, count, 1);
        var after = ComparisonArchInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Scalar(int count, int padding)
    {
        using var fixture = new RaceArchUpdate1.ArchContext(count, padding);
        var before = ComparisonArchInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonArchInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceArchUpdate1Scalar.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1ScalarSourceGenerated.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1ScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchUpdate1.ArchContext(count, padding);
        var before = ComparisonArchInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonArchInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceArchUpdate1ScalarSourceGenerated.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Scalar(int count, int padding)
    {
        using var fixture = new RaceArchUpdate2.ArchContext(count, padding);
        var before = ComparisonArchInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonArchInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceArchUpdate2Scalar.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2ScalarSourceGenerated.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2ScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchUpdate2.ArchContext(count, padding);
        var before = ComparisonArchInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonArchInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceArchUpdate2ScalarSourceGenerated.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Scalar(int count, int padding)
    {
        using var fixture = new RaceArchUpdate3.ArchContext(count, padding);
        var before = ComparisonArchInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonArchInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceArchUpdate3Scalar.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3ScalarSourceGenerated.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3ScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchUpdate3.ArchContext(count, padding);
        var before = ComparisonArchInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonArchInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceArchUpdate3ScalarSourceGenerated.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedDefault.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedDefault(int count, int padding)
    {
        using var fixture = new RaceArchMixed.ArchContext(count);
        var before = ComparisonArchInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceArchMixedDefault.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedScalarSourceGenerated.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedScalarSourceGenerated(int count, int padding)
    {
        using var fixture = new RaceArchMixed.ArchContext(count);
        var before = ComparisonArchInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceArchMixedScalarSourceGenerated.Run(fixture, count, 2);
        var after = ComparisonArchInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

}
