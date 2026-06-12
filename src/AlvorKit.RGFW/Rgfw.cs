namespace AlvorKit.RGFW;

/// <summary>
/// The RGFW API surface. Every method throws NotImplementedException until a
/// backend overrides it (e.g. RgfwBackend from AlvorKit.RGFW.Backend).
/// </summary>
public class Rgfw
{
    public const int EventNoWait = 0;
    public const int EventWaitNext = -1;

    public virtual nint CreateWindow(string name, int x, int y, int w, int h, RgfwWindowFlags flags) => throw new NotImplementedException();

    public virtual void WindowClose(nint window) => throw new NotImplementedException();

    public virtual bool WindowShouldClose(nint window) => throw new NotImplementedException();

    public virtual void WindowSetShouldClose(nint window, bool shouldClose) => throw new NotImplementedException();

    public virtual bool WindowCheckEvent(nint window, out RgfwEvent ev) => throw new NotImplementedException();

    public virtual void PollEvents() => throw new NotImplementedException();

    public virtual void WaitForEvent(int waitMs) => throw new NotImplementedException();

    public virtual bool WindowGetSize(nint window, out int w, out int h) => throw new NotImplementedException();

    public virtual bool WindowGetPosition(nint window, out int x, out int y) => throw new NotImplementedException();

    public virtual void WindowSetName(nint window, string name) => throw new NotImplementedException();

    public virtual void WindowSetExitKey(nint window, RgfwKey key) => throw new NotImplementedException();

    public virtual void WindowMakeCurrentContextOpenGL(nint window) => throw new NotImplementedException();

    public virtual void WindowSwapBuffersOpenGL(nint window) => throw new NotImplementedException();

    public virtual nint GetProcAddressOpenGL(string procName) => throw new NotImplementedException();
}
