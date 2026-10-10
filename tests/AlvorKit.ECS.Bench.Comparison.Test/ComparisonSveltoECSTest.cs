namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonSveltoECSTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        using var fixture = new RaceSveltoECSBaseContext();
        RaceSveltoCreate1Default.Run(fixture, count, 1);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        using var fixture = new RaceSveltoECSBaseContext();
        RaceSveltoCreate2Default.Run(fixture, count, 1);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        using var fixture = new RaceSveltoECSBaseContext();
        RaceSveltoCreate3Default.Run(fixture, count, 1);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Default(int count, int padding)
    {
        using var fixture = new RaceSveltoUpdate1.SveltoECSContext(count, padding);
        var before = ComparisonSveltoECSInspection.Read(fixture.Root, 1);
        Assert.AreEqual(count * padding, ComparisonSveltoECSInspection.Padding(fixture.Root));
        before.Verify(count, 1, 0, false);
        RaceSveltoUpdate1Default.Run(fixture, count, 2);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Default(int count, int padding)
    {
        using var fixture = new RaceSveltoUpdate2.SveltoECSContext(count, padding);
        var before = ComparisonSveltoECSInspection.Read(fixture.Root, 2);
        Assert.AreEqual(count * padding, ComparisonSveltoECSInspection.Padding(fixture.Root));
        before.Verify(count, 2, 0, false);
        RaceSveltoUpdate2Default.Run(fixture, count, 2);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Default(int count, int padding)
    {
        using var fixture = new RaceSveltoUpdate3.SveltoECSContext(count, padding);
        var before = ComparisonSveltoECSInspection.Read(fixture.Root, 3);
        Assert.AreEqual(count * padding, ComparisonSveltoECSInspection.Padding(fixture.Root));
        before.Verify(count, 3, 0, false);
        RaceSveltoUpdate3Default.Run(fixture, count, 2);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedDefault.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedDefault(int count, int padding)
    {
        using var fixture = new RaceSveltoMixed.SveltoECSContext(count);
        var before = ComparisonSveltoECSInspection.Read(fixture.Root, 2);
        before.Verify(count, 2, 0, false);
        RaceSveltoMixedDefault.Run(fixture, count, 2);
        var after = ComparisonSveltoECSInspection.Read(fixture.Root, 2);
        after.Verify(count, 2, 2, false);
    }
}
