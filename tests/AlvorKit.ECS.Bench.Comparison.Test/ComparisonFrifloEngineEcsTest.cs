namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonFrifloEngineEcsTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        var fixture = new RaceFrifloCreateContext();
        RaceFrifloCreate1Default.Run(fixture, count, 1);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.Store, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        var fixture = new RaceFrifloCreateContext();
        RaceFrifloCreate2Default.Run(fixture, count, 1);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.Store, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        var fixture = new RaceFrifloCreateContext();
        RaceFrifloCreate3Default.Run(fixture, count, 1);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.Store, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Scalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate1.FrifloEngineEcsContext(count, padding);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 1);
        Assert.AreEqual(count * padding, ComparisonFrifloEngineEcsInspection.Padding(fixture.EntityStore));
        before.Verify(count, 1, 0, false);
        RaceFrifloUpdate1Scalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1SIMDScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1SIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate1.FrifloEngineEcsContext(count, padding);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 1);
        Assert.AreEqual(count * padding, ComparisonFrifloEngineEcsInspection.Padding(fixture.EntityStore));
        before.Verify(count, 1, 0, false);
        RaceFrifloUpdate1SIMDScalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Scalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate2.FrifloEngineEcsContext(count, padding);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        Assert.AreEqual(count * padding, ComparisonFrifloEngineEcsInspection.Padding(fixture.EntityStore));
        before.Verify(count, 2, 0, false);
        RaceFrifloUpdate2Scalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2SIMDScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2SIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate2.FrifloEngineEcsContext(count, padding);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        Assert.AreEqual(count * padding, ComparisonFrifloEngineEcsInspection.Padding(fixture.EntityStore));
        before.Verify(count, 2, 0, false);
        RaceFrifloUpdate2SIMDScalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Scalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Scalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate3.FrifloEngineEcsContext(count, padding);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 3);
        Assert.AreEqual(count * padding, ComparisonFrifloEngineEcsInspection.Padding(fixture.EntityStore));
        before.Verify(count, 3, 0, false);
        RaceFrifloUpdate3Scalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3SIMDScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3SIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloUpdate3.FrifloEngineEcsContext(count, padding);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 3);
        Assert.AreEqual(count * padding, ComparisonFrifloEngineEcsInspection.Padding(fixture.EntityStore));
        before.Verify(count, 3, 0, false);
        RaceFrifloUpdate3SIMDScalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedScalar(int count, int padding)
    {
        var fixture = new RaceFrifloMixed.FrifloEngineEcsContext(count);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        before.Verify(count, 2, 0, false);
        RaceFrifloMixedScalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedSIMDScalar.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedSIMDScalar(int count, int padding)
    {
        var fixture = new RaceFrifloMixed.FrifloEngineEcsContext(count);
        var before = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        before.Verify(count, 2, 0, false);
        RaceFrifloMixedSIMDScalar.Run(fixture, count, 2);
        var after = ComparisonFrifloEngineEcsInspection.Read(fixture.EntityStore, 2);
        after.Verify(count, 2, 2, false);
    }
}
