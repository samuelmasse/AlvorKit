namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonDefaultEcsTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsBaseContext();
        RaceDefaultEcsCreate1Default.Run(fixture, count, 1);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsBaseContext();
        RaceDefaultEcsCreate2Default.Run(fixture, count, 1);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsBaseContext();
        RaceDefaultEcsCreate3Default.Run(fixture, count, 1);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1ComponentSystemScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1ComponentSystemScalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate1.DefaultEcsContext(count, padding);
        var before = ComparisonDefaultEcsInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonDefaultEcsInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceDefaultEcsUpdate1ComponentSystemScalar.Run(fixture, count, 2);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1EntitySetSystemScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1EntitySetSystemScalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate1.DefaultEcsContext(count, padding);
        var before = ComparisonDefaultEcsInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonDefaultEcsInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceDefaultEcsUpdate1EntitySetSystemScalar.Run(fixture, count, 2);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Scalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate2.DefaultEcsContext(count, padding);
        var before = ComparisonDefaultEcsInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonDefaultEcsInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceDefaultEcsUpdate2Scalar.Run(fixture, count, 2);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Scalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsUpdate3.DefaultEcsContext(count, padding);
        var before = ComparisonDefaultEcsInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonDefaultEcsInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceDefaultEcsUpdate3Scalar.Run(fixture, count, 2);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedScalar(int count, int padding)
    {
        using var fixture = new RaceDefaultEcsMixed.DefaultEcsContext(count);
        var before = ComparisonDefaultEcsInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceDefaultEcsMixedScalar.Run(fixture, count, 2);
        var after = ComparisonDefaultEcsInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

}
