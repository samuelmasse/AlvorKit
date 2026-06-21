namespace AlvorKit.Windowing;

public partial class GlfwWindowHost
{
    /// <inheritdoc />
    public virtual void Close() => Glfw.SetWindowShouldClose(Window, true);

    /// <inheritdoc />
    public virtual void SwapBuffers() => Glfw.SwapBuffers(Window);

    /// <inheritdoc />
    public virtual void Run()
    {
        clock.Restart();
        var previous = clock.Elapsed.TotalSeconds;

        while (!Glfw.WindowShouldClose(Window))
        {
            Glfw.PollEvents();
            var now = clock.Elapsed.TotalSeconds;
            var elapsed = now - previous;
            previous = now;
            var frame = new WindowFrameEvent(elapsed, now);
            OnUpdateFrame(frame);
            OnRenderFrame(frame);
        }
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        if (!HasNativeWindow)
            return;

        Glfw.DestroyWindow(Window);
        Glfw.Terminate();
    }

    private void RegisterCallbacks()
    {
        Glfw.SetWindowCloseCallback(Window, (_) => OnClosing());
        Glfw.SetWindowPosCallback(Window, (_, x, y) => OnMove(new((x, y))));
        Glfw.SetFramebufferSizeCallback(Window, (_, width, height) => OnResize(new((checked((uint)width), checked((uint)height)))));
        Glfw.SetCursorPosCallback(Window, (_, x, y) => OnMouseMove(new(new((float)x, (float)y))));
        Glfw.SetScrollCallback(Window, (_, x, y) => OnMouseWheel(new(new((float)x, (float)y))));
        Glfw.SetMouseButtonCallback(Window, (_, button, action, _) => AcceptMouseButton(button, action));
        Glfw.SetKeyCallback(Window, (_, key, _, action, _) => AcceptKey(key, action));
        Glfw.SetCharCallback(Window, (_, codepoint) => OnTextInput(new(new((int)codepoint))));
    }

    private void AcceptMouseButton(GlfwMouseButton button, GlfwInputAction action)
    {
        var mapped = (WindowMouseButton)(int)button;

        if (action == GlfwInputAction.Press)
            OnMouseDown(new(mapped));
        else if (action == GlfwInputAction.Release)
            OnMouseUp(new(mapped));
    }

    private void AcceptKey(GlfwKey key, GlfwInputAction action)
    {
        var mapped = (WindowKey)(int)key;

        if (action == GlfwInputAction.Press || action == GlfwInputAction.Repeat)
            OnKeyDown(new(mapped, action == GlfwInputAction.Repeat));
        else if (action == GlfwInputAction.Release)
            OnKeyUp(new(mapped, false));
    }
}
