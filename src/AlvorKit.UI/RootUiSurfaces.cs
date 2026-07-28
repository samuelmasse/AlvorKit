namespace AlvorKit.UI;

/// <summary>
/// Owns the default full-window UI surface and any opt-in independently scaled surfaces.
/// Existing <see cref="RootUi"/> trees automatically remain on <see cref="Default"/>.
/// </summary>
[Root]
public sealed class RootUiSurfaces
{
    private readonly RootCanvas canvas;
    private readonly RootScripts scripts;
    private readonly RootUiScale scale;
    private readonly UiSurfaceActivation activation;
    private readonly UiSurfacePipeline pipeline;
    private readonly List<UiSurface> surfaces = [];
    private readonly Dictionary<UiSurface, UiSurfaceScript> drawScripts = [];

    public RootUiSurfaces(
        RootCanvas canvas,
        RootScripts scripts,
        RootUi ui,
        RootUiScale scale,
        RootUiContext context,
        RootUiTraverse traverse,
        RootUiSize size,
        RootUiPosition position,
        RootUiDraw draw)
    {
        this.canvas = canvas;
        this.scripts = scripts;
        this.scale = scale;
        activation = new(scale, context);
        pipeline = new(
            this,
            context,
            traverse,
            size,
            position,
            draw);
        Default = new(
            this,
            ui,
            null,
            default,
            null,
            1f,
            0f,
            true,
            true);
        surfaces.Add(Default);
    }

    /// <summary>
    /// Gets the full-window surface used by the existing <see cref="RootUi"/>,
    /// <see cref="RootUiScale"/>, and <see cref="RootUiScript"/> API.
    /// </summary>
    public UiSurface Default { get; }

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

    internal void DrawDefault() => pipeline.Draw(Default);

    internal void UpdateAll(RootUiUpdate update)
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

        surfaces.RemoveAt(index);
        if (drawScripts.TryGetValue(surface, out var script))
            scripts.Remove(script);
        surface.MarkDisposed();
    }

    internal void RemoveDrawScript(UiSurface surface) =>
        drawScripts.Remove(surface);

    private UiSurface CreateCore(
        Func<Box2>? viewport,
        Box2 fixedViewport,
        Func<float>? scale,
        float fixedScale,
        float order,
        bool usesDefaultViewport)
    {
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
        var script = new UiSurfaceScript(this, surface);
        drawScripts.Add(surface, script);
        surfaces.Add(surface);
        surfaces.Sort(static (left, right) => left.Order.CompareTo(right.Order));
        scripts.Add(script);
        return surface;
    }

}
