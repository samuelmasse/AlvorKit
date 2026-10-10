namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonFrentTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Default(int count, int padding)
    {
        using var fixture = new RaceFrentBaseContext();
        RaceFrentCreate1Default.Run(fixture, count, 1);
        var after = ComparisonFrentInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Default(int count, int padding)
    {
        using var fixture = new RaceFrentBaseContext();
        RaceFrentCreate2Default.Run(fixture, count, 1);
        var after = ComparisonFrentInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Default.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Default(int count, int padding)
    {
        using var fixture = new RaceFrentBaseContext();
        RaceFrentCreate3Default.Run(fixture, count, 1);
        var after = ComparisonFrentInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1QueryInline.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1QueryInline(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate1.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFrentUpdate1QueryInline.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1QueryDelegate.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1QueryDelegate(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate1.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFrentUpdate1QueryDelegate.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Simd.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Simd(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate1.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 1);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 1, 0, false);
        RaceFrentUpdate1Simd.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2QueryInline.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2QueryInline(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate2.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFrentUpdate2QueryInline.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2QueryDelegate.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2QueryDelegate(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate2.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFrentUpdate2QueryDelegate.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Simd.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Simd(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate2.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 2);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 2, 0, false);
        RaceFrentUpdate2Simd.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3QueryInline.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3QueryInline(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFrentUpdate3QueryInline.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3QueryDelegate.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3QueryDelegate(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFrentUpdate3QueryDelegate.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Simd.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Simd(int count, int padding)
    {
        using var fixture = new RaceFrentUpdate3.FrentContext(count, padding);
        var before = ComparisonFrentInspection.Read(fixture.World, 3);
        Assert.AreEqual(count * padding, ComparisonFrentInspection.Padding(fixture.World));
        before.Verify(count, 3, 0, false);
        RaceFrentUpdate3Simd.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedQueryInline.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedQueryInline(int count, int padding)
    {
        using var fixture = new RaceFrentMixed.FrentContext(count);
        var before = ComparisonFrentInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceFrentMixedQueryInline.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedSimd.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedSimd(int count, int padding)
    {
        using var fixture = new RaceFrentMixed.FrentContext(count);
        var before = ComparisonFrentInspection.Read(fixture.World, 2);
        before.Verify(count, 2, 0, false);
        RaceFrentMixedSimd.Run(fixture, count, 2);
        var after = ComparisonFrentInspection.Read(fixture.World, 2);
        after.Verify(count, 2, 2, false);
    }
}
