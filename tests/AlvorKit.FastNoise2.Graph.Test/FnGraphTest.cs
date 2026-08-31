namespace AlvorKit;

[TestClass]
public class FnGraphTest
{
    /// <summary>Proves the typed node enum covers every node exposed by the pinned FastNoise2 runtime.</summary>
    [TestMethod]
    public void NodeTypeEnumMatchesRuntimeCatalog()
    {
        var fn = new FnBackend();
        var types = Enum.GetValues<FnNodeType>();
        using var graph = new FnGraph(fn);

        Assert.HasCount(fn.GetMetadataCount(), types);

        foreach (var type in types)
        {
            var node = graph.Create(type);
            Assert.AreNotEqual(0u, node.GetActiveFeatureSet(), $"{type} did not select a FastSIMD feature set.");
        }
    }

    /// <summary>Proves typed variables, enum options, hybrids, sources, sampling, and min/max compose correctly.</summary>
    [TestMethod]
    public void TypedGraphGeneratesFiniteOutputAndRange()
    {
        var fn = new FnBackend();
        using var graph = new FnGraph(fn);
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

    /// <summary>Proves clearing a graph prevents stale native handles from being sampled.</summary>
    [TestMethod]
    public void ClearInvalidatesReturnedNodes()
    {
        var fn = new FnBackend();
        using var graph = new FnGraph(fn);
        var node = graph.Create(FnNodeType.Simplex);

        graph.Clear();

        Assert.ThrowsExactly<InvalidOperationException>(() => node.GenSingle2D((1f, 2f), 17));
    }

    /// <summary>Proves graph wiring cannot accidentally connect nodes with independent native ownership.</summary>
    [TestMethod]
    public void SourceRejectsNodeOwnedByAnotherGraph()
    {
        var fn = new FnBackend();
        using var sourceGraph = new FnGraph(fn);
        using var rootGraph = new FnGraph(fn);
        var source = sourceGraph.Create(FnNodeType.Simplex);
        var root = rootGraph.Create(FnNodeType.FractalFbm);

        Assert.ThrowsExactly<InvalidOperationException>(() => root.Source(FnSource.Source, source));
    }

    /// <summary>Proves encoded node trees enter the same graph-owned lifetime and typed sampling surface.</summary>
    [TestMethod]
    public void EncodedTreeCreatesGraphOwnedNode()
    {
        var fn = new FnBackend();
        using var graph = new FnGraph(fn);
        var node = graph.CreateEncoded("DQkGDA==");

        Assert.IsTrue(float.IsFinite(node.GenSingle2D((1.25f, -7f), 1337)));
    }

    /// <summary>Proves typed members fail clearly when the selected node does not expose that member.</summary>
    [TestMethod]
    public void VariableRejectsWrongNodeType()
    {
        var fn = new FnBackend();
        using var graph = new FnGraph(fn);
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
        using var graph = new FnGraph(fn);
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
}
