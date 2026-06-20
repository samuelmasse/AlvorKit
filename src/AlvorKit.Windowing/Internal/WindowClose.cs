namespace AlvorKit.Windowing;

/// <summary>Tracks close state and makes unload callbacks idempotent.</summary>
internal sealed class WindowClose
{
    private readonly IWindowHost window;
    private readonly Action callback;
    private bool closing;

    /// <summary>Creates close tracking for a host window.</summary>
    internal WindowClose(IWindowHost window, Action callback)
    {
        this.window = window;
        this.callback = callback;
        window.Closing += OnClosing;
    }

    /// <summary>Gets whether the host window is exiting.</summary>
    internal bool IsExiting => window.IsExiting;

    /// <summary>Requests that the host window close.</summary>
    internal void Close() => window.Close();

    private void OnClosing()
    {
        if (closing)
            return;

        closing = true;
        callback.Invoke();
    }
}
