namespace AlvorKit.Windowing;

/// <summary>Tracks cursor position and frame-to-frame delta for a window loop.</summary>
internal sealed class WindowMousePosition
{
    private readonly IWindowHost window;
    private readonly WindowPhysical physical;
    private Vec2 position;
    private Vec2 delta;
    private Vec2u lastWindowSize;
    private Vec2 lastPosition;
    private int skips;
    private bool track;

    /// <summary>Creates a cursor tracker from host mouse movement.</summary>
    internal WindowMousePosition(IWindowHost window, WindowPhysical physical)
    {
        this.window = window;
        this.physical = physical;
        position = window.MousePosition;
        window.MouseMove += OnMouseMove;
    }

    /// <summary>Gets or sets the cursor position in window coordinates.</summary>
    internal Vec2 Position
    {
        get => position;
        set
        {
            position = value;
            lastPosition = value;
            window.MousePosition = value;
        }
    }

    /// <summary>Gets the tracked cursor delta.</summary>
    internal Vec2 Delta => delta;

    /// <summary>Gets or sets whether delta tracking is active.</summary>
    internal ref bool Track => ref track;

    /// <summary>Advances cursor delta state by one update.</summary>
    internal void Update()
    {
        var dp = position - lastPosition;

        if (physical.Size != lastWindowSize || !window.IsFocused)
        {
            lastWindowSize = physical.Size;
            delta = default;
            skips = 1;
        }
        else if (track)
        {
            delta = skips == 0 ? dp : default;

            if (skips > 0 && dp != Vec2.Zero)
                skips--;
        }
        else
        {
            delta = default;
            skips = 2;
        }

        lastPosition = position;
    }

    private void OnMouseMove(WindowMouseMoveEvent e) => position = e.Position;
}
