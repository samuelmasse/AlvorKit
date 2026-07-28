namespace AlvorKit.Windowing;

/// <summary>Runs native GLFW polling and translates elapsed time into update and render frames.</summary>
[ExcludeFromCodeCoverage]
internal sealed class GlfwWindowPump
{
    /// <summary>Clock used to calculate monotonic frame durations.</summary>
    private readonly Stopwatch clock = new();

    /// <summary>GLFW API used to poll native events and read close state.</summary>
    private readonly Glfw glfw;

    /// <summary>Window whose native queue and close state drive the loop.</summary>
    private readonly GlfwWindow window;

    /// <summary>Creates a polling loop for one GLFW window.</summary>
    internal GlfwWindowPump(Glfw glfw, GlfwWindow window)
    {
        this.glfw = glfw;
        this.window = window;
    }

    /// <summary>Runs native polling hooks and forwards timed update and render frames until close.</summary>
    internal void Run(
        Action beforePollEvents,
        Action afterPollEvents,
        Action<WindowFrameEvent> updateFrame,
        Action<WindowFrameEvent> renderFrame)
    {
        clock.Restart();
        var previous = clock.Elapsed.TotalSeconds;
        while (!glfw.WindowShouldClose(window))
        {
            beforePollEvents();
            try
            {
                glfw.PollEvents();
            }
            finally
            {
                afterPollEvents();
            }

            var now = clock.Elapsed.TotalSeconds;
            var elapsed = now - previous;
            previous = now;
            var frame = new WindowFrameEvent(elapsed, now);
            updateFrame(frame);
            renderFrame(frame);
        }
    }
}
