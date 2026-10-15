namespace AlvorKit;

/// <summary>Platform cursor metrics used only by the selected GLFW desktop backend.</summary>
internal static partial class GlfwCursorSizeNative
{
    /// <summary>Reads the nominal cursor canvas for the window's current DPI and the Windows pointer-size setting.</summary>
    internal static Vec2u Windows(nint window)
    {
        var dpi = GetDpiForWindow(window);
        var width = GetSystemMetricsForDpi(13, dpi);
        var height = GetSystemMetricsForDpi(14, dpi);

        if (dpi == 0 || width <= 0 || height <= 0)
            throw new InvalidOperationException("Windows could not read cursor dimensions for the GLFW window.");

        return ((uint)width, (uint)height);
    }

    /// <summary>Reads Xcursor's desktop theme setting using the same display and library as GLFW.</summary>
    internal static int X11(nint display)
    {
        var size = XcursorGetDefaultSize(display);

        if (size <= 0)
            throw new InvalidOperationException("Xcursor reported an invalid desktop cursor size.");

        return size;
    }

    /// <summary>Gets the DPI associated with a native Windows window.</summary>
    [LibraryImport("user32.dll")]
    private static partial uint GetDpiForWindow(nint window);

    /// <summary>Gets a Windows system metric at the requested DPI.</summary>
    [LibraryImport("user32.dll")]
    private static partial int GetSystemMetricsForDpi(int index, uint dpi);

    /// <summary>Gets the X11 desktop cursor-theme size.</summary>
    [LibraryImport("libXcursor.so.1")]
    private static partial int XcursorGetDefaultSize(nint display);
}
