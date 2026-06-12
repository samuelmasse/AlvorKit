namespace AlvorKit.RGFW;

/// <summary>Implements <see cref="Rgfw"/> against the RGFW shared library.</summary>
public class RgfwBackend : Rgfw
{
    public override nint CreateWindow(string name, int x, int y, int w, int h, RgfwWindowFlags flags) => RgfwNative.CreateWindow(name, x, y, w, h, flags);

    public override void WindowClose(nint window) => RgfwNative.WindowClose(window);

    public override bool WindowShouldClose(nint window) => RgfwNative.WindowShouldClose(window);

    public override void WindowSetShouldClose(nint window, bool shouldClose) => RgfwNative.WindowSetShouldClose(window, shouldClose);

    public override bool WindowCheckEvent(nint window, out RgfwEvent ev) => RgfwNative.WindowCheckEvent(window, out ev);

    public override void PollEvents() => RgfwNative.PollEvents();

    public override void WaitForEvent(int waitMs) => RgfwNative.WaitForEvent(waitMs);

    public override bool WindowGetSize(nint window, out int w, out int h) => RgfwNative.WindowGetSize(window, out w, out h);

    public override bool WindowGetPosition(nint window, out int x, out int y) => RgfwNative.WindowGetPosition(window, out x, out y);

    public override void WindowSetName(nint window, string name) => RgfwNative.WindowSetName(window, name);

    public override void WindowSetExitKey(nint window, RgfwKey key) => RgfwNative.WindowSetExitKey(window, key);

    public override void WindowMakeCurrentContextOpenGL(nint window) => RgfwNative.WindowMakeCurrentContextOpenGL(window);

    public override void WindowSwapBuffersOpenGL(nint window) => RgfwNative.WindowSwapBuffersOpenGL(window);

    public override nint GetProcAddressOpenGL(string procName) => RgfwNative.GetProcAddressOpenGL(procName);
}
