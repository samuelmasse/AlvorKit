namespace AlvorKit.Engine;

/// <summary>Root-scoped mouse reader for buttons, wheel, position, and motion.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootMouse(WindowLoop window)
{
    private readonly Mouse mouse = new(window);

    /// <summary>Gets the primary mouse button used by convenience methods.</summary>
    public MouseButton Main => mouse.Main;

    /// <summary>Gets the secondary mouse button used by convenience methods.</summary>
    public MouseButton Secondary => mouse.Secondary;

    /// <summary>Gets cursor capture and visibility.</summary>
    public CursorMode CursorMode => mouse.CursorMode;

    /// <summary>Gets the cursor position in window coordinates.</summary>
    public Vec2 Position => mouse.Position;

    /// <summary>Gets the tracked cursor delta for the current update.</summary>
    public Vec2 Delta => mouse.Delta;

    /// <summary>Gets the mouse wheel offset for the current tick.</summary>
    public Vec2 Wheel => mouse.Wheel;

    /// <summary>Gets whether cursor delta tracking is active.</summary>
    public bool Track => mouse.Track;

    /// <summary>Returns whether a mouse button is currently down.</summary>
    public bool IsButtonDown(MouseButton button) => mouse.IsButtonDown(button);

    /// <summary>Returns whether a mouse button is currently up.</summary>
    public bool IsButtonUp(MouseButton button) => mouse.IsButtonUp(button);

    /// <summary>Returns whether a mouse button transitioned down this tick.</summary>
    public bool IsButtonPressed(MouseButton button) => mouse.IsButtonPressed(button);

    /// <summary>Returns whether the main mouse button is currently down.</summary>
    public bool IsMainDown() => mouse.IsMainDown();

    /// <summary>Returns whether the main mouse button is currently up.</summary>
    public bool IsMainUp() => mouse.IsMainUp();

    /// <summary>Returns whether the main mouse button transitioned down this tick.</summary>
    public bool IsMainPressed() => mouse.IsMainPressed();

    /// <summary>Returns whether the secondary mouse button is currently down.</summary>
    public bool IsSecondaryDown() => mouse.IsSecondaryDown();

    /// <summary>Returns whether the secondary mouse button is currently up.</summary>
    public bool IsSecondaryUp() => mouse.IsSecondaryUp();

    /// <summary>Returns whether the secondary mouse button transitioned down this tick.</summary>
    public bool IsSecondaryPressed() => mouse.IsSecondaryPressed();
}
