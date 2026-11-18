namespace AlvorKit;

/// <summary>
/// Owns the default full-window UI surface and any opt-in independently scaled surfaces.
/// </summary>
public class UiSurfaces
{
    private readonly WindowCanvas canvas;
    private readonly UiScale scale;
    private readonly UiSurfaceActivation activation;
    private readonly UiSurfacePipeline pipeline;
    private readonly List<UiSurface> surfaces = [];
    private readonly UiSurface defaultSurface;

    internal UiSurfaces(
        WindowCanvas canvas,
        UiScale scale,
        UiContext context,
        UiUpdate update,
        SpriteBatchWriter sprites,
        UiClipping clipping)
    {
        this.canvas = canvas;
        this.scale = scale;
        activation = new(scale, context);
        var position = new UiPosition(sprites, scale);
        pipeline = new(this, context, update, new UiTraverse(), new UiSize(sprites, scale), position,
            new UiDraw(sprites, scale, position, clipping));
        defaultSurface = new(this, new UiRoot(), null, default, null, 1f, 0f, true, true);
        surfaces.Add(Default);
    }

    /// <summary>Raised when an additional surface joins the draw order.</summary>
    public event Action<UiSurface>? Added;

    /// <summary>Raised when an additional surface leaves the draw order.</summary>
    public event Action<UiSurface>? Removed;

    /// <summary>Gets the full-window surface.</summary>
    public UiSurface Default => defaultSurface;

    /// <summary>Gets all surfaces in ascending draw and input order without copying.</summary>
    public ReadOnlySpan<UiSurface> Span => CollectionsMarshal.AsSpan(surfaces);

    internal Box2 FullViewport => new(Vec2.Zero, canvas.Size);

    internal float DefaultScale => scale.DefaultScale;

    /// <summary>Creates a full-window UI surface with a fixed independent scale.</summary>
    public UiSurface Create(
        float scale = 1f,
        float order = 1f) =>
        CreateCore(
            null,
            default,
            null,
            scale,
            order,
            true);

    /// <summary>Creates a full-window UI surface whose independent scale is resolved each frame.</summary>
    public UiSurface Create(
        Func<float> scale,
        float order = 1f)
    {
        ArgumentNullException.ThrowIfNull(scale);
        return CreateCore(
            null,
            default,
            scale,
            1f,
            order,
            true);
    }

    /// <summary>Creates a dynamic-viewport UI surface with a fixed independent scale.</summary>
    public UiSurface Create(
        Func<Box2> viewport,
        float scale = 1f,
        float order = 1f)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        return CreateCore(
            viewport,
            default,
            null,
            scale,
            order,
            false);
    }

    /// <summary>Creates a dynamic-viewport UI surface whose independent scale is resolved each frame.</summary>
    public UiSurface Create(
        Func<Box2> viewport,
        Func<float> scale,
        float order = 1f)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        ArgumentNullException.ThrowIfNull(scale);
        return CreateCore(
            viewport,
            default,
            scale,
            1f,
            order,
            false);
    }

    /// <summary>Creates a fixed-viewport UI surface with a fixed independent scale.</summary>
    public UiSurface Create(
        Box2 viewport,
        float scale = 1f,
        float order = 1f) =>
        CreateCore(
            null,
            viewport,
            null,
            scale,
            order,
            false);

    /// <summary>Creates a fixed-viewport UI surface whose independent scale is resolved each frame.</summary>
    public UiSurface Create(
        Box2 viewport,
        Func<float> scale,
        float order = 1f)
    {
        ArgumentNullException.ThrowIfNull(scale);
        return CreateCore(
            null,
            viewport,
            scale,
            1f,
            order,
            false);
    }

    internal void PrepareAll()
    {
        foreach (var surface in Span)
            pipeline.Prepare(surface);
    }

    internal void Prepare(UiSurface surface) =>
        pipeline.Prepare(surface);

    internal void Draw(UiSurface surface) =>
        pipeline.Draw(surface);

    internal void UpdateAll(UiUpdate update)
    {
        foreach (var surface in Span)
        {
            var active = Activate(surface);
            try
            {
                update.Update(surface.Root);
                surface.Root.Cleanup();
            }
            finally
            {
                Restore(active);
            }
        }
    }

    internal UiSurfaceActiveState Activate(UiSurface surface) =>
        activation.Activate(surface);

    internal void Restore(UiSurfaceActiveState state) =>
        activation.Restore(state);

    internal void Remove(UiSurface surface)
    {
        if (ReferenceEquals(surface, Default))
            throw new InvalidOperationException("The default UI surface cannot be removed.");

        var index = surfaces.IndexOf(surface);
        if (index < 0)
            return;

        RemoveAt(index);
    }

    /// <summary>Releases every UI tree owned by this runtime.</summary>
    internal void Dispose()
    {
        while (surfaces.Count > 0)
            RemoveAt(surfaces.Count - 1);
    }

    private void RemoveAt(int index)
    {
        var surface = surfaces[index];
        surfaces.RemoveAt(index);
        surface.Root.Dispose();
        surface.MarkDisposed();

        if (surface != Default)
            Removed?.Invoke(surface);
    }

    private UiSurface CreateCore(
        Func<Box2>? viewport,
        Box2 fixedViewport,
        Func<float>? scale,
        float fixedScale,
        float order,
        bool usesDefaultViewport)
    {
        ObjectDisposedException.ThrowIf(!Default.Root.IsAlive, this);
        if (scale is null && (!float.IsFinite(fixedScale) || fixedScale <= 0f))
            throw new ArgumentOutOfRangeException(nameof(fixedScale));
        if (!float.IsFinite(order))
            throw new ArgumentOutOfRangeException(nameof(order));

        var surface = new UiSurface(
            this,
            new(),
            viewport,
            fixedViewport,
            scale,
            fixedScale,
            order,
            usesDefaultViewport,
            false);
        surfaces.Add(surface);
        surfaces.Sort(static (left, right) => left.Order.CompareTo(right.Order));
        Added?.Invoke(surface);
        return surface;
    }
}
