using AlvorKit.GLFW;

namespace AlvorKit.Demo;

public sealed class DemoWindow : IDisposable
{
    private readonly Glfw glfw;
    private GlfwWindow handle;

    private DemoWindow(Glfw glfw, GlfwWindow handle)
    {
        this.glfw = glfw;
        this.handle = handle;
    }

    public GlfwWindow Handle => handle;

    public static DemoWindow? TryCreate(Glfw glfw, string title, int width, int height)
    {
        var handle = glfw.CreateWindow(width, height, title, default, default);
        if (handle == default)
            return null;

        glfw.MakeContextCurrent(handle);
        return new DemoWindow(glfw, handle);
    }

    public void Dispose()
    {
        if (handle == default)
            return;

        glfw.DestroyWindow(handle);
        handle = default;
    }
}
