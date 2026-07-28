namespace AlvorKit.UI.Test;

/// <summary>Covers independently scaled UI roots while preserving the default full-window path.</summary>
[TestClass]
public sealed class RootUiSurfacesTest
{
    /// <summary>The existing RootUi and RootUiScale API remains the automatically managed default surface.</summary>
    [TestMethod]
    public void DefaultSurface_UsesExistingRootAndScale()
    {
        var h = new UiTestHarness();
        h.Scale.Scale = 2f;
        Node(h.Ui, out var fill)
            .SizeRelativeV((1, 1));

        h.Update();

        Assert.AreSame(h.Ui, h.Surfaces.Default.Root);
        Assert.AreEqual(new Vec2(400, 300), h.Ui.SizeR);
        Assert.AreEqual(h.Ui.SizeR, fill.SizeR);
        Assert.AreEqual(Vec2.Zero, h.Context.Origin);
        Assert.AreEqual(new Vec2(400, 300), h.Script.DrawArea);
    }

    /// <summary>Every public creation shape resolves its fixed and dynamic viewport and scale inputs.</summary>
    [TestMethod]
    public void CreateOverloads_ResolveViewportScaleAndOrder()
    {
        var h = new UiTestHarness();
        var viewport = new Box2((100, 50), (500, 250));
        var scale = 2f;
        using var fullFixed = h.Surfaces.Create(
            scale: 4f,
            order: 4f);
        using var fullDynamic = h.Surfaces.Create(
            () => scale,
            order: 3f);
        using var viewportFixed = h.Surfaces.Create(
            () => viewport,
            scale: 3f,
            order: 2f);
        using var viewportDynamic = h.Surfaces.Create(
            () => viewport,
            () => scale,
            order: 1f);

        Assert.AreEqual(new Box2(Vec2.Zero, (800, 600)), fullFixed.CurrentViewport);
        Assert.AreEqual(4f, fullFixed.CurrentScale);
        Assert.AreEqual(new Box2(Vec2.Zero, (800, 600)), fullDynamic.CurrentViewport);
        Assert.AreEqual(2f, fullDynamic.CurrentScale);
        Assert.AreEqual(viewport, viewportFixed.CurrentViewport);
        Assert.AreEqual(3f, viewportFixed.CurrentScale);
        Assert.AreEqual(viewport, viewportDynamic.CurrentViewport);
        Assert.AreEqual(2f, viewportDynamic.CurrentScale);
        CollectionAssert.AreEqual(
            new[] { 0f, 1f, 2f, 3f, 4f },
            h.Surfaces.Span.ToArray()
                .Select(static surface => surface.Order)
                .ToArray());
    }

