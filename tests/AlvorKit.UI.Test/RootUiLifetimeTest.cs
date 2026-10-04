using System.Runtime.CompilerServices;

namespace AlvorKit;

[TestClass]
public class RootUiLifetimeTest
{
    /// <summary>Root component access and borrowed handles address the same explicitly owned Ent.</summary>
    [TestMethod]
    public void RootComponents_ShareIdentityAndInvalidateOnDispose()
    {
        using var ui = new RootUi();
        Ent read = ui;
        EntMut mutable = ui;
        Assert.AreEqual(read.Handle, ui.Handle);
        Assert.AreEqual(mutable.Handle, ui.Handle);

        ui.Set<int, RootValue>(42);
        ui.SetArchetypal<int, RootValue, RootArch>(73);
        Assert.IsTrue(ui.Has<int, RootValue>());
        Assert.IsTrue(ui.HasArchetypal<int, RootValue, RootArch>());
        Assert.AreEqual(42, read.Get<int, RootValue>());
        Assert.AreEqual(73, ui.GetArchetypal<int, RootValue, RootArch>());
        Assert.IsTrue(ui.Unset<int, RootValue>());
        Assert.IsTrue(ui.UnsetArchetypal<int, RootValue, RootArch>());
        Assert.IsFalse(mutable.Has<int, RootValue>());
        Assert.IsFalse(read.HasArchetypal<int, RootValue, RootArch>());

        ui.Dispose();
        Assert.IsFalse(mutable.IsAlive);
        Assert.AreEqual("Ent Disposed", ui.ToString());
    }

    /// <summary>Cleanup retains stacked views and companions until their root lifetime ends.</summary>
    [TestMethod]
    public void Dispose_ReleasesStackedViewsAndCompanions()
    {
        using var ui = new RootUi();
        EntMut first = NodeS(ui);
        EntMut second = NodeS(ui);
        var companion = NodeC(ui);
        first.CompanionFV = companion;
        ui.Cleanup();
        Assert.IsTrue(first.IsAlive);
        Assert.IsTrue(second.IsAlive);
        Assert.IsTrue(companion.IsAlive);

        ui.Dispose();
        Assert.IsFalse(first.IsAlive);
        Assert.IsFalse(second.IsAlive);
        Assert.IsFalse(companion.IsAlive);
    }

    /// <summary>Growing root-owned traversal buffers preserves child ordering and render delays before disposal.</summary>
    [TestMethod]
    public void Traverse_GrowsOwnedBuffersAndPreservesOrdering()
    {
        using var h = new UiTestHarness();
        EntMut root = h.Ui;
        var children = new EntMut[40];
        for (int index = 0; index < children.Length; index++)
            Node(h.Ui, out children[index]).OrderValueV(children.Length - index);
        children[0].RenderDelayFV = 2;

        h.Script.Draw();
        CollectionAssert.AreEqual(children.Reverse().ToArray(), root.NodesR.ToArray());
        Assert.AreEqual(0, children[0].RenderDelayFV.Resolve());
        Assert.AreEqual(0f, children[0].SnapR);

        h.Dispose();
        Assert.IsTrue(children.All(child => !child.IsAlive));
    }

    /// <summary>Root disposal invalidates mounted and detached children and releases retained callbacks.</summary>
    [TestMethod]
    public void Dispose_ReleasesWholeTreeAndCapturedReferences()
    {
        using var ui = new RootUi();
        EntMut root = ui;
        var detached = ui.Alloc();
        var retained = AttachCallback(ui, out var child);
        GC.Collect();
        Assert.IsTrue(retained.IsAlive);

        ui.Dispose();
        ui.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(root.IsAlive);
        Assert.IsFalse(child.IsAlive);
        Assert.IsFalse(detached.IsAlive);
        Assert.IsFalse(retained.IsAlive);
        Assert.ThrowsExactly<EntArenaDisposedException>(() => ui.Alloc());
        GC.KeepAlive(ui);
    }

    /// <summary>Surface disposal leaves other trees live, and registered shutdown releases every remaining tree.</summary>
    [TestMethod]
    public void SurfaceAndRootShutdown_DisposeTheirOwnedTrees()
    {
        using var h = new UiTestHarness();
        var first = h.Surfaces.Create();
        var second = h.Surfaces.Create();
        var firstChild = first.Root.Alloc();
        var secondChild = second.Root.Alloc();
        var defaultChild = h.Ui.Alloc();
        first.Dispose();

        Assert.IsFalse(first.Root.IsAlive);
        Assert.IsFalse(firstChild.IsAlive);
        Assert.IsTrue(secondChild.IsAlive);
        Assert.IsTrue(defaultChild.IsAlive);

        h.Dispose();

        Assert.IsFalse(second.Root.IsAlive);
        Assert.IsFalse(secondChild.IsAlive);
        Assert.IsFalse(h.Ui.IsAlive);
        Assert.IsFalse(defaultChild.IsAlive);
        Assert.AreEqual(0, h.Surfaces.Span.Length);
        Assert.AreEqual(0, h.Scripts.Span.Length);
        Assert.ThrowsExactly<ObjectDisposedException>(() => h.Surfaces.Create());
        second.Dispose();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AttachCallback(RootUi ui, out EntMut child)
    {
        var owner = new object();
        Node(ui, out child).OnUpdateF(() => GC.KeepAlive(owner));
        return new(owner);
    }

    private readonly record struct RootValue;
    private readonly record struct RootArch;
}
