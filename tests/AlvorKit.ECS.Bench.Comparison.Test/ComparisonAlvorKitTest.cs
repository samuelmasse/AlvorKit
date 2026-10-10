namespace AlvorKit;

[TestClass]
[DoNotParallelize]
public class ComparisonAlvorKitTest
{
    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1Sparse.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1Sparse(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var last = RaceAlvorCreate1Sparse.Run(fixture, count, 1);
        Assert.AreEqual(count, fixture.Arena.Allocated);
        Assert.IsTrue(last.HasSparseComponent1);
        Assert.AreEqual(0, last.SparseComponent1);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1ArchetypalSetters.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1ArchetypalSetters(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        RaceAlvorCreate1ArchetypalSetters.Run(fixture, count, 1);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create1ArchetypalFinalShape.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create1ArchetypalFinalShape(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        RaceAlvorCreate1ArchetypalFinalShape.Run(fixture, count, 1);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        after.Verify(count, 1, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2Sparse.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2Sparse(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var last = RaceAlvorCreate2Sparse.Run(fixture, count, 1);
        Assert.AreEqual(count, fixture.Arena.Allocated);
        Assert.IsTrue(last.HasSparseComponent1);
        Assert.AreEqual(0, last.SparseComponent1);
        Assert.IsTrue(last.HasSparseComponent2);
        Assert.AreEqual(0, last.SparseComponent2);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2ArchetypalSetters.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2ArchetypalSetters(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        RaceAlvorCreate2ArchetypalSetters.Run(fixture, count, 1);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create2ArchetypalFinalShape.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create2ArchetypalFinalShape(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        RaceAlvorCreate2ArchetypalFinalShape.Run(fixture, count, 1);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3Sparse.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3Sparse(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        var last = RaceAlvorCreate3Sparse.Run(fixture, count, 1);
        Assert.AreEqual(count, fixture.Arena.Allocated);
        Assert.IsTrue(last.HasSparseComponent1);
        Assert.AreEqual(0, last.SparseComponent1);
        Assert.IsTrue(last.HasSparseComponent2);
        Assert.AreEqual(0, last.SparseComponent2);
        Assert.IsTrue(last.HasSparseComponent3);
        Assert.AreEqual(0, last.SparseComponent3);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3ArchetypalSetters.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3ArchetypalSetters(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        RaceAlvorCreate3ArchetypalSetters.Run(fixture, count, 1);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Create3ArchetypalFinalShape.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Create3ArchetypalFinalShape(int count, int padding)
    {
        using var fixture = new RaceAlvorKitBaseContext();
        RaceAlvorCreate3ArchetypalFinalShape.Run(fixture, count, 1);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        after.Verify(count, 3, 1, true);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1DenseHandles.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1DenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 1, 0, false);
        RaceAlvorUpdate1DenseHandles.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Query.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Query(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 1, 0, false);
        RaceAlvorUpdate1Query.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1Rows.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1Rows(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 1, 0, false);
        RaceAlvorUpdate1Rows.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update1QuerySIMD.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update1QuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate1.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 1, 0, false);
        RaceAlvorUpdate1QuerySIMD.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 1);
        after.Verify(count, 1, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2DenseHandles.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2DenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 2, 0, false);
        RaceAlvorUpdate2DenseHandles.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Query.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Query(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 2, 0, false);
        RaceAlvorUpdate2Query.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2QueryUnrolled4.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2QueryUnrolled4(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 2, 0, false);
        RaceAlvorUpdate2QueryUnrolled4.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2QueryUnrolled8.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2QueryUnrolled8(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 2, 0, false);
        RaceAlvorUpdate2QueryUnrolled8.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2Rows.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2Rows(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 2, 0, false);
        RaceAlvorUpdate2Rows.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update2QuerySIMD.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update2QuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate2.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 2, 0, false);
        RaceAlvorUpdate2QuerySIMD.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3DenseHandles.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3DenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 3, 0, false);
        RaceAlvorUpdate3DenseHandles.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Query.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Query(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 3, 0, false);
        RaceAlvorUpdate3Query.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3Rows.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3Rows(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 3, 0, false);
        RaceAlvorUpdate3Rows.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for Update3QuerySIMD.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void Update3QuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorUpdate3.AlvorKitContext(count, padding);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        Assert.AreEqual(count * padding, ComparisonAlvorKitInspection.Padding(fixture.Arena));
        before.Verify(count, 3, 0, false);
        RaceAlvorUpdate3QuerySIMD.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 3);
        after.Verify(count, 3, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedDenseHandles.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedDenseHandles(int count, int padding)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        before.Verify(count, 2, 0, false);
        RaceAlvorMixedDenseHandles.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedQuery.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedQuery(int count, int padding)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        before.Verify(count, 2, 0, false);
        RaceAlvorMixedQuery.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedRows.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedRows(int count, int padding)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        before.Verify(count, 2, 0, false);
        RaceAlvorMixedRows.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }

    /// <summary>Checks exact component results, matching counts, and odd-size tails for MixedQuerySIMD.</summary>
    [TestMethod]
    [DataRow(1, 0)]
    [DataRow(33, 0)]
    [DataRow(257, 10)]
    public void MixedQuerySIMD(int count, int padding)
    {
        using var fixture = new RaceAlvorMixed.AlvorKitContext(count);
        var before = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        before.Verify(count, 2, 0, false);
        RaceAlvorMixedQuerySIMD.Run(fixture, count, 2);
        var after = ComparisonAlvorKitInspection.Read(fixture.Arena, 2);
        after.Verify(count, 2, 2, false);
    }
}
