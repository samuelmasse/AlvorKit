namespace AlvorKit;

[TestClass]
public class GlfwWindowDisplayTest
{
    /// <summary>A drawable size request must round-trip on a Retina framebuffer rather than doubling on every launch.</summary>
    [TestMethod]
    public void DrawableSize_Retina_RoundTrips()
    {
        var glfw = new WindowingTestGlfw(new(960, 540)) { FramebufferSize = new(1920, 1080) };
        using var host = new GlfwWindowHost(glfw, glfw.Window);
        host.ClientSize = new(2560, 1600);

        Assert.AreEqual(new Vec2u(1280, 800), glfw.WindowSize);
        Assert.AreEqual(new Vec2u(2560, 1600), host.ClientSize);
        host.ClientSize = host.ClientSize;
        Assert.AreEqual(new Vec2u(2560, 1600), host.ClientSize);
    }

    /// <summary>DPI follows the window as it moves to another display without restarting the game.</summary>
    [TestMethod]
    public void MonitorScale_TracksWindowDpi()
    {
        var glfw = new WindowingTestGlfw(new(960, 540)) { ContentScale = new(1.25f, 1.25f) };
        using var host = new GlfwWindowHost(glfw, glfw.Window);
        Assert.AreEqual(1.25f, host.MonitorScale);
        glfw.ContentScale = new(2, 2);
        Assert.AreEqual(2f, host.MonitorScale);
    }

    /// <summary>Fullscreen uses the display containing the window at native size and refresh rate, then restores its bounds.</summary>
    [TestMethod]
    public void Fullscreen_UsesCurrentMonitorAndRestoresWindow()
    {
        var glfw = new WindowingTestGlfw(new(1280, 800)) { WindowPosition = new(2200, 100) };
        using var host = new GlfwWindowHost(glfw, glfw.Window);
        var loop = new WindowLoop(host);
        var screen = new WindowScreen(loop);
        var observedFullscreen = false;
        var observedHostFullscreen = false;
        host.Resize += _ =>
        {
            observedFullscreen = screen.IsFullscreen;
            observedHostFullscreen = host.IsFullscreen;
        };
        screen.ToggleFullscreen();

        Assert.IsTrue(host.IsFullscreen);
        Assert.AreEqual(new GlfwMonitor((nint)3), glfw.GetWindowMonitor(glfw.Window));
        Assert.AreEqual(new Vec2u(3840, 2160), host.ClientSize);
        Assert.AreEqual(144, glfw.LastRefreshRate);
        Assert.IsTrue(observedFullscreen);
        Assert.IsTrue(observedHostFullscreen);

        screen.ToggleFullscreen();

        Assert.IsFalse(host.IsFullscreen);
        Assert.AreEqual(new Vec2u(1280, 800), host.ClientSize);
        Assert.AreEqual(new Vec2i(2200, 100), glfw.WindowPosition);
        Assert.IsFalse(observedFullscreen);
        Assert.IsFalse(observedHostFullscreen);
    }

    /// <summary>Straddling windows use greatest overlap; windows outside all displays use the nearest monitor.</summary>
    [TestMethod]
    [DataRow(1700, 0, 3)]
    [DataRow(-1800, 0, 2)]
    [DataRow(6000, 0, 3)]
    public void Fullscreen_SelectsOverlapOrNearestMonitor(int x, int y, int monitor)
    {
        var glfw = new WindowingTestGlfw(new(800, 600)) { WindowPosition = new(x, y) };
        using var host = new GlfwWindowHost(glfw, glfw.Window);
        host.WindowState = WindowState.Fullscreen;
        Assert.AreEqual(new GlfwMonitor((nint)monitor), glfw.GetWindowMonitor(glfw.Window));
    }
}
