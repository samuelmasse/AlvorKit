namespace AlvorKit;

/// <summary>Verifies keyed subtree identity, synchronous data delivery, nesting, cleanup, and retained storage.</summary>
[TestClass]
public class UiChildrenTest
{
    /// <summary>Initial and replacement data reach the next layout without invoking tick or input callbacks.</summary>
    [TestMethod]
    public void Set_SuppliesDataBeforeLayoutWithoutRunningCallbacks()
    {
        var h = new UiTestHarness();
        var updates = 0;
        var presses = 0;
        Node(h.Ui, out var parent);
        var children = NodesFor<int, Row>(parent, row => row.Key, mount =>
        {
            var value = default(Row);
            Node(mount, out var node)
                .SizeRelativeV((0, 0))
                .SizeF(() => (value.Width, 20))
                .TextF(() => value.Text)
                .OnUpdateF(() => updates++)
                .OnPressF(() => presses++);
            return new(node, next => value = next);
        });

        children.Set([new(1, "First", 40)]);
        var first = Nodes(parent)[0];
        Assert.AreEqual("First", first.TextFV.Resolve().ToString());
        h.Script.Draw();
        Assert.AreEqual(new Vec2(40, 20), first.SizeR);

        children.Set([new(1, "Changed", 90)]);
        h.Script.Draw();
        Assert.AreEqual(first, Nodes(parent)[0]);
        Assert.AreEqual("Changed", first.TextFV.Resolve().ToString());
        Assert.AreEqual(new Vec2(90, 20), first.SizeR);
        Assert.AreEqual(0, updates);
        Assert.AreEqual(0, presses);
    }

    /// <summary>Inserting, removing, and reordering keys preserves surviving nodes and their independent local state.</summary>
    [TestMethod]
    public void Set_PreservesIdentityAndLocalStateAcrossChanges()
    {
        var h = new UiTestHarness();
        var created = 0;
        Node(h.Ui, out var parent);
        var children = NodesFor<int, Row>(parent, row => row.Key, mount =>
        {
            created++;
            var text = string.Empty;
            var expanded = false;
            Node(mount, out var node)
                .TextF(() => text)
                .SizeF(() => (100, expanded ? 40 : 20))
                .OnPressF(() => expanded = !expanded);
            return new(node, row => text = row.Text);
        });
        children.Set([new(1, "One", 0), new(2, "Two", 0), new(3, "Three", 0)]);
        var first = Nodes(parent)[0];
        var removed = Nodes(parent)[1];
        var third = Nodes(parent)[2];
        third.OnPressFV.Resolve()!();

        children.Set([new(3, "Updated three", 0), new(4, "Four", 0), new(1, "Updated one", 0)]);

        Assert.AreEqual(4, created);
        Assert.AreEqual(third, Nodes(parent)[0]);
        Assert.AreEqual(first, Nodes(parent)[2]);
        Assert.AreEqual("Updated three", third.TextFV.Resolve().ToString());
        Assert.AreEqual(40f, third.SizeFV.Resolve().Y);
        Assert.AreEqual(20f, first.SizeFV.Resolve().Y);
        h.Ui.Cleanup();
        Assert.IsFalse(removed.IsAlive);
        Assert.IsTrue(first.IsAlive);
        Assert.IsTrue(third.IsAlive);

        children.Set([new(2, "Recreated two", 0), new(3, "Three again", 0)]);
        Assert.AreEqual(5, created);
        Assert.AreNotEqual(removed, Nodes(parent)[0]);
        Assert.AreEqual(third, Nodes(parent)[1]);
        Assert.AreEqual(40f, third.SizeFV.Resolve().Y);
    }

