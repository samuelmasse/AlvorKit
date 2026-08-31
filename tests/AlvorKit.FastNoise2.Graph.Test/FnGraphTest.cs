namespace AlvorKit;

/// <summary>Verifies typed graph construction, ownership, and validation contracts.</summary>
[TestClass]
public class FnGraphTest
{
    /// <summary>Proves the typed node enum covers every node exposed by the pinned FastNoise2 runtime.</summary>
    [TestMethod]
    public void NodeTypeEnumMatchesRuntimeCatalog()
    {
        var fn = new FnBackend();
        var types = Enum.GetValues<FnNodeType>();
        var graph = new FnGraph(fn);

        Assert.HasCount(fn.GetMetadataCount(), types);

        foreach (var type in types)
        {
            var node = graph.Create(type);
            Assert.AreNotEqual(default, node.GetActiveFeatureSet(), $"{type} did not select a FastSIMD feature set.");
        }
    }

    /// <summary>Proves typed variables, enum options, hybrids, sources, sampling, and min/max compose correctly.</summary>
    [TestMethod]
    public void TypedGraphGeneratesFiniteOutputAndRange()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var source = graph.Create(FnNodeType.CellularValue)
            .Float(FnFloatVariable.FeatureScale, 32f)
            .Integer(FnIntegerVariable.SeedOffset, 19)
            .DistanceFunction(FnDistanceFunction.Manhattan)
            .Hybrid(FnHybrid.GridJitter, 0.8f);
        var root = graph.Create(FnNodeType.FractalFbm)
            .Integer(FnIntegerVariable.Octaves, 4)
            .Float(FnFloatVariable.Lacunarity, 2f)
            .Hybrid(FnHybrid.Gain, 0.5f)
            .Source(FnSource.Source, source);
        var output = new float[9 * 7];
        var minMax = new float[2];
        Vec2 offset = (-4f, 3f);
        Vec2i count = (9, 7);
        Vec2 step = (0.75f, 1.25f);

        root.GenUniformGrid2D(output, offset, count, step, 1337, minMax);

