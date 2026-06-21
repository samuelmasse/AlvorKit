namespace AlvorKit.Windowing.Test;

[TestClass]
public class ScreenTest
{
    [TestMethod]
    public void Screen_IsExiting_HasCorrectValue()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);

        Assert.IsFalse(screen.IsExiting);

        host.IsExiting = true;

        Assert.IsTrue(screen.IsExiting);
    }

    [TestMethod]
    public void Screen_HasDecorationProps()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop)
        {
            IsVisible = true,
            Title = "Windowing test",
            Size = new(640, 480)
        };
        screen.Close();

        Assert.IsTrue(screen.IsVisible);
        Assert.AreEqual("Windowing test", screen.Title);
        Assert.IsTrue(host.IsVisible);
        Assert.AreEqual("Windowing test", host.Title);
        Assert.AreEqual(new Vec2u(640u, 480u), host.ClientSize);
        Assert.AreEqual(1, host.CloseCount);
    }

    [TestMethod]
    public void Screen_MonitorProps_AreForwarded()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);
        host.MonitorSize = new(2560, 1440);
        host.MonitorScale = 1.5f;

        Assert.AreEqual(new Vec2u(2560u, 1440u), screen.MonitorSize);
        Assert.AreEqual(1.5f, screen.MonitorScale);
    }

    [TestMethod]
    public void Screen_Fullscreen_CanToggle()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop)
        {
            IsFullscreen = false
        };
        screen.ToggleFullscreen();

        Assert.IsTrue(screen.IsFullscreen);
        Assert.AreEqual(WindowState.Fullscreen, host.WindowState);

        screen.IsFullscreen = true;
        screen.ToggleFullscreen();

        Assert.IsFalse(screen.IsFullscreen);
        Assert.AreEqual(WindowState.Normal, host.WindowState);
    }

    [TestMethod]
    public void Screen_Fullscreen_SetterIgnoresMatchingState()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);
        host.WindowState = WindowState.Fullscreen;

        screen.IsFullscreen = true;

        Assert.AreEqual(WindowState.Fullscreen, host.WindowState);
    }

    [TestMethod]
    public void Screen_Fullscreen_FromMaximized_RestoresPreviousState()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);
        host.WindowState = WindowState.Maximized;

        screen.ToggleFullscreen();
        screen.ToggleFullscreen();

        Assert.AreEqual(WindowState.Maximized, host.WindowState);
    }

    [TestMethod]
    public void Screen_VSync_CanToggle()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop)
        {
            IsVSyncEnabled = false
        };
        screen.ToggleVSync();

        Assert.IsTrue(screen.IsVSyncEnabled);
        Assert.IsTrue(host.IsVSyncEnabled);

        screen.IsVSyncEnabled = true;
        screen.ToggleVSync();

        Assert.IsFalse(screen.IsVSyncEnabled);
        Assert.IsFalse(host.IsVSyncEnabled);
    }

    [TestMethod]
    public void Screen_VSync_SetterIgnoresMatchingState()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);
        host.IsVSyncEnabled = true;

        screen.IsVSyncEnabled = true;

        Assert.IsTrue(host.IsVSyncEnabled);
    }
}
