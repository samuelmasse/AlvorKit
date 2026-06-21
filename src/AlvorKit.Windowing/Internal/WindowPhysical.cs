namespace AlvorKit.Windowing;

/// <summary>Tracks drawable size and resize/move update suppression for a window loop.</summary>
internal sealed class WindowPhysical
{
    private readonly IWindowHost window;
    private readonly Func<bool> callback;
    private Vec2u size;
    private int skips;

    /// <summary>Creates a physical-size tracker for a host window.</summary>
    internal WindowPhysical(IWindowHost window, Func<bool> callback)
    {
        this.window = window;
        this.callback = callback;
        size = window.ClientSize;
        window.Resize += OnResize;
        window.Move += OnMove;
    }

    /// <summary>Gets or sets the drawable client size.</summary>
    internal Vec2u Size
    {
        get => size;
        set => window.ClientSize = value;
    }

    /// <summary>Gets the primary monitor work-area size.</summary>
    internal Vec2u MonitorSize => window.MonitorSize;

    /// <summary>Gets the primary monitor horizontal content scale.</summary>
    internal float MonitorScale => window.MonitorScale;

    /// <summary>Gets or sets how many update ticks should be skipped after resize or move.</summary>
    internal ref int Skips => ref skips;

    private void OnResize(WindowResizeEvent e)
    {
        if (e.Size == Vec2u.Zero)
            return;

        size = e.Size;

        if (callback.Invoke())
            skips = 2;
    }

    private void OnMove(WindowPositionEvent e) => skips = 2;
}
