namespace AlvorKit.UI;

/// <summary>
/// Owns an independent UI tree whose layout, text, clipping, input, and drawing share one scale and viewport.
/// </summary>
public sealed class UiSurface : IDisposable
{
    private readonly RootUiSurfaces owner;
    private readonly Func<Box2>? viewport;
    private readonly Func<float>? scale;
    private readonly Box2 fixedViewport;
    private readonly float fixedScale;
    private readonly bool usesDefaultViewport;
    private readonly bool usesDefaultScale;
    private bool isDisposed;

    internal UiSurface(
        RootUiSurfaces owner,
        RootUi root,
        Func<Box2>? viewport,
        Box2 fixedViewport,
        Func<float>? scale,
        float fixedScale,
        float order,
        bool usesDefaultViewport,
        bool usesDefaultScale)
    {
        this.owner = owner;
        Root = root;
        this.viewport = viewport;
        this.fixedViewport = fixedViewport;
        this.scale = scale;
        this.fixedScale = fixedScale;
        Order = order;
        this.usesDefaultViewport = usesDefaultViewport;
        this.usesDefaultScale = usesDefaultScale;
    }

    /// <summary>Gets the entity root on which this surface's ordinary UI tree is mounted.</summary>
    public RootUi Root { get; }

    /// <summary>Gets the order used for drawing and hit testing; higher surfaces are drawn and hit tested later.</summary>
    public float Order { get; }

    /// <summary>Gets the currently resolved physical canvas viewport.</summary>
    public Box2 CurrentViewport => ResolveViewport();

    /// <summary>Gets the currently resolved physical-pixels-per-logical-unit scale.</summary>
    public float CurrentScale => ResolveScale();

    /// <summary>Gets the currently resolved logical size.</summary>
    public Vec2 Size => CurrentViewport.Size / CurrentScale;

    /// <summary>Removes the surface and its drawing script. The default surface cannot be disposed.</summary>
    public void Dispose()
    {
        if (isDisposed)
            return;

        owner.Remove(this);
        isDisposed = true;
    }

    internal Box2 ResolveViewport() =>
        usesDefaultViewport
            ? owner.FullViewport
            : viewport?.Invoke() ?? fixedViewport;

    internal float ResolveScale()
    {
        var value = usesDefaultScale
            ? owner.DefaultScale
            : scale?.Invoke() ?? fixedScale;
        if (!float.IsFinite(value) || value <= 0f)
        {
            throw new InvalidOperationException(
                $"UI surface scale must be finite and greater than zero, but was {value}.");
        }

        return value;
    }

    internal void MarkDisposed() => isDisposed = true;
}
