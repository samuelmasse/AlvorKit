using System.Runtime.CompilerServices;

namespace AlvorKit;

/// <summary>Verifies per-node ownership using collection and real native-reference release.</summary>
[TestClass]
public class FnGraphLifetimeTest
{
    /// <summary>Proves repeated unloads release all nodes while the injected creation service remains alive.</summary>
    [TestMethod]
    public void LongLivedServiceDoesNotRetainDroppedGraphs()
    {
        var fn = new FnLifetimeBinding(new FnBackend());
        var graph = new FnGraph(fn);

        for (var iteration = 1; iteration <= 16; iteration++)
        {
            var dropped = CreateDroppedGraph(graph);
            CollectFinalizers();

            Assert.IsFalse(dropped.IsAlive);
            Assert.AreEqual(iteration * 2, fn.Released);
        }

        GC.KeepAlive(graph);
    }

    /// <summary>Proves a copied root retains required and hybrid dependencies but no unrelated nodes.</summary>
    [TestMethod]
    public void CopiedRootRetainsOnlyItsConnectedDependencies()
    {
        var fn = new FnLifetimeBinding(new FnBackend());
        var graph = new FnGraph(fn);
        var copy = CreateConnectedRoot(graph, out var source, out var hybrid);
        var dropped = CreateDroppedGraph(graph);
        CollectFinalizers();

        Assert.IsTrue(source.IsAlive);
        Assert.IsTrue(hybrid.IsAlive);
        Assert.IsFalse(dropped.IsAlive);
        Assert.AreEqual(2, fn.Released);
        Assert.AreEqual(7f, copy.GenSingle2D((4f, -3f), 1));
        copy.KeepAlive();
        GC.KeepAlive(graph);
    }

    /// <summary>Proves replacing a required or hybrid input releases its old unshared dependency.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ReplacingConnectionReleasesOldDependency(bool hybrid)
    {
        var fn = new FnLifetimeBinding(new FnBackend());
        var graph = new FnGraph(fn);
        var root = CreateReplaceableRoot(graph, hybrid, out var previous);
        ReplaceConnection(graph, root, hybrid);
        CollectFinalizers();

        Assert.IsFalse(previous.IsAlive);
        Assert.AreEqual(1, fn.Released);
        Assert.AreEqual(9f, root.GenSingle2D((0f, 0f), 1));
        root.KeepAlive();
    }

    /// <summary>Proves another root preserves a shared source after the first root changes its input.</summary>
    [TestMethod]
    public void ReplacingConnectionPreservesSharedDependency()
    {
        var fn = new FnLifetimeBinding(new FnBackend());
        var graph = new FnGraph(fn);
        var first = CreateSharedRoots(graph, out var second, out var source);
        ReplaceConnection(graph, first, false);
        CollectFinalizers();

        Assert.IsTrue(source.IsAlive);
        Assert.AreEqual(0, fn.Released);
        Assert.AreEqual(9f, first.GenSingle2D((0f, 0f), 1));
        Assert.AreEqual(3f, second.GenSingle2D((0f, 0f), 1));

        ReplaceConnection(graph, second, false);
        CollectFinalizers();

        Assert.IsFalse(source.IsAlive);
        Assert.AreEqual(1, fn.Released);
        Assert.AreEqual(9f, second.GenSingle2D((0f, 0f), 1));
        first.KeepAlive();
        second.KeepAlive();
    }

    /// <summary>Proves encoded roots release independently while retained encoded trees remain sampleable.</summary>
    [TestMethod]
    public void EncodedRootsHaveIndependentLifetimes()
    {
        var fn = new FnLifetimeBinding(new FnBackend());
        var graph = new FnGraph(fn);
        var retained = graph.CreateEncoded("DQkGDA==");
        var expected = retained.GenSingle2D((1.25f, -7f), 1337);
        var dropped = CreateDroppedEncodedRoot(graph);
        CollectFinalizers();

        Assert.IsFalse(dropped.IsAlive);
        Assert.AreEqual(1, fn.Released);
        Assert.AreEqual(expected, retained.GenSingle2D((1.25f, -7f), 1337));
        retained.KeepAlive();
        GC.KeepAlive(graph);
    }

    /// <summary>Proves borrowing a raw reference cannot finalize a temporary node during its native call.</summary>
    [TestMethod]
    public void TemporaryNodeSurvivesCollectionInsideSampling()
    {
        var fn = new FnLifetimeBinding(new FnBackend());
        var graph = new FnGraph(fn);
        fn.BeforeSample = () =>
        {
            CollectFinalizers();
            Assert.AreEqual(0, fn.Released, "The borrowed native node was released before sampling completed.");
        };

        Assert.AreEqual(3f, SampleTemporaryNode(graph));
        CollectFinalizers();
        Assert.AreEqual(1, fn.Released);
        GC.KeepAlive(graph);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDroppedGraph(FnGraph graph)
    {
        var source = graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 3f);
        var root = graph.Create(FnNodeType.Abs).Source(FnSource.Source, source);
        Assert.AreEqual(3f, root.GenSingle2D((0f, 0f), 1));
        return new(root.State);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static FnGraphNode CreateConnectedRoot(FnGraph graph, out WeakReference source, out WeakReference hybrid)
    {
        var constant = graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 3f);
        var addend = graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 4f);
        var sum = graph.Create(FnNodeType.Add).Source(FnSource.Lhs, constant).Hybrid(FnHybrid.Rhs, addend);
        source = new(constant.State);
        hybrid = new(addend.State);
        return graph.Create(FnNodeType.Abs).Source(FnSource.Source, sum);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static FnGraphNode CreateReplaceableRoot(FnGraph graph, bool hybrid, out WeakReference previous)
    {
        var source = graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 3f);
        previous = new(source.State);
        return hybrid
            ? graph.Create(FnNodeType.Subtract).Hybrid(FnHybrid.Lhs, source).Hybrid(FnHybrid.Rhs, 0f)
            : graph.Create(FnNodeType.Abs).Source(FnSource.Source, source);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ReplaceConnection(FnGraph graph, FnGraphNode root, bool hybrid)
    {
        var replacement = graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 9f);

        if (hybrid)
            root.Hybrid(FnHybrid.Lhs, replacement);
        else root.Source(FnSource.Source, replacement);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static FnGraphNode CreateSharedRoots(FnGraph graph, out FnGraphNode second, out WeakReference source)
    {
        var constant = graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 3f);
        source = new(constant.State);
        second = graph.Create(FnNodeType.Abs).Source(FnSource.Source, constant);
        return graph.Create(FnNodeType.Abs).Source(FnSource.Source, constant);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDroppedEncodedRoot(FnGraph graph) => new(graph.CreateEncoded("DQkGDA==").State);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static float SampleTemporaryNode(FnGraph graph) =>
        graph.Create(FnNodeType.Constant).Float(FnFloatVariable.Value, 3f).GenSingle2D((0f, 0f), 1);

    private static void CollectFinalizers()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
