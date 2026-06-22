namespace AlvorKit.Engine;

/// <summary>Root-scoped input mutation facade for clipboard, cursor mode, and cursor tracking.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootInput(WindowLoop window)
{
    private readonly WindowInput input = new(window);

    /// <summary>Gets or sets the host clipboard text.</summary>
    public string Clipboard { get => input.Clipboard; set => input.Clipboard = value; }

    /// <summary>Gets or sets cursor capture and visibility.</summary>
    public CursorMode CursorMode { get => input.CursorMode; set => input.CursorMode = value; }

    /// <summary>Gets or sets the cursor position in window coordinates.</summary>
    public Vec2 MousePosition { get => input.MousePosition; set => input.MousePosition = value; }

    /// <summary>Gets or sets whether cursor delta tracking is active.</summary>
    public bool Track { get => input.Track; set => input.Track = value; }
}
