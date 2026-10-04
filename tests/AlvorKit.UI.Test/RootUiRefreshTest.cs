namespace AlvorKit;

/// <summary>Verifies data refresh before layout, explicit requests, active lifetimes, and independent tick callbacks.</summary>
[TestClass]
public class RootUiRefreshTest
{
    /// <summary>Initial refresh coalesces with a pending count, decrements before each call, and never invokes tick callbacks.</summary>
    [TestMethod]
    public void Counter_CoalescesInitialRefreshAndPreservesTickUpdates()
    {
        using var h = new UiTestHarness();
        List<int> counts = [];
        var updates = 0;
        Node(h.Ui, out var node)
            .SizeRelativeV((0, 0))
            .SizeF(() => (counts.Count * 10, 20))
            .RefreshCountV(3)
            .OnRefreshF(() => counts.Add(node.RefreshCountFV.Resolve()))
            .OnUpdateF(() => updates++);

        h.Script.Draw();

        CollectionAssert.AreEqual(new[] { 2, 1, 0 }, counts);
        Assert.AreEqual(new Vec2(30, 20), node.SizeR);
        Assert.AreEqual(0, updates);
        h.Script.Draw();
        Assert.AreEqual(3, counts.Count);

        h.Update();
        Assert.AreEqual(3, counts.Count);
        Assert.AreEqual(1, updates);

        node.RefreshCountFV = 2;
        h.Script.Draw();
        CollectionAssert.AreEqual(new[] { 2, 1, 0, 1, 0 }, counts);
        Assert.AreEqual(new Vec2(50, 20), node.SizeR);
        Assert.AreEqual(1, updates);
    }

    /// <summary>New descendants refresh automatically before their first layout, without an explicit pending counter.</summary>
    [TestMethod]
    public void CreatedDescendants_RefreshBeforeFirstLayout()
    {
        using var h = new UiTestHarness();
        var leaf = default(EntMut);
        Node(h.Ui, out var parent)
            .OnRefreshF(() =>
            {
                Node(parent, out var child)
                    .OnRefreshF(() =>
                    {
                        Node(child, out leaf)
                            .SizeRelativeV((0, 0))
                            .OnRefreshF(() => leaf.SizeFV = new Vec2(73, 29));
                    });
            });

        h.Script.Draw();

        Assert.AreNotEqual(default, leaf);
        Assert.AreEqual(new Vec2(73, 29), leaf.SizeR);
        Assert.AreEqual(0, leaf.RefreshCountFV.Resolve());
    }

    /// <summary>A later node's request for an earlier node is drained before layout without repeating automatic refreshes.</summary>
    [TestMethod]
    public void RequestOnEarlierNode_DrainsBeforeLayout()
    {
        using var h = new UiTestHarness();
        var firstCalls = 0;
        var laterCalls = 0;
        Node(h.Ui, out var first)
            .SizeRelativeV((0, 0))
            .SizeF(() => (firstCalls * 10, 20))
            .OnRefreshF(() => firstCalls++);
        Node(h.Ui)
            .OnRefreshF(() =>
            {
                laterCalls++;
                first.RefreshCountFV = 2;
            });

        h.Script.Draw();

        Assert.AreEqual(3, firstCalls);
        Assert.AreEqual(1, laterCalls);
        Assert.AreEqual(new Vec2(30, 20), first.SizeR);
    }

    /// <summary>Removing siblings during refresh skips their callbacks and still prepares surviving children.</summary>
    [TestMethod]
    public void RefreshRemoval_UsesLiveChildren()
    {
        using var h = new UiTestHarness();
        var removedCalls = 0;
        var survivorCalls = 0;
        Node(h.Ui, out var remover);
        Node(h.Ui, out var removed)
            .OnRefreshF(() => removedCalls++);
        Node(h.Ui)
            .OnRefreshF(() => survivorCalls++);
        remover.Mutate()
            .OnRefreshF(() =>
            {
                NodesRemove(h.Ui, remover);
                NodesRemove(h.Ui, removed);
            });

        h.Script.Draw();

        Assert.AreEqual(0, removedCalls);
        Assert.AreEqual(1, survivorCalls);
        Assert.AreEqual(1, NodesCount(h.Ui));
    }

