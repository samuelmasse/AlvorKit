namespace AlvorKit;

/// <summary>Describes the physical viewport and logical extent of the UI surface currently being processed.</summary>
[Root]
public sealed class RootUiContext(
    RootCanvas canvas,
    RootUiScale scale)
{
    private Box2 viewport;
    private bool isActive;

    /// <summary>Gets the active physical canvas viewport using top-left-origin coordinates.</summary>
    public Box2 Viewport =>
        isActive
            ? viewport
            : new(Vec2.Zero, canvas.Size);

    /// <summary>Gets the active surface origin in physical canvas coordinates.</summary>
    public Vec2 Origin => Viewport.Min;

    /// <summary>Gets the active surface size in logical UI coordinates.</summary>
    public Vec2 Size => Viewport.Size / scale.Scale;

    internal ActiveState Activate(Box2 value)
    {
        var previous = new ActiveState(isActive, viewport);
        isActive = true;
        viewport = value;
        return previous;
    }

    internal void Restore(ActiveState state)
    {
        isActive = state.IsActive;
        viewport = state.Viewport;
    }

    internal readonly record struct ActiveState(
        bool IsActive,
        Box2 Viewport);
}