    /// <summary>Creation rejects missing delegates, invalid fixed scales, and non-finite ordering.</summary>
    [TestMethod]
    public void Create_InvalidArguments_Throws()
    {
        var h = new UiTestHarness();
        var viewport = new Box2((100, 50), (500, 250));

        Assert.ThrowsException<ArgumentNullException>(
            () => h.Surfaces.Create((Func<float>)null!));
        Assert.ThrowsException<ArgumentNullException>(
            () => h.Surfaces.Create((Func<Box2>)null!));
        Assert.ThrowsException<ArgumentNullException>(
            () => h.Surfaces.Create(viewport, (Func<float>)null!));
        Assert.ThrowsException<ArgumentNullException>(
            () => h.Surfaces.Create(() => viewport, (Func<float>)null!));
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => h.Surfaces.Create(scale: 0f));
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => h.Surfaces.Create(order: float.NaN));
    }

    /// <summary>A dynamic surface scale relayouts the same tree without rebuilding it.</summary>
    [TestMethod]
    public void DynamicScale_RelayoutsExistingSurfaceTree()
    {
        var h = new UiTestHarness();
        var scale = 2f;
        using var surface = h.Surfaces.Create(
            new Box2((100, 50), (500, 250)),
            () => scale,
            order: 10f);
        Node(surface.Root, out var fill)
            .SizeRelativeV((1, 1));

        h.Update();
        Assert.AreEqual(new Vec2(200, 100), surface.Root.SizeR);
        Assert.AreEqual(surface.Root.SizeR, fill.SizeR);

        scale = 1f;
        h.Update();
        Assert.AreEqual(new Vec2(400, 200), surface.Root.SizeR);
        Assert.AreEqual(surface.Root.SizeR, fill.SizeR);
    }

    /// <summary>Surface callbacks observe their own scale and physical viewport through root UI services.</summary>
    [TestMethod]
    public void UpdateCallback_ObservesActiveSurfaceContext()
    {
        var h = new UiTestHarness();
        var viewport = new Box2((120, 80), (520, 280));
        using var surface = h.Surfaces.Create(
            viewport,
            scale: 1.5f,
            order: 10f);
        var observedScale = 0f;
        var observedViewport = default(Box2);
        Node(surface.Root)
            .OnUpdateF(() =>
            {
                observedScale = h.Scale.Scale;
                observedViewport = h.Context.Viewport;
            });

        h.Update();

        Assert.AreEqual(1.5f, observedScale);
        Assert.AreEqual(viewport, observedViewport);
        Assert.AreEqual(1f, h.Scale.Scale);
        Assert.AreEqual(new Box2(Vec2.Zero, (800, 600)), h.Context.Viewport);
    }

    /// <summary>An invalid dynamic scale restores the previously active surface context before failing.</summary>
    [TestMethod]
    public void DynamicScale_InvalidValue_RestoresSurfaceContext()
    {
        var h = new UiTestHarness();
        using var surface = h.Surfaces.Create(
            new Box2((100, 50), (500, 250)),
            () => 0f,
            order: 10f);

        Assert.ThrowsException<InvalidOperationException>(() => h.Update());
        Assert.AreEqual(1f, h.Scale.Scale);
        Assert.AreEqual(
            new Box2(Vec2.Zero, (800, 600)),
            h.Context.Viewport);
    }

    /// <summary>
    /// Mouse coordinates are converted into each surface, and transparent space on a higher
    /// surface falls through to a lower surface.
    /// </summary>
    [TestMethod]
    public void MouseDispatch_UsesSurfaceCoordinatesAndFallsThroughTransparentSpace()
    {
        var h = new UiTestHarness();
        var viewport = new Box2((100, 50), (500, 250));
        using var lower = h.Surfaces.Create(viewport, scale: 2f, order: 10f);
        using var upper = h.Surfaces.Create(viewport, scale: 2f, order: 20f);
        var lowerClicks = 0;
        var upperClicks = 0;
        Node(lower.Root, out var lowerButton)
            .SizeRelativeV((1, 1))
            .IsSelectableV(true)
            .OnClickF(() => lowerClicks++);
        Node(upper.Root)
            .SizeRelativeV((0, 0))
            .SizeV((50, 50))
            .IsSelectableV(true)
            .OnClickF(() => upperClicks++);

        h.MoveMouse((300, 150));
        h.Update();
        Assert.AreEqual(new Vec2(200, 100), lower.Root.SizeR);
        Assert.AreEqual(lower.Root.SizeR, lowerButton.SizeR);
        Assert.AreEqual(Vec2.Zero, lowerButton.PositionR);
        Assert.IsTrue(lowerButton.IsSelectableFV.Resolve());
        Assert.AreEqual(new Vec2(100, 50), h.UiMouse.Position);
        Assert.AreEqual(lowerButton, h.UiMouse.Hovered);
        PressAndRelease(h);
        Assert.AreEqual(1, lowerClicks);
        Assert.AreEqual(0, upperClicks);

        Click(h, (150, 100));
        Assert.AreEqual(1, lowerClicks);
        Assert.AreEqual(1, upperClicks);
        Assert.AreEqual(new Vec2(25, 25), h.UiMouse.Position);
    }

    /// <summary>Disposing a surface removes both its tree and its draw script.</summary>
    [TestMethod]
    public void Dispose_RemovesSurfaceAndDrawScript()
    {
        var h = new UiTestHarness();
        var surface = h.Surfaces.Create(
            new Box2((0, 0), (100, 100)),
            order: 10f);

        Assert.AreEqual(2, h.Surfaces.Span.Length);
        Assert.AreEqual(1, h.Scripts.Span.Length);
        Assert.AreEqual(
            new Box2((0, 0), (100, 100)),
            h.Scripts.Span[0].DrawViewport);
        Assert.AreEqual(
            new Vec2(100, 100),
            h.Scripts.Span[0].DrawArea);

        surface.Dispose();
        surface.Dispose();

        Assert.AreEqual(1, h.Surfaces.Span.Length);
        Assert.AreEqual(0, h.Scripts.Span.Length);
    }

    /// <summary>The automatically owned default surface cannot be disposed by an app.</summary>
    [TestMethod]
    public void DefaultSurface_Dispose_Throws()
    {
        var h = new UiTestHarness();

        Assert.ThrowsException<InvalidOperationException>(
            h.Surfaces.Default.Dispose);
        Assert.AreEqual(1, h.Surfaces.Span.Length);
    }

    /// <summary>A surface ordered below the default prepares its own retained tree before drawing.</summary>
    [TestMethod]
    public void BelowDefaultSurface_Draw_PreparesLayout()
    {
        var h = new UiTestHarness();
        using var surface = h.Surfaces.Create(
            new Box2((50, 25), (250, 125)),
            scale: 2f,
            order: -10f);
        Node(surface.Root, out var fill)
            .SizeRelativeV((1, 1));

        h.Scripts.Span[0].Draw();

        Assert.AreEqual(new Vec2(100, 50), surface.Root.SizeR);
        Assert.AreEqual(surface.Root.SizeR, fill.SizeR);
    }

    private static void Click(
        UiTestHarness harness,
        Vec2 position)
    {
        harness.MoveMouse(position);
        harness.Update();
        PressAndRelease(harness);
    }

    private static void PressAndRelease(
        UiTestHarness harness)
    {
        harness.Host.RaiseMouseDown(MouseButton.Left);
        harness.Update();
        harness.Host.RaiseMouseUp(MouseButton.Left);
        harness.Update();
    }
}