    /// <summary>Periodic refresh uses supplied UI deltas and coalesces due work with one explicit request.</summary>
    [TestMethod]
    public void Interval_UsesElapsedUiTimeAndCoalescesRequests()
    {
        using var h = new UiTestHarness();
        var calls = 0;
        var updates = 0;
        Node(h.Ui, out var node)
            .RefreshIntervalV(TimeSpan.FromSeconds(1))
            .OnRefreshF(() => calls++)
            .OnUpdateF(() => updates++);
        h.Script.Draw();
        Assert.AreEqual(1, calls);

        h.Host.RaiseUpdate(0.25);
        h.Host.RaiseUpdate(0.5);
        h.Script.Draw();
        Assert.AreEqual(1, calls);
        Assert.AreEqual(2, updates);

        h.Host.RaiseUpdate(0.25);
        Assert.AreEqual(2, calls);
        node.RefreshCountFV = 1;
        h.Host.RaiseUpdate(1);
        Assert.AreEqual(3, calls);
        Assert.AreEqual(0, node.RefreshCountFV.Resolve());

        h.Host.RaiseUpdate(3);
        Assert.AreEqual(4, calls);
        Assert.AreEqual(5, updates);
        h.Script.Draw();
        Assert.AreEqual(4, calls);
    }

    /// <summary>Hidden ancestors defer first refresh and requests; re-enabling a prepared subtree refreshes it once again.</summary>
    [TestMethod]
    public void DisabledAncestors_DeferRequestsAndRefreshOnReenable()
    {
        using var h = new UiTestHarness();
        var calls = 0;
        Node(h.Ui, out var hidden)
            .IsDisabledV(true);
        Node(hidden, out var child)
            .RefreshCountV(2)
            .OnRefreshF(() => calls++);
        Node(h.Ui)
            .IsDeletedV(true)
            .OnRefreshF(() => calls += 100);

        h.Script.Draw();

        Assert.AreEqual(0, calls);
        Assert.AreEqual(2, child.RefreshCountFV.Resolve());
        hidden.IsDisabledFV = false;
        h.Script.Draw();
        Assert.AreEqual(2, calls);
        Assert.AreEqual(0, child.RefreshCountFV.Resolve());
        h.Script.Draw();
        Assert.AreEqual(2, calls);

        hidden.IsDisabledFV = true;
        h.Script.Draw();
        hidden.IsDisabledFV = false;
        h.Script.Draw();
        Assert.AreEqual(3, calls);
    }

    /// <summary>Stack companions and the top menu refresh, while covered entries retain requests until revealed.</summary>
    [TestMethod]
    public void Stack_RefreshesCompanionsAndRevealedMenus()
    {
        using var h = new UiTestHarness();
        var companionCalls = 0;
        var coveredCalls = 0;
        var topCalls = 0;
        var companion = NodeC(h.Ui);
        companion.Mutate()
            .OnRefreshF(() => companionCalls++);
        var covered = NodeS(h.Ui).Ent;
        covered.Mutate()
            .CompanionV(companion)
            .RefreshCountV(1)
            .OnRefreshF(() => coveredCalls++);
        NodeS(h.Ui)
            .OnRefreshF(() => topCalls++);

        h.Script.Draw();

        Assert.AreEqual(1, companionCalls);
        Assert.AreEqual(0, coveredCalls);
        Assert.AreEqual(1, topCalls);
        Assert.AreEqual(1, covered.RefreshCountFV.Resolve());
        NodeStackPop(h.Ui);
        h.Script.Draw();
        Assert.AreEqual(1, companionCalls);
        Assert.AreEqual(1, coveredCalls);

        NodeS(h.Ui);
        h.Script.Draw();
        NodeStackPop(h.Ui);
        h.Script.Draw();
        Assert.AreEqual(2, coveredCalls);
    }

    /// <summary>A disabled top menu cannot resolve layout before its refresh, while its companion remains visible.</summary>
    [TestMethod]
    public void Stack_DisabledTopWaitsForRefreshBeforeLayout()
    {
        using var h = new UiTestHarness();
        var prepared = false;
        var layouts = 0;
        var companionCalls = 0;
        var companion = NodeC(h.Ui);
        companion.Mutate()
            .OnRefreshF(() => companionCalls++);
        var top = NodeS(h.Ui).Ent;
        top.Mutate()
            .CompanionV(companion)
            .IsDisabledV(true)
            .OnRefreshF(() => prepared = true)
            .SizeF(() =>
            {
                Assert.IsTrue(prepared);
                layouts++;
                return (40, 20);
            });

        h.Script.Draw();
        Assert.IsFalse(prepared);
        Assert.AreEqual(0, layouts);
        Assert.AreEqual(1, companionCalls);

        top.IsDisabledFV = false;
        h.Script.Draw();
        Assert.IsTrue(prepared);
        Assert.IsTrue(layouts > 0);
        Assert.AreEqual(1, companionCalls);
    }

