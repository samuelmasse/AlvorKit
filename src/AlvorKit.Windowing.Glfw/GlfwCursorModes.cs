namespace AlvorKit.Windowing;

/// <summary>Translates AlvorKit cursor modes to and from GLFW cursor modes.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwCursorModes
{
    /// <summary>Returns the GLFW cursor mode for a caller-provided AlvorKit cursor mode.</summary>
    internal GlfwCursorMode ToGlfw(CursorMode mode) =>
        mode switch
        {
            CursorMode.Normal => GlfwCursorMode.Normal,
            CursorMode.Hidden => GlfwCursorMode.Hidden,
            CursorMode.Disabled => GlfwCursorMode.Disabled,
            CursorMode.Captured => GlfwCursorMode.Captured,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Cursor mode must be a defined value.")
        };

    /// <summary>Returns the nearest AlvorKit cursor mode for a value read from GLFW.</summary>
    internal CursorMode FromGlfw(GlfwCursorMode mode) =>
        mode switch
        {
            GlfwCursorMode.Hidden => CursorMode.Hidden,
            GlfwCursorMode.Disabled => CursorMode.Disabled,
            GlfwCursorMode.Captured => CursorMode.Captured,
            _ => CursorMode.Normal
        };
}
