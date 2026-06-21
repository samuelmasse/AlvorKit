namespace AlvorKit.Windowing;

/// <summary>Registers GLFW callbacks and translates their payloads into AlvorKit window events.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwWindowCallbacks(
    Glfw glfw,
    GlfwWindow window,
    Action closing,
    Action<WindowPositionEvent> move,
    Action<WindowResizeEvent> resize,
    Action<WindowMouseMoveEvent> mouseMove,
    Action<WindowMouseWheelEvent> mouseWheel,
    Action<WindowMouseButtonEvent> mouseDown,
    Action<WindowMouseButtonEvent> mouseUp,
    Action<WindowKeyEvent> keyDown,
    Action<WindowKeyEvent> keyUp,
    Action<WindowTextInputEvent> textInput)
{
    /// <summary>Installs the callbacks used by <see cref="GlfwWindowHost"/>.</summary>
    internal void Register()
    {
        glfw.SetWindowCloseCallback(window, (_) => closing());
        glfw.SetWindowPosCallback(window, (_, x, y) => move(new((x, y))));
        glfw.SetFramebufferSizeCallback(window, (_, width, height) => resize(new((checked((uint)width), checked((uint)height)))));
        glfw.SetCursorPosCallback(window, (_, x, y) => mouseMove(new(((float)x, (float)y))));
        glfw.SetScrollCallback(window, (_, x, y) => mouseWheel(new(((float)x, (float)y))));
        glfw.SetMouseButtonCallback(window, (_, button, action, _) => AcceptMouseButton(button, action));
        glfw.SetKeyCallback(window, (_, key, _, action, _) => AcceptKey(key, action));
        glfw.SetCharCallback(window, (_, codepoint) => textInput(new(new((int)codepoint))));
    }

    /// <summary>Raises an AlvorKit mouse-button event for GLFW press and release actions.</summary>
    private void AcceptMouseButton(GlfwMouseButton button, GlfwInputAction action)
    {
        var mapped = (MouseButton)(int)button;
        if (action == GlfwInputAction.Press)
            mouseDown(new(mapped));
        else if (action == GlfwInputAction.Release)
            mouseUp(new(mapped));
    }

    /// <summary>Raises an AlvorKit key event for GLFW press, repeat, and release actions.</summary>
    private void AcceptKey(GlfwKey key, GlfwInputAction action)
    {
        var mapped = (Keys)(int)key;
        if (action == GlfwInputAction.Press || action == GlfwInputAction.Repeat)
            keyDown(new(mapped, action == GlfwInputAction.Repeat));
        else if (action == GlfwInputAction.Release)
            keyUp(new(mapped, false));
    }
}