    /// <summary>Refresh callbacks run with the owning surface's scale and viewport active.</summary>
    [TestMethod]
    public void Refresh_UsesOwningSurfaceContext()
    {
        using var h = new UiTestHarness();
        var viewport = new Box2((100, 50), (500, 250));
        using var surface = h.Surfaces.Create(viewport, 2f, 1f);
        var observedScale = 0f;
        var observedViewport = default(Box2);
        Node(surface.Root)
            .OnRefreshF(() =>
            {
                observedScale = h.Scale.Scale;
                observedViewport = h.Context.Viewport;
            });

        h.Script.Draw();

        Assert.AreEqual(2f, observedScale);
        Assert.AreEqual(viewport, observedViewport);
        Assert.AreEqual(1f, h.Scale.Scale);
    }

    /// <summary>Repeated requests on an established tree reuse preparation storage without managed allocations.</summary>
    [TestMethod]
    public void RepeatedRequests_DoNotAllocateAfterWarmup()
    {
        using var h = new UiTestHarness();
        var calls = 0;
        Node(h.Ui, out var node)
            .OnRefreshF(() => calls++);

        for (var i = 0; i < 100; i++)
        {
            node.RefreshCountFV = 1;
            h.Script.Draw();
        }

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
        {
            node.RefreshCountFV = 1;
            h.Script.Draw();
        }

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.AreEqual(1100, calls);
    }

    /// <summary>Removing a subtree from its first child's tick immediately skips the remaining descendants.</summary>
    [TestMethod]
    public void Tick_RemovedAncestorSkipsRemainingDescendants()
    {
        using var h = new UiTestHarness();
        var removedCalls = 0;
        var survivorCalls = 0;
        Node(h.Ui, out var group);
        Node(group)
            .OnUpdateF(() => NodesRemove(h.Ui, group));
        Node(group)
            .OnUpdateF(() => removedCalls++);
        Node(h.Ui)
            .OnUpdateF(() => survivorCalls++);

        h.Update();

        Assert.AreEqual(0, removedCalls);
        Assert.AreEqual(1, survivorCalls);
    }

    /// <summary>Disabling a subtree while updating its first child immediately skips the remaining descendants.</summary>
    [TestMethod]
    public void Tick_DisabledAncestorSkipsRemainingDescendants()
    {
        using var h = new UiTestHarness();
        var disabledCalls = 0;
        var survivorCalls = 0;
        Node(h.Ui, out var group);
        Node(group)
            .OnUpdateF(() => group.IsDisabledFV = true);
        Node(group)
            .OnUpdateF(() => disabledCalls++);
        Node(h.Ui)
            .OnUpdateF(() => survivorCalls++);

        h.Update();

        Assert.AreEqual(0, disabledCalls);
        Assert.AreEqual(1, survivorCalls);
    }

    /// <summary>Reordering retained siblings and removing the current node cannot duplicate or skip surviving tick callbacks.</summary>
    [TestMethod]
    public void Tick_ReordersAndSelfRemovalVisitEachSurvivorOnce()
    {
        using var h = new UiTestHarness();
        var firstCalls = 0;
        var secondCalls = 0;
        var thirdCalls = 0;
        Node(h.Ui, out var first);
        Node(h.Ui, out var second)
            .OnUpdateF(() => secondCalls++);
        Node(h.Ui)
            .OnUpdateF(() => thirdCalls++);
        first.Mutate()
            .OnUpdateF(() =>
            {
                firstCalls++;
                NodesRemove(h.Ui, second);
                NodesAdd(h.Ui, second);
                NodesRemove(h.Ui, first);
            });

        h.Update();

        Assert.AreEqual(1, firstCalls);
        Assert.AreEqual(1, secondCalls);
        Assert.AreEqual(1, thirdCalls);
    }

    /// <summary>Children created during a tick refresh and lay out before they receive their first normal tick.</summary>
    [TestMethod]
    public void Tick_NewChildrenWaitForFirstPreparation()
    {
        using var h = new UiTestHarness();
        var created = false;
        var refreshes = 0;
        var updates = 0;
        Node(h.Ui, out var parent)
            .OnUpdateF(() =>
            {
                if (created)
                    return;

                created = true;
                Node(parent)
                    .SizeRelativeV((0, 0))
                    .SizeV((40, 30))
                    .OnRefreshF(() => refreshes++)
                    .OnUpdateF(() =>
                    {
                        Assert.AreEqual(1, refreshes);
                        Assert.AreEqual(new Vec2(40, 30), Nodes(parent)[0].SizeR);
                        updates++;
                    });
            });

        h.Update();
        Assert.AreEqual(0, updates);
        h.Update();
        Assert.AreEqual(1, refreshes);
        Assert.AreEqual(1, updates);
    }
}
