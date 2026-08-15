namespace AlvorKit;

/// <summary>Tracks fullscreen and vsync toggle state.</summary>
internal sealed class WindowToggle(IWindowHost window)
{
    private WindowState previousState = window.WindowState;
    private bool isVSyncEnabled = window.IsVSyncEnabled;
    private bool isVSyncForced;
    private bool isFullscreen = window.IsFullscreen;

    /// <summary>Gets whether the window is tracked as fullscreen.</summary>
    internal bool IsFullscreen => isFullscreen;

    /// <summary>Gets whether vertical synchronization is tracked as enabled.</summary>
    internal bool IsVSyncEnabled => isVSyncEnabled;

    /// <summary>Toggles between fullscreen and the previous window state.</summary>
    internal void ToggleFullscreen()
    {
        if (isFullscreen)
        {
            window.WindowState = previousState;
            isFullscreen = false;
        }
        else
        {
            previousState = window.WindowState;

            if (window.WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Normal;

            window.WindowState = WindowState.Fullscreen;
            isFullscreen = true;
        }

        window.IsVSyncEnabled = isVSyncEnabled || isVSyncForced;
    }

    /// <summary>Toggles vertical synchronization on or off.</summary>
    internal void ToggleVSync()
    {
        isVSyncEnabled = !isVSyncEnabled;
        window.IsVSyncEnabled = isVSyncEnabled || isVSyncForced;
    }

    /// <summary>Forces vertical synchronization while the window is unfocused without changing the tracked preference.</summary>
    internal void ApplyFocusVSync(bool isFocused)
    {
        var shouldForceVSync = !isFocused;
        if (isVSyncForced == shouldForceVSync)
            return;

        isVSyncForced = shouldForceVSync;
        window.IsVSyncEnabled = isVSyncEnabled || isVSyncForced;
    }
}
