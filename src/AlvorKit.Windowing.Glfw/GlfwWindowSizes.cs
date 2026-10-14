namespace AlvorKit;

/// <summary>Reads and writes GLFW window and monitor sizes using AlvorKit vector types.</summary>
[ExcludeFromCodeCoverage]
internal class GlfwWindowSizes(Glfw glfw, GlfwWindow window)
{
    /// <summary>Converts a position from GLFW window coordinates to drawable framebuffer coordinates.</summary>
    internal Vec2 WindowToFramebuffer(Vec2 value) => value * FramebufferScale;

    /// <summary>Converts a position from drawable framebuffer coordinates to GLFW window coordinates.</summary>
    internal Vec2 FramebufferToWindow(Vec2 value) => value / FramebufferScale;

    /// <summary>Gets the drawable framebuffer size of the window.</summary>
    internal Vec2u FramebufferSize
    {
        get
        {
            glfw.GetFramebufferSize(window, out var width, out var height);
            return new(((uint)width), ((uint)height));
        }
    }

    /// <summary>Converts the requested drawable size to GLFW window coordinates, including Retina framebuffers.</summary>
    internal void Set(Vec2u size)
    {
        var logical = (Vec2)size / FramebufferScale;
        glfw.SetWindowSize(window, (int)MathF.Round(logical.X), (int)MathF.Round(logical.Y));
    }

    /// <summary>Gets the current monitor work area in drawable pixels.</summary>
    internal Vec2u MonitorWorkareaSize
    {
        get
        {
            var monitor = GlfwWindowMonitor.Find(glfw, window);
            glfw.GetMonitorWorkarea(monitor, out _, out _, out var width, out var height);
            return (Vec2u)(new Vec2(width, height) * FramebufferScale);
        }
    }

    /// <summary>Gets this window's current DPI scale, including changes after moving between monitors.</summary>
    internal float MonitorContentScale
    {
        get
        {
            glfw.GetWindowContentScale(window, out var xscale, out _);
            return xscale;
        }
    }

    private Vec2 FramebufferScale
    {
        get
        {
            glfw.GetWindowSize(window, out var windowWidth, out var windowHeight);
            glfw.GetFramebufferSize(window, out var framebufferWidth, out var framebufferHeight);
            return new(
                framebufferWidth / (float)windowWidth,
                framebufferHeight / (float)windowHeight);
        }
    }
}