        Assert.IsTrue(output.All(float.IsFinite));
        Assert.AreEqual(output.Min(), minMax[0], 0.00001f);
        Assert.AreEqual(output.Max(), minMax[1], 0.00001f);
    }

    /// <summary>Proves graph wiring cannot accidentally connect nodes with independent native ownership.</summary>
    [TestMethod]
    public void SourceRejectsNodeOwnedByAnotherGraph()
    {
        var fn = new FnBackend();
        var sourceGraph = new FnGraph(fn);
        var rootGraph = new FnGraph(fn);
        var source = sourceGraph.Create(FnNodeType.Simplex);
        var root = rootGraph.Create(FnNodeType.FractalFbm);

        Assert.ThrowsExactly<InvalidOperationException>(() => root.Source(FnSource.Source, source));
    }

    /// <summary>Proves encoded node trees enter the same managed-handle lifetime and typed sampling surface.</summary>
    [TestMethod]
    public void EncodedTreeCreatesGraphOwnedNode()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var node = graph.CreateEncoded("DQkGDA==");

        Assert.IsTrue(float.IsFinite(node.GenSingle2D((1.25f, -7f), 1337)));
    }

    /// <summary>Proves typed members fail clearly when the selected node does not expose that member.</summary>
    [TestMethod]
    public void VariableRejectsWrongNodeType()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var node = graph.Create(FnNodeType.Simplex);

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => node.Integer(FnIntegerVariable.Octaves, 4));
        StringAssert.Contains(exception.Message, "Simplex");
        StringAssert.Contains(exception.Message, "Octaves");
    }

    /// <summary>Proves managed sampling validates output, min/max, and position spans before native calls.</summary>
    [TestMethod]
    public void SamplingRejectsUndersizedSpans()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var node = graph.Create(FnNodeType.Simplex);
        var shortOutput = new float[3];
        var output = new float[4];
        var shortRange = new float[1];
        var shortPositions = new float[3];

        Assert.ThrowsExactly<ArgumentException>(
            () => node.GenUniformGrid2D(shortOutput, (0f, 0f), (2, 2), (1f, 1f), 1));
        Assert.ThrowsExactly<ArgumentException>(
            () => node.GenUniformGrid2D(output, (0f, 0f), (2, 2), (1f, 1f), 1, shortRange));
        Assert.ThrowsExactly<ArgumentException>(
            () => node.GenPositionArray2D(output, shortPositions, output, (0f, 0f), 1));
    }

    /// <summary>Proves connection-time validation prevents self-reference and longer dependency cycles.</summary>
    [TestMethod]
    public void ConnectionsRejectCycles()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var first = graph.Create(FnNodeType.FractalFbm);
        var second = graph.Create(FnNodeType.FractalRidged);

        Assert.ThrowsExactly<InvalidOperationException>(() => first.Source(FnSource.Source, first));

        first.Source(FnSource.Source, second);
        Assert.ThrowsExactly<InvalidOperationException>(() => second.Source(FnSource.Source, first));
    }

    /// <summary>Proves the wrapper exposes the pinned runtime's inability to detach a hybrid node connection.</summary>
    [TestMethod]
    public void HybridConstantRejectsExistingNodeConnection()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var source = graph.Create(FnNodeType.Simplex);
        var subtract = graph.Create(FnNodeType.Subtract).Hybrid(FnHybrid.Lhs, source);

        Assert.ThrowsExactly<InvalidOperationException>(() => subtract.Hybrid(FnHybrid.Lhs, 0.5f));
    }

    /// <summary>Proves an opaque encoded hybrid connection cannot be mistaken for a replaceable constant.</summary>
    [TestMethod]
    public void HybridConstantRejectsEncodedRootState()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var subtractWithConnectedLhs = graph.CreateEncoded("FgIADA==");

        Assert.ThrowsExactly<InvalidOperationException>(
            () => subtractWithConnectedLhs.Hybrid(FnHybrid.Lhs, 0.5f));
    }

    /// <summary>Proves native sampling never receives overlapping input, output, or range storage.</summary>
    [TestMethod]
    public void SamplingRejectsOverlappingSpans()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var node = graph.Create(FnNodeType.Simplex);
        var storage = new float[8];

        Assert.ThrowsExactly<ArgumentException>(
            () => node.GenPositionArray2D(storage.AsSpan(0, 4), storage.AsSpan(0, 4), storage.AsSpan(4, 4), (0f, 0f), 1));
        Assert.ThrowsExactly<ArgumentException>(() => node.GenPositionArray3D(
            storage.AsSpan(0, 2), storage.AsSpan(2, 2), storage.AsSpan(4, 2), storage.AsSpan(0, 2), (0f, 0f, 0f), 1));
        Assert.ThrowsExactly<ArgumentException>(() => node.GenPositionArray4D(
            storage.AsSpan(0, 2), storage.AsSpan(2, 2), storage.AsSpan(4, 2), storage.AsSpan(6, 2),
            storage.AsSpan(0, 2), (0f, 0f, 0f, 0f), 1));
        Assert.ThrowsExactly<ArgumentException>(
            () => node.GenUniformGrid2D(storage.AsSpan(0, 4), (0f, 0f), (2, 2), (1f, 1f), 1, storage.AsSpan(2, 2)));
    }

    /// <summary>Proves native count multiplication cannot receive nonpositive or overflowing grid sizes.</summary>
    [TestMethod]
    public void SamplingRejectsInvalidGridCounts()
    {
        var fn = new FnBackend();
        var graph = new FnGraph(fn);
        var node = graph.Create(FnNodeType.Simplex);
        var output = new float[1];

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => node.GenUniformGrid2D(output, (0f, 0f), (0, 1), (1f, 1f), 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => node.GenUniformGrid2D(output, (0f, 0f), (int.MaxValue, 2), (1f, 1f), 1));
    }

}
