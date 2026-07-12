namespace AlvorKit.Windowing;

/// <summary>Maps a GLFW window to the native parent-window representation accepted by NFDe.</summary>
[ExcludeFromCodeCoverage(Justification = "Dispatches runtime OS-specific GLFW native accessors.")]
internal static class GlfwFileDialogParent
{
    /// <summary>Gets the current platform's native window handle, or an unset parent when unavailable.</summary>
    internal static NfdWindowHandle Get(Glfw glfw, GlfwWindow window)
    {
        if (OperatingSystem.IsWindows())
            return Create(NfdWindowHandleType.Windows, glfw.GetWin32Window(window));
        if (OperatingSystem.IsMacOS())
            return Create(NfdWindowHandleType.Cocoa, glfw.GetCocoaWindow(window));
        if (OperatingSystem.IsLinux())
            return Create(NfdWindowHandleType.X11, unchecked((nint)glfw.GetX11Window(window)));
        return default;
    }

    /// <summary>Tags a nonzero native handle with its NFDe platform type.</summary>
    internal static NfdWindowHandle Create(NfdWindowHandleType type, nint handle) =>
        handle == 0 ? default : new() { Type = (nuint)type, Handle = handle };
}
