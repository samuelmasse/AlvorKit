namespace AlvorKit.Engine;

/// <summary>Root-scoped window screen, visibility, title, and close facade.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootScreen(WindowLoop window)
{
    private readonly WindowScreen screen = new(window);

    /// <summary>Gets whether the window is exiting.</summary>
    public bool IsExiting => screen.IsExiting;

    /// <summary>Gets or sets whether the window is fullscreen.</summary>
    public bool IsFullscreen { get => screen.IsFullscreen; set => screen.IsFullscreen = value; }

    /// <summary>Gets or sets whether vertical synchronization is enabled.</summary>
    public bool IsVSyncEnabled { get => screen.IsVSyncEnabled; set => screen.IsVSyncEnabled = value; }

    /// <summary>Gets or sets whether the window is visible.</summary>
    public bool IsVisible { get => screen.IsVisible; set => screen.IsVisible = value; }

    /// <summary>Gets or sets the window title.</summary>
    public string Title { get => screen.Title; set => screen.Title = value; }

    /// <summary>Sets the drawable client size.</summary>
    public Vec2u Size { set => screen.Size = value; }

    /// <summary>Gets the primary monitor work-area size.</summary>
    public Vec2u MonitorSize => screen.MonitorSize;

    /// <summary>Gets the primary monitor horizontal content scale.</summary>
    public float MonitorScale => screen.MonitorScale;

    /// <summary>Toggles fullscreen mode.</summary>
    public void ToggleFullscreen() => screen.ToggleFullscreen();

    /// <summary>Toggles vertical synchronization.</summary>
    public void ToggleVSync() => screen.ToggleVSync();

    /// <summary>Requests that the window close.</summary>
    public void Close() => screen.Close();
}
