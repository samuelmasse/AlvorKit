namespace AlvorKit.Windowing;

/// <summary>Best-effort dark title bar switch for GLFW windows on Windows.</summary>
[ExcludeFromCodeCoverage]
internal static partial class GlfwWindowsDarkMode
{
    /// <summary>The <c>DWMWA_USE_IMMERSIVE_DARK_MODE</c> attribute understood by Windows 10 2004 and later.</summary>
    private const uint UseImmersiveDarkMode = 20;

    /// <summary>Asks DWM to draw a dark title bar; any failure keeps the stock light title bar.</summary>
    public static void TryEnable(Glfw glfw, GlfwWindow window)
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            EnableDarkTitleBar(glfw, window);
        }
        catch (Exception)
        {
            // Best effort only: a non-Win32 GLFW platform, an old Windows build, or a glfw3
            // without native-access exports all fall back to the default title bar.
        }
    }

    /// <summary>Applies the dark title bar attribute to the window's HWND.</summary>
    [SupportedOSPlatform("windows")]
    private static void EnableDarkTitleBar(Glfw glfw, GlfwWindow window)
    {
        var hwnd = glfw.GetWin32Window(window);
        if (hwnd == 0)
            return;

        var dark = 1;
        _ = DwmSetWindowAttribute(hwnd, UseImmersiveDarkMode, ref dark, sizeof(int));
    }

    /// <summary>Sets a DWM window attribute.</summary>
    [LibraryImport("dwmapi.dll")]
    [SupportedOSPlatform("windows")]
    private static partial int DwmSetWindowAttribute(nint hwnd, uint attribute, ref int value, uint size);
}
