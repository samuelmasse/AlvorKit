namespace AlvorKit.Windowing;

/// <summary>Changes input-related host state for a window loop.</summary>
/// <param name="window">The window loop that owns the input state.</param>
public class WindowInput(WindowLoop window)
{
    /// <summary>Gets or sets the host clipboard text.</summary>
    public string Clipboard
    {
        get => window.Text.Clipboard;
        set => window.Text.Clipboard = value;
    }

    /// <summary>Gets or sets the cursor capture and visibility mode.</summary>
    public CursorMode CursorMode
    {
        get => window.Mouse.CursorMode;
        set => window.Mouse.CursorMode = value;
    }

    /// <summary>Gets or sets the requested visual cursor shape.</summary>
    public CursorShape CursorShape
    {
        get => window.Mouse.CursorShape;
        set => window.Mouse.CursorShape = value;
    }

    /// <summary>Gets or sets the cursor position in window coordinates.</summary>
    public Vec2 MousePosition
    {
        get => window.MousePosition.Position;
        set => window.MousePosition.Position = value;
    }

    /// <summary>Gets or sets whether cursor delta tracking is active.</summary>
    public bool Track
    {
        get => window.MousePosition.Track;
        set => window.MousePosition.Track = value;
    }
}
