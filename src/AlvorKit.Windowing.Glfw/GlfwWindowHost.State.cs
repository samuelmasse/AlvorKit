namespace AlvorKit.Windowing;

public partial class GlfwWindowHost
{
    private void SetWindowState(WindowState value)
    {
        if (value == WindowState.Fullscreen)
        {
            EnterFullscreen();
            return;
        }

        if (fullscreen)
            ExitFullscreen();

        if (value == WindowState.Minimized)
            Glfw.IconifyWindow(Window);
        else if (value == WindowState.Maximized)
            Glfw.MaximizeWindow(Window);
        else Glfw.RestoreWindow(Window);
    }

    private void EnterFullscreen()
    {
        if (fullscreen)
            return;

        Glfw.GetWindowPos(Window, out windowedX, out windowedY);
        Glfw.GetWindowSize(Window, out windowedWidth, out windowedHeight);
        var monitor = Glfw.GetPrimaryMonitor();
        Glfw.GetMonitorWorkarea(monitor, out _, out _, out var width, out var height);
        Glfw.SetWindowMonitor(Window, monitor, 0, 0, width, height, (int)GlfwEnum.DontCare);
        fullscreen = true;
    }

    private void ExitFullscreen()
    {
        Glfw.SetWindowMonitor(Window, default, windowedX, windowedY, windowedWidth, windowedHeight, (int)GlfwEnum.DontCare);
        fullscreen = false;
    }

    private static GlfwCursorMode ToGlfwCursorMode(WindowCursorMode mode) =>
        mode switch
        {
            WindowCursorMode.Hidden => GlfwCursorMode.Hidden,
            WindowCursorMode.Disabled => GlfwCursorMode.Disabled,
            WindowCursorMode.Captured => GlfwCursorMode.Captured,
            _ => GlfwCursorMode.Normal
        };

    private static WindowCursorMode FromGlfwCursorMode(GlfwCursorMode mode) =>
        mode switch
        {
            GlfwCursorMode.Hidden => WindowCursorMode.Hidden,
            GlfwCursorMode.Disabled => WindowCursorMode.Disabled,
            GlfwCursorMode.Captured => WindowCursorMode.Captured,
            _ => WindowCursorMode.Normal
        };
}
