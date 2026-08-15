namespace AlvorKit;

/// <summary>Tests reusable Blend scroll offset positioning.</summary>
[TestClass]
public sealed class BlendScrollHandleTest
{
    /// <summary>An interval below the viewport advances only far enough to reveal its maximum.</summary>
    [TestMethod]
    public void EnsureVisible_WithIntervalBelowViewport_RevealsMaximum()
    {
        BlendScrollHandle handle = new();

        handle.EnsureVisible(120f, 160f, 100f);

        Assert.AreEqual(60f, handle.Offset);
    }

    /// <summary>An interval above the current viewport moves the offset back to its minimum.</summary>
    [TestMethod]
    public void EnsureVisible_WithIntervalAboveViewport_RevealsMinimum()
    {
        BlendScrollHandle handle = new();
        handle.EnsureVisible(120f, 160f, 100f);

        handle.EnsureVisible(20f, 40f, 100f);

        Assert.AreEqual(20f, handle.Offset);
    }

    /// <summary>An already visible interval leaves the current offset unchanged.</summary>
    [TestMethod]
    public void EnsureVisible_WithVisibleInterval_PreservesOffset()
    {
        BlendScrollHandle handle = new();
        handle.EnsureVisible(120f, 160f, 100f);

        handle.EnsureVisible(70f, 100f, 100f);

        Assert.AreEqual(60f, handle.Offset);
    }

    /// <summary>Reset returns a reused handle to the beginning.</summary>
    [TestMethod]
    public void Reset_AfterScrolling_ClearsOffset()
    {
        BlendScrollHandle handle = new();
        handle.EnsureVisible(120f, 160f, 100f);

        handle.Reset();

        Assert.AreEqual(0f, handle.Offset);
    }
}
