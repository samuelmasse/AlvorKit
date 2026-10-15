namespace AlvorKit;

/// <summary>Reads the desktop's nominal cursor size in GLFW custom-image units, independently of application UI scale.</summary>
/// <param name="glfw">The initialized GLFW instance that owns the target window.</param>
public class GlfwCursorSize(Glfw glfw)
{
    /// <summary>
    /// Gets Windows DPI cursor metrics, the X11 theme size, or the Wayland theme's logical size.
    /// Cocoa uses a 32-point image canvas; the compositor applies Retina and accessibility enlargement.
    /// The null platform uses a 32-pixel canvas. Query during configuration refresh, not per draw.
    /// </summary>
    public Vec2u Get(GlfwWindow window) => glfw.GetPlatform() switch
    {
        GlfwPlatform.PlatformWin32 when OperatingSystem.IsWindows() =>
            GlfwCursorSizeNative.Windows(glfw.GetWin32Window(window)),
        GlfwPlatform.PlatformX11 when OperatingSystem.IsLinux() =>
            new((uint)GlfwCursorSizeNative.X11(glfw.GetX11Display().Handle)),
        GlfwPlatform.PlatformWayland => new(WaylandSize(Environment.GetEnvironmentVariable("XCURSOR_SIZE"))),
        GlfwPlatform.PlatformCocoa or GlfwPlatform.PlatformNull => new(32),
        _ => throw new InvalidOperationException("Cursor sizing requires an initialized GLFW platform.")
    };

    /// <summary>Matches GLFW 3.4's Wayland theme size: a positive XCURSOR_SIZE, otherwise its 16-unit default.</summary>
    internal static uint WaylandSize(string? configured) =>
        int.TryParse(configured, out var size) && size > 0 ? (uint)size : 16;
}
