namespace AlvorKit.Windowing;

/// <summary>Reads mouse button, wheel, cursor, and motion state from a window loop.</summary>
public sealed class Mouse(WindowLoop window)
{
    /// <summary>Gets or sets the primary mouse button used by convenience methods.</summary>
    public MouseButton Main { get; set; } = MouseButton.Left;

    /// <summary>Gets or sets the secondary mouse button used by convenience methods.</summary>
    public MouseButton Secondary { get; set; } = MouseButton.Right;

    /// <summary>Gets or sets the cursor capture and visibility mode.</summary>
    public CursorMode CursorMode
    {
        get => window.Mouse.CursorMode;
        set => window.Mouse.CursorMode = value;
    }

    /// <summary>Gets or sets the cursor position in window coordinates.</summary>
    public Vec2 Position
    {
        get => window.MousePosition.Position;
        set => window.MousePosition.Position = value;
    }

    /// <summary>Gets the tracked cursor delta for the current update.</summary>
    public Vec2 Delta => window.MousePosition.Delta;

    /// <summary>Gets the mouse wheel offset for the current tick.</summary>
    public Vec2 Wheel => window.Mouse.Wheel;

    /// <summary>Gets or sets whether cursor delta tracking is active.</summary>
    public ref bool Track => ref window.MousePosition.Track;

    /// <summary>Returns whether a mouse button is currently down.</summary>
    public bool IsButtonDown(MouseButton button) => window.Mouse.IsButtonDown(button);

    /// <summary>Returns whether a mouse button is currently up.</summary>
    public bool IsButtonUp(MouseButton button) => window.Mouse.IsButtonUp(button);

    /// <summary>Returns whether a mouse button transitioned down this tick.</summary>
    public bool IsButtonPressed(MouseButton button) => window.Mouse.IsButtonPressed(button);

    /// <summary>Returns whether the main mouse button is currently down.</summary>
    public bool IsMainDown() => IsButtonDown(Main);

    /// <summary>Returns whether the main mouse button is currently up.</summary>
    public bool IsMainUp() => IsButtonUp(Main);

    /// <summary>Returns whether the main mouse button transitioned down this tick.</summary>
    public bool IsMainPressed() => IsButtonPressed(Main);

    /// <summary>Returns whether the secondary mouse button is currently down.</summary>
    public bool IsSecondaryDown() => IsButtonDown(Secondary);

    /// <summary>Returns whether the secondary mouse button is currently up.</summary>
    public bool IsSecondaryUp() => IsButtonUp(Secondary);

    /// <summary>Returns whether the secondary mouse button transitioned down this tick.</summary>
    public bool IsSecondaryPressed() => IsButtonPressed(Secondary);

    /// <summary>Advances mouse state by one tick.</summary>
    public void Tick() => window.Mouse.Tick();
}
