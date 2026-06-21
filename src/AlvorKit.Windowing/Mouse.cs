namespace AlvorKit.Windowing;

/// <summary>Reads mouse button, wheel, cursor, and motion state from a window loop.</summary>
/// <param name="window">The window loop that owns the mouse state.</param>
/// <param name="main">The button used by main-button convenience methods.</param>
/// <param name="secondary">The button used by secondary-button convenience methods.</param>
public sealed class Mouse(WindowLoop window, MouseButton main = MouseButton.Left, MouseButton secondary = MouseButton.Right)
{
    /// <summary>Gets the primary mouse button used by convenience methods.</summary>
    public MouseButton Main { get; } = main;

    /// <summary>Gets the secondary mouse button used by convenience methods.</summary>
    public MouseButton Secondary { get; } = secondary;

    /// <summary>Gets the cursor capture and visibility mode.</summary>
    public CursorMode CursorMode => window.Mouse.CursorMode;

    /// <summary>Gets the cursor position in window coordinates.</summary>
    public Vec2 Position => window.MousePosition.Position;

    /// <summary>Gets the tracked cursor delta for the current update.</summary>
    public Vec2 Delta => window.MousePosition.Delta;

    /// <summary>Gets the mouse wheel offset for the current tick.</summary>
    public Vec2 Wheel => window.Mouse.Wheel;

    /// <summary>Gets whether cursor delta tracking is active.</summary>
    public bool Track => window.MousePosition.Track;

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
}
