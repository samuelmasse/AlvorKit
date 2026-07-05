namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public sealed class GlLayerHierarchyTest
{
    private RecordingGl inner = null!;
    private GlLayer root = null!;

    [TestInitialize]
    public void Setup()
    {
        inner = new RecordingGl();
        root = new GlLayer(inner);
    }

    /// <summary>Objects created through a child are owned by the child, not the parent.</summary>
    [TestMethod]
    public void ChildCreations_LandInTheChildSets()
    {
        var child = new ChildGl(root);
        var buffer = child.GenBuffer();
        var texture = child.GenTexture();

        Assert.IsTrue(child.Buffers.Contains(buffer));
        Assert.IsTrue(child.Textures.Contains(texture));
        Assert.IsFalse(root.Buffers.Contains(buffer));
        Assert.IsFalse(root.Textures.Contains(texture));
    }

    /// <summary>The children list follows construction and disposal.</summary>
    [TestMethod]
    public void Children_TrackTopology()
    {
        var first = new ChildGl(root);
        var second = new ChildGl(first);

        Assert.AreEqual(root, first.Parent);
        Assert.AreEqual(first, second.Parent);
        Assert.IsNull(root.Parent);
        CollectionAssert.AreEqual(new[] { first }, root.Children.ToArray());
        CollectionAssert.AreEqual(new[] { second }, first.Children.ToArray());

        first.Dispose();

        Assert.AreEqual(0, root.Children.Count);
        Assert.AreEqual(0, first.Children.Count);
    }

    /// <summary>Disposing a child deletes its objects and leaves the rest of the tree alive.</summary>
    [TestMethod]
    public void ChildDispose_DeletesOwnObjectsOnly()
    {
        var child = new ChildGl(root);
        var rootBuffer = root.GenBuffer();
        var childBuffer = child.GenBuffer();

        child.Dispose();

        CollectionAssert.AreEqual(new[] { (uint)childBuffer }, inner.Deleted);
        Assert.IsTrue(root.Buffers.Contains(rootBuffer));
    }

    /// <summary>Disposing a parent disposes children first, newest child first.</summary>
    [TestMethod]
    public void ParentDispose_DisposesChildrenNewestFirst()
    {
        var older = new ChildGl(root);
        var newer = new ChildGl(root);
        var rootBuffer = root.GenBuffer();
        var olderBuffer = older.GenBuffer();
        var newerBuffer = newer.GenBuffer();

        root.Dispose();

        CollectionAssert.AreEqual(new[] { (uint)newerBuffer, (uint)olderBuffer, (uint)rootBuffer }, inner.Deleted);
        Assert.AreEqual(0, root.Children.Count);
    }

    /// <summary>A child may delete an object its scope inherited from an ancestor.</summary>
    [TestMethod]
    public void ChildDelete_ResolvesAncestorOwnership()
    {
        var child = new ChildGl(root);
        var buffer = root.GenBuffer();

        child.DeleteBuffer(buffer);

        Assert.IsFalse(root.Buffers.Contains(buffer));
        CollectionAssert.AreEqual(new[] { (uint)buffer }, inner.Deleted);
    }

    /// <summary>Deleting an object owned outside the node's ancestry throws.</summary>
    [TestMethod]
    public void ForeignDelete_Throws()
    {
        var first = new ChildGl(root);
        var second = new ChildGl(root);
        var buffer = first.GenBuffer();

        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => second.DeleteBuffer(buffer));
        Assert.IsTrue(first.Buffers.Contains(buffer));
    }

    /// <summary>Disposing a child that still owns a bound object throws instead of force-unbinding.</summary>
    [TestMethod]
    public void ChildDispose_WithBoundObjectThrows()
    {
        var child = new ChildGl(root);
        var buffer = child.GenBuffer();
        child.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);

        Assert.Throws<GlBindConflictException>(child.Dispose);

        child.UnbindBuffer(GlBufferTarget.ArrayBuffer);
    }

    /// <summary>Dispose is idempotent for children and roots.</summary>
    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        var child = new ChildGl(root);
        child.GenBuffer();

        child.Dispose();
        child.Dispose();
        root.Dispose();
        root.Dispose();

        Assert.AreEqual(1, inner.Deleted.Count);
    }

    /// <summary>Bind validation state is shared across every node of the hierarchy.</summary>
    [TestMethod]
    public void BindValidation_IsSharedAcrossNodes()
    {
        var child = new ChildGl(root);
        var childBuffer = child.GenBuffer();
        var rootBuffer = root.GenBuffer();

        child.BindBuffer(GlBufferTarget.ArrayBuffer, childBuffer);

        Assert.Throws<GlAlreadyBoundException>(() => root.BindBuffer(GlBufferTarget.ArrayBuffer, rootBuffer));

        root.UnbindBuffer(GlBufferTarget.ArrayBuffer);
    }

    /// <summary>Tracked buffer memory drops back to zero when the owning child is disposed.</summary>
    [TestMethod]
    public void BufferUsage_DropsOnChildDispose()
    {
        var child = new ChildGl(root);
        var buffer = child.GenBuffer();
        child.BindBuffer(GlBufferTarget.ArrayBuffer, buffer);
        child.BufferData(GlBufferTarget.ArrayBuffer, 1024, 0, GlBufferUsage.StaticDraw);
        child.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        Assert.AreEqual(1024, root.BufferUsage);
        Assert.AreEqual(1024, child.BufferUsage);

        child.Dispose();

        Assert.AreEqual(0, root.BufferUsage);
    }

    private sealed class ChildGl(GlLayer parent) : GlLayer(parent);
}
