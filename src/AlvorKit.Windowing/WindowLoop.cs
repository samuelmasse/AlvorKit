namespace AlvorKit.Windowing;

/// <summary>Coordinates host window events into AlvorKit update, frame, and render callbacks.</summary>
public sealed class WindowLoop
{
    private readonly IWindowHost host;
    private readonly WindowMouse mouse;
    private readonly WindowMousePosition mousePosition;
    private readonly WindowKeyboard keyboard;
    private readonly WindowToggle toggle;
    private readonly WindowPhysical physical;
    private readonly WindowClose close;
    private readonly WindowDecoration decoration;
    private readonly WindowText text;
    private readonly WindowControls controls;
    private readonly WindowGamepads gamepads;
    private bool updating;
    private bool rendering;
    private bool unloaded;

    /// <summary>Raised when the loop performs logical work for the frame.</summary>
    public event Action<double>? Update;

    /// <summary>Raised immediately before render work.</summary>
    public event Action<double>? Frame;

    /// <summary>Raised when the loop should draw the current frame.</summary>
    public event Action? Render;

    /// <summary>Raised once when the host requests close or the host loop ends.</summary>
    public event Action? Unload;

    /// <summary>Creates a loop around a host-specific window.</summary>
    public WindowLoop(IWindowHost host)
    {
        this.host = host;
        physical = new(host, DrawResizeFrame);
        mouse = new(host);
        mousePosition = new(host, physical);
        keyboard = new(host);
        toggle = new(host);
        decoration = new(host);
        text = new(host);
        controls = new(mouse, keyboard);
        gamepads = new(host);
        close = new(host, UnloadOnce);

        host.UpdateFrame += OnUpdateFrame;
        host.RenderFrame += OnRenderFrame;
    }

    /// <summary>Gets mouse button and wheel state.</summary>
    internal WindowMouse Mouse => mouse;

    /// <summary>Gets cursor position and delta state.</summary>
    internal WindowMousePosition MousePosition => mousePosition;

    /// <summary>Gets keyboard key state.</summary>
    internal WindowKeyboard Keyboard => keyboard;

    /// <summary>Gets fullscreen and vsync toggle state.</summary>
    internal WindowToggle Toggle => toggle;

    /// <summary>Gets drawable size state.</summary>
    internal WindowPhysical Physical => physical;

    /// <summary>Gets close state.</summary>
    internal WindowClose Close => close;

    /// <summary>Gets decoration state.</summary>
    internal WindowDecoration Decoration => decoration;

    /// <summary>Gets text input state.</summary>
    internal WindowText Text => text;

    /// <summary>Gets control binding state.</summary>
    internal WindowControls Controls => controls;

    /// <summary>Gets polled gamepad state.</summary>
    internal WindowGamepads Gamepads => gamepads;

    /// <summary>Runs the host event loop until the host exits.</summary>
    public void Run()
    {
        host.Run();
        UnloadOnce();
    }

    private void UnloadOnce()
    {
        if (unloaded)
            return;

        unloaded = true;
        Unload?.Invoke();
    }

    private bool DrawResizeFrame()
    {
        if (host.IsExiting || updating || rendering)
            return false;

        Update?.Invoke(0);
        Render?.Invoke();
        host.SwapBuffers();
        return true;
    }

    private void OnUpdateFrame(WindowFrameEvent e)
    {
        if (host.IsExiting)
            return;

        gamepads.Tick();
        mousePosition.Update();

        if (physical.Skips == 0 && host.IsFocused)
        {
            updating = true;
            Update?.Invoke(e.Time);
            updating = false;
        }

        if (physical.Skips > 0)
            physical.Skips--;

        if (keyboard.IsKeyPressed(Keys.F11))
            toggle.ToggleFullscreen();
        if (keyboard.IsKeyPressed(Keys.F12))
            toggle.ToggleVSync();

        mouse.Tick();
        keyboard.Tick();
        text.Tick();
        controls.Tick();
    }

    private void OnRenderFrame(WindowFrameEvent e)
    {
        if (host.IsExiting)
            return;

        rendering = true;
        Frame?.Invoke(e.Time);

        if (host.WindowState != WindowState.Minimized)
        {
            Render?.Invoke();
            host.SwapBuffers();
        }

        rendering = false;
    }
}
