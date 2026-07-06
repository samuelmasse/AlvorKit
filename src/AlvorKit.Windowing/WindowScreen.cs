namespace AlvorKit.Windowing;

/// <summary>Exposes screen, visibility, title, and close operations for a window loop.</summary>
public class WindowScreen(WindowLoop window)
{
    /// <summary>Gets whether the window is exiting.</summary>
    public bool IsExiting => window.Close.IsExiting;

    /// <summary>Gets or sets whether the window is fullscreen.</summary>
    public bool IsFullscreen
    {
        get => window.Toggle.IsFullscreen;
        set
        {
            if (window.Toggle.IsFullscreen != value)
                window.Toggle.ToggleFullscreen();
        }
    }

    /// <summary>Gets or sets whether vertical synchronization is enabled.</summary>
    public bool IsVSyncEnabled
    {
        get => window.Toggle.IsVSyncEnabled;
        set
        {
            if (window.Toggle.IsVSyncEnabled != value)
                window.Toggle.ToggleVSync();
        }
    }

    /// <summary>Gets or sets whether the window is visible.</summary>
    public bool IsVisible
    {
        get => window.Decoration.IsVisible;
        set => window.Decoration.IsVisible = value;
    }

    /// <summary>Gets or sets the window title.</summary>
    public string Title
    {
        get => window.Decoration.Title;
        set => window.Decoration.Title = value;
    }

    /// <summary>Sets the drawable client size.</summary>
    public Vec2u Size
    {
        set => window.Physical.Size = value;
    }

    /// <summary>Gets the primary monitor work-area size.</summary>
    public Vec2u MonitorSize => window.Physical.MonitorSize;

    /// <summary>Gets the primary monitor horizontal content scale.</summary>
    public float MonitorScale => window.Physical.MonitorScale;

    /// <summary>Sets the window icon from RGBA pixels.</summary>
    public void SetIcon(Vec2u size, ReadOnlySpan<Vec4u8> pixels) => window.Decoration.SetIcon(size, pixels);

    /// <summary>Toggles fullscreen mode.</summary>
    public void ToggleFullscreen() => window.Toggle.ToggleFullscreen();

    /// <summary>Toggles vertical synchronization.</summary>
    public void ToggleVSync() => window.Toggle.ToggleVSync();

    /// <summary>Requests that the window close.</summary>
    public void Close() => window.Close.Close();
}
