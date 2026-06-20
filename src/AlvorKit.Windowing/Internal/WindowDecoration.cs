namespace AlvorKit.Windowing;

/// <summary>Wraps title and visibility state for a window loop.</summary>
internal sealed class WindowDecoration(IWindowHost window)
{
    /// <summary>Gets or sets whether the window is visible.</summary>
    internal bool IsVisible
    {
        get => window.IsVisible;
        set => window.IsVisible = value;
    }

    /// <summary>Gets or sets the window title.</summary>
    internal string Title
    {
        get => window.Title;
        set => window.Title = value;
    }
}