    /// <summary>An empty set detaches the entire collection and cleanup reclaims descendants before later keys are recreated.</summary>
    [TestMethod]
    public void EmptySet_DetachesAndReclaimsSubtrees()
    {
        var h = new UiTestHarness();
        Node(h.Ui, out var parent);
        var children = NodesFor<int, Row>(parent, row => row.Key, mount =>
        {
            Node(mount, out var node);
            Node(node, out var label);
            return new(node, row => label.TextFV = row.Text);
        });
        children.Set([new(1, "One", 0)]);
        var removed = Nodes(parent)[0];
        var descendant = Nodes(removed)[0];

        children.Set([]);

        Assert.AreEqual(0, NodesCount(parent));
        h.Ui.Cleanup();
        Assert.IsFalse(removed.IsAlive);
        Assert.IsFalse(descendant.IsAlive);

        children.Set([new(1, "Replacement", 0)]);
        Assert.AreNotEqual(removed, Nodes(parent)[0]);
        Assert.AreEqual("Replacement", Nodes(Nodes(parent)[0])[0].TextFV.Resolve().ToString());
    }

    /// <summary>Nested keyed views update synchronously and retain identities independently at every depth.</summary>
    [TestMethod]
    public void NestedViews_RetainChildrenWhenParentsMove()
    {
        var h = new UiTestHarness();
        Node(h.Ui, out var parent);
        var branches = NodesFor<int, Branch>(parent, branch => branch.Key, mount =>
        {
            Node(mount, out var branchNode);
            var leaves = NodesFor<int, Row>(branchNode, row => row.Key, leafMount =>
            {
                Node(leafMount, out var leaf);
                return new(leaf, row => leaf.TextFV = row.Text);
            });
            return new(branchNode, branch => leaves.Set(branch.Children));
        });
        branches.Set([new(1, [new(10, "A", 0), new(11, "B", 0)]), new(2, [new(20, "C", 0)])]);
        var firstBranch = Nodes(parent)[0];
        var secondBranch = Nodes(parent)[1];
        var firstLeaf = Nodes(firstBranch)[0];
        var secondLeaf = Nodes(firstBranch)[1];
        var removedLeaf = Nodes(secondBranch)[0];

        branches.Set([new(2, []), new(1, [new(11, "B updated", 0), new(10, "A updated", 0)])]);

        Assert.AreEqual(secondBranch, Nodes(parent)[0]);
        Assert.AreEqual(firstBranch, Nodes(parent)[1]);
        Assert.AreEqual(0, NodesCount(secondBranch));
        Assert.AreEqual(secondLeaf, Nodes(firstBranch)[0]);
        Assert.AreEqual(firstLeaf, Nodes(firstBranch)[1]);
        Assert.AreEqual("B updated", secondLeaf.TextFV.Resolve().ToString());
        Assert.AreEqual("A updated", firstLeaf.TextFV.Resolve().ToString());
        h.Ui.Cleanup();
        Assert.IsFalse(removedLeaf.IsAlive);
        Assert.IsTrue(firstLeaf.IsAlive);
        Assert.IsTrue(secondLeaf.IsAlive);
    }

    /// <summary>Reordering an established collection repeatedly reuses both its views and reconciliation storage.</summary>
    [TestMethod]
    public void RepeatedSets_DoNotAllocateAfterWarmup()
    {
        var h = new UiTestHarness();
        var created = 0;
        Node(h.Ui, out var parent);
        var children = NodesFor<int, Row>(parent, row => row.Key, mount =>
        {
            created++;
            Node(mount, out var node);
            return new(node, row => node.TextFV = row.Text);
        });
        Row[] forward = [new(1, "One", 0), new(2, "Two", 0), new(3, "Three", 0)];
        Row[] reverse = [new(3, "Changed three", 0), new(2, "Changed two", 0), new(1, "Changed one", 0)];

        for (var i = 0; i < 100; i++)
        {
            children.Set(forward);
            children.Set(reverse);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < 1000; i++)
        {
            children.Set(forward);
            children.Set(reverse);
        }

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.AreEqual(3, created);
        Assert.AreEqual("Changed three", Nodes(parent)[0].TextFV.Resolve().ToString());
    }

    private readonly record struct Row(int Key, string Text, int Width);
    private readonly record struct Branch(int Key, Row[] Children);
}
