namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests the typed GL resource lifetime tracker.
/// </summary>
[TestClass]
public unsafe class GlResourceSetTest
{
    /// <summary>Tracking a single typed handle adds it to the live set.</summary>
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

    /// <summary>Tracking native ids converts them to typed handles in the live set.</summary>
    [TestMethod]
    public void Track_NativeIds_ConvertsAndAddsItems()
    {
        var set = CreateSet();
        uint* ids = stackalloc uint[] { 1, 2 };

        set.Track(2, (nint)ids);

        Assert.IsTrue(set.Contains(Buffer(1)));
        Assert.IsTrue(set.Contains(Buffer(2)));
    }

    /// <summary>Untracking a single live handle removes it from the live set.</summary>
    [TestMethod]
    public void Untrack_SingleHandle_RemovesItem()
    {
        var set = CreateSet();
        var handle = Buffer(7);
        set.Track(handle);

        set.Untrack("Delete", handle);

        Assert.IsFalse(set.Contains(handle));
    }

    /// <summary>Untracking a missing handle reports the missing resource.</summary>
    [TestMethod]
    public void Untrack_MissingHandle_Throws()
    {
        var set = CreateSet();

        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => set.Untrack("Delete", Buffer(7)));
    }

    /// <summary>Untracking native ids returns a non-copying span and removes every handle.</summary>
    [TestMethod]
    public void Untrack_NativeIds_ReturnsAndRemovesItems()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));
        uint* ids = stackalloc uint[] { 1, 2 };

        var removed = set.Untrack("Delete", 2, (nint)ids);

        Assert.IsTrue(removed.SequenceEqual([Buffer(1), Buffer(2)]));
        Assert.AreEqual(0, set.Count);
    }

    /// <summary>Untracking native ids validates the entire span before removing anything.</summary>
    [TestMethod]
    public void Untrack_NativeIdsWithMissingHandle_ThrowsBeforeRemoving()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        uint* ids = stackalloc uint[] { 1, 2 };

        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => set.Untrack("Delete", 2, (nint)ids));
        Assert.IsTrue(set.Contains(Buffer(1)));
    }

    /// <summary>Draining one handle removes a tracked value without requiring a snapshot.</summary>
    [TestMethod]
    public void TryDrain_WhenTracked_RemovesOneHandle()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));

        Assert.IsTrue(set.TryDrain(out var drained));

        Assert.IsFalse(set.Contains(drained));
        Assert.AreEqual(1, set.Count);
    }

    /// <summary>Draining one handle reports an empty set without changing it.</summary>
    [TestMethod]
    public void TryDrain_WhenEmpty_ReturnsFalse()
    {
        var set = CreateSet();

        Assert.IsFalse(set.TryDrain(out _));
    }

    /// <summary>Draining raw ids writes them into caller-owned storage and clears the drained handles.</summary>
    [TestMethod]
    public void DrainIds_WritesRawIdsToCallerOwnedSpan()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));
        Span<uint> drained = stackalloc uint[2];

        var count = set.DrainIds(drained);

        Assert.AreEqual(2, count);
        CollectionAssert.AreEquivalent(new uint[] { 1, 2 }, drained.ToArray());
        Assert.AreEqual(0, set.Count);
    }

    /// <summary>Draining raw ids only removes the handles that fit in the caller-owned buffer.</summary>
    [TestMethod]
    public void DrainIds_WhenSpanIsSmaller_RemovesOneChunk()
    {
        var set = CreateSet();
        set.Track(Buffer(1));
        set.Track(Buffer(2));
        Span<uint> drained = stackalloc uint[1];

        Assert.AreEqual(1, set.DrainIds(drained));
        Assert.AreEqual(1, set.Count);
    }

    /// <summary>Tracking rejects unmanaged handles that do not match the generated single-uint handle layout.</summary>
    [TestMethod]
    public void Track_WhenHandleIsNotSingleUint_Throws()
    {
        var set = new GlResourceSet<ulong>("wide");
        uint* ids = stackalloc uint[] { 1 };

        Assert.Throws<InvalidOperationException>(() => set.Track(1, (nint)ids));
    }

    private static GlResourceSet<GlBufferHandle> CreateSet() => new("buffer");

    private static GlBufferHandle Buffer(uint id) => (GlBufferHandle)id;
}
