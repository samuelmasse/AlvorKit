namespace AlvorKit;

/// <summary>Covers pending update calls before layout and their interaction with live UI trees.</summary>
[TestClass]
public class RootUiAheadUpdateTest
{
    /// <summary>Preparation consumes each count before invoking the callback and leaves tick updates independent.</summary>
    [TestMethod]
    public void Counter_PreparesFirstLayoutAndPreservesNormalUpdates()
    {
        var h = new UiTestHarness();
        List<int> counts = [];
        Node(h.Ui, out var node)
            .SizeRelativeV((0, 0))
            .SizeF(() => (counts.Count * 10, 20))
            .AheadUpdateCountV(3)
            .OnUpdateF(() => counts.Add(node.AheadUpdateCountFV.Resolve()));

        h.Script.Draw();

        CollectionAssert.AreEqual(new[] { 2, 1, 0 }, counts);
        Assert.AreEqual(new Vec2(30, 20), node.SizeR);
        h.Script.Draw();
        Assert.AreEqual(3, counts.Count);

        h.Update();
        CollectionAssert.AreEqual(new[] { 2, 1, 0, 0 }, counts);

        node.AheadUpdateCountFV = 1;
        h.Script.Draw();
        Assert.AreEqual(5, counts.Count);
        Assert.AreEqual(new Vec2(50, 20), node.SizeR);
    }

    /// <summary>Nested branches created by pending callbacks are ready for their very first layout.</summary>
    [TestMethod]
    public void CreatedDescendants_PrepareInSamePhase()
    {
        var h = new UiTestHarness();
        var leaf = default(EntMut);
        Node(h.Ui, out var parent)
            .AheadUpdateCountV(1)
            .OnUpdateF(() =>
            {
                Node(parent, out var child)
                    .AheadUpdateCountV(1)
                    .OnUpdateF(() =>
                    {
                        Node(child, out leaf)
                            .AheadUpdateCountV(1)
                            .SizeRelativeV((0, 0))
                            .OnUpdateF(() => leaf.SizeFV = new Vec2(73, 29));
                    });
            });

        h.Script.Draw();

        Assert.AreNotEqual(default(EntMut), leaf);
        Assert.AreEqual(new Vec2(73, 29), leaf.SizeR);
        Assert.AreEqual(0, leaf.AheadUpdateCountFV.Resolve());
    }

    /// <summary>A request targeting an earlier node is drained before layout begins.</summary>
    [TestMethod]
    public void RequestOnEarlierNode_RunsBeforeLayout()
    {
        var h = new UiTestHarness();
        var calls = 0;
        Node(h.Ui, out var first)
            .SizeRelativeV((0, 0))
            .SizeF(() => (calls * 10, 20))
            .OnUpdateF(() => calls++);
        Node(h.Ui)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => first.AheadUpdateCountFV = 2);

        h.Script.Draw();

        Assert.AreEqual(2, calls);
        Assert.AreEqual(new Vec2(20, 20), first.SizeR);
    }

    /// <summary>Removing nodes during preparation cannot run their pending callbacks or skip surviving siblings.</summary>
    [TestMethod]
    public void Removal_UsesLiveChildren()
    {
        var h = new UiTestHarness();
        var removedCalls = 0;
        var survivorCalls = 0;
        Node(h.Ui, out var remover);
        Node(h.Ui, out var removed)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => removedCalls++);
        Node(h.Ui)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => survivorCalls++);
        remover.Mutate()
            .AheadUpdateCountV(1)
            .OnUpdateF(() =>
            {
                NodesRemove(h.Ui, remover);
                NodesRemove(h.Ui, removed);
            });

        h.Script.Draw();

        Assert.AreEqual(0, removedCalls);
        Assert.AreEqual(1, survivorCalls);
        Assert.AreEqual(1, NodesCount(h.Ui));
    }

    /// <summary>Hidden branches retain requests while deleted nodes never run ahead callbacks.</summary>
    [TestMethod]
    public void DisabledAndDeletedNodes_WaitOrSkip()
    {
        var h = new UiTestHarness();
        var calls = 0;
        Node(h.Ui, out var hidden)
            .IsDisabledV(true);
        Node(hidden, out var child)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => calls++);
        Node(h.Ui)
            .IsDeletedV(true)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => calls += 100);

        h.Script.Draw();

        Assert.AreEqual(0, calls);
        Assert.AreEqual(1, child.AheadUpdateCountFV.Resolve());
        hidden.IsDisabledFV = false;
        h.Script.Draw();
        Assert.AreEqual(1, calls);
        Assert.AreEqual(0, child.AheadUpdateCountFV.Resolve());
    }

    /// <summary>Preparation visits stack companions and the top menu while leaving covered menus pending.</summary>
    [TestMethod]
    public void Stack_PreparesCompanionsAndTop()
    {
        var h = new UiTestHarness();
        var calls = 0;
        var companion = NodeC(h.Ui);
        companion.Mutate()
            .AheadUpdateCountV(1)
            .OnUpdateF(() => calls++);
        var covered = NodeS(h.Ui).Ent;
        covered.Mutate()
            .CompanionV(companion)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => calls += 10);
        NodeS(h.Ui)
            .AheadUpdateCountV(1)
            .OnUpdateF(() => calls += 100);

        h.Script.Draw();

        Assert.AreEqual(101, calls);
        Assert.AreEqual(1, covered.AheadUpdateCountFV.Resolve());
        NodeStackPop(h.Ui);
        h.Script.Draw();
        Assert.AreEqual(111, calls);
    }

    /// <summary>Ahead callbacks run with the owning surface's scale and viewport active.</summary>
    [TestMethod]
    public void AheadCallback_UsesSurfaceContext()
    {
        var h = new UiTestHarness();
        var viewport = new Box2((100, 50), (500, 250));
        using var surface = h.Surfaces.Create(viewport, 2f, 1f);
        var observedScale = 0f;
        var observedViewport = default(Box2);
        Node(surface.Root)
            .AheadUpdateCountV(1)
            .OnUpdateF(() =>
            {
                observedScale = h.Scale.Scale;
                observedViewport = h.Context.Viewport;
            });

        h.Script.Draw();

        Assert.AreEqual(2f, observedScale);
        Assert.AreEqual(viewport, observedViewport);
        Assert.AreEqual(1f, h.Scale.Scale);
    }

    /// <summary>Repeated requests on a warmed tree do not allocate managed storage.</summary>
    [TestMethod]
    public void RepeatedRequests_DoNotAllocateAfterWarmup()
    {
        var h = new UiTestHarness();
        var calls = 0;
        Node(h.Ui, out var node)
            .OnUpdateF(() => calls++);

        for (var i = 0; i < 100; i++)
        {
            node.AheadUpdateCountFV = 1;
            h.Script.Draw();
        }

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
        {
            node.AheadUpdateCountFV = 1;
            h.Script.Draw();
        }

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.AreEqual(1100, calls);
    }
}
