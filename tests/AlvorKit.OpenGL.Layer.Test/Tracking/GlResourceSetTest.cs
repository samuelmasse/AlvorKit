namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests the typed GL resource lifetime tracker.
/// </summary>
[TestClass]
public unsafe class GlResourceSetTest
{
    [TestMethod]
    public void Track_SingleHandle_AddsItem()
    {
        var set = CreateSet();
        var handle = Buffer(7);

        set.Track(handle);

        Assert.AreEqual(1, set.Count);
        Assert.IsTrue(set.Contains(handle));
        Assert.IsTrue(set.Items.Contains(handle));
    }

    [TestMethod]
    public void Track_NativeIds_ConvertsAndAddsItems()
    {
        var set = CreateSet();
        uint* ids = stackalloc uint[] { 1, 2 };

        set.Track(2, (nint)ids);

        Assert.IsTrue(set.Contains(Buffer(1)));
        Assert.IsTrue(set.Contains(Buffer(2)));
    }

    [TestMethod]
    public void Untrack_SingleHandle_RemovesItem()
    {
        var set = CreateSet();
        var handle = Buffer(7);
        set.Track(handle);

        set.Untrack("Delete", handle);

        Assert.IsFalse(set.Contains(handle));
    }

    [TestMethod]
    public void Untrack_MissingHandle_Throws()
    {
        var set = CreateSet();

        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => set.Untrack("Delete", Buffer(7)));
    }

    [TestMethod]
    public void Untrack_NativeIds_ReturnsAndRemovesItems()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));
        uint* ids = stackalloc uint[] { 1, 2 };

        var removed = set.Untrack("Delete", 2, (nint)ids);

        CollectionAssert.AreEquivalent(new[] { Buffer(1), Buffer(2) }, removed);
        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void Untrack_NativeIdsWithMissingHandle_ThrowsBeforeRemoving()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        uint* ids = stackalloc uint[] { 1, 2 };

        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => set.Untrack("Delete", 2, (nint)ids));
        Assert.IsTrue(set.Contains(Buffer(1)));
    }

    [TestMethod]
    public void Drain_ReturnsAndClearsHandles()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));

        var drained = set.Drain();

        CollectionAssert.AreEquivalent(new[] { Buffer(1), Buffer(2) }, drained);
        Assert.AreEqual(0, set.Count);
    }

    [TestMethod]
    public void DrainIds_ReturnsRawIdsAndClearsHandles()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));

        var drained = set.DrainIds();

        CollectionAssert.AreEquivalent(new uint[] { 1, 2 }, drained);
        Assert.AreEqual(0, set.Count);
    }

    private static GlResourceSet<GlBufferHandle> CreateSet() =>
        new("buffer", id => (GlBufferHandle)id, handle => (uint)handle);

    private static GlBufferHandle Buffer(uint id) => (GlBufferHandle)id;
}
