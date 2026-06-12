using AlvorKit.RGFW;

namespace AlvorKit.Demo;

public sealed class DemoWindow : IDisposable
{
    private readonly Rgfw rgfw;
    private nint handle;

    private DemoWindow(Rgfw rgfw, nint handle)
    {
        this.rgfw = rgfw;
        this.handle = handle;
    }

    public nint Handle => handle;

    public static DemoWindow? TryCreate(Rgfw rgfw, string title, int width, int height)
    {
        var handle = rgfw.CreateWindow(
            title,
            0,
            0,
            width,
            height,
            RgfwWindowFlags.WindowCenter | RgfwWindowFlags.WindowOpenGL);

        if (handle == 0)
            return null;

        rgfw.WindowSetExitKey(handle, RgfwKey.Escape);
        rgfw.WindowMakeCurrentContextOpenGL(handle);
        return new DemoWindow(rgfw, handle);
    }

    public void GetSize(out int width, out int height)
    {
        rgfw.WindowGetSize(handle, out width, out height);
    }

    public void Dispose()
    {
        if (handle == 0)
            return;

        rgfw.WindowClose(handle);
        handle = 0;
    }
}
