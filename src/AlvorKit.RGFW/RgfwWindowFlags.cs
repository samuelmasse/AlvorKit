namespace AlvorKit.RGFW;

/// <summary>Maps RGFW_windowFlags.</summary>
[Flags]
public enum RgfwWindowFlags : uint
{
    None = 0,
    NoBorder = 1 << 0,
    NoResize = 1 << 1,
    AllowDnd = 1 << 2,
    HideMouse = 1 << 3,
    Fullscreen = 1 << 4,
    Transparent = 1 << 5,
    Center = 1 << 6,
    ScaleToMonitor = 1 << 8,
    Hide = 1 << 9,
    Maximize = 1 << 10,
    CenterCursor = 1 << 11,
    Floating = 1 << 12,
    FocusOnShow = 1 << 13,
    Minimize = 1 << 14,
    Focus = 1 << 15,
    OpenGL = 1 << 17,
    Egl = 1 << 18,
    WindowedFullscreen = NoBorder | Maximize
}
