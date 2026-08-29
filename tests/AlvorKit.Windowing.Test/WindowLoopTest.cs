namespace AlvorKit;

[TestClass]
public class WindowLoopTest
{
    [TestMethod]
    public void WindowLoop_EventsWithNoListeners_ShouldNotCrash()
    {
        var (host, _) = WindowingTestFactory.Create();

        host.RaiseUpdate();
        host.IsFocused = true;
        host.RaiseUpdate();
        host.RaiseRender();
        host.RaiseResize(new(1, 1));
        host.RaiseMove(new(3, 4));
        host.RaiseClosing();
    }

    [TestMethod]
    public void WindowLoop_EventsWhenExiting_ShouldNotFire()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var updateInvoked = 0;
        var frameInvoked = 0;
        var renderInvoked = 0;
        loop.Update += (_) => updateInvoked++;
        loop.Frame += (_) => frameInvoked++;
        loop.Render += () => renderInvoked++;

        host.IsExiting = true;
        host.RaiseUpdate();
        host.RaiseRender();
        host.RaiseResize(new(1, 1));

        Assert.AreEqual(0, updateInvoked);
        Assert.AreEqual(0, frameInvoked);
        Assert.AreEqual(0, renderInvoked);
    }

    [TestMethod]
    public void WindowLoop_CloseEvent_IsIdempotent()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var unloadInvoked = 0;
        loop.Unload += () => unloadInvoked++;

        host.IsExiting = true;
        host.RaiseClosing();
        host.RaiseClosing();
        host.RaiseClosing();

        Assert.AreEqual(1, unloadInvoked);
    }

    [TestMethod]
    public void WindowLoop_Events_AreFiredCorrectly()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var updateInvoked = 0;
        double updateDelta = 0;
        var frameInvoked = 0;
        double frameDelta = 0;
        var renderInvoked = 0;
        loop.Update += (e) => { updateDelta = e; updateInvoked++; };
        loop.Frame += (e) => { frameDelta = e; frameInvoked++; };
        loop.Render += () => renderInvoked++;

        host.RaiseUpdate(45);
        host.WindowState = WindowState.Minimized;
        host.RaiseRender(86);
        host.IsFocused = true;
        host.RaiseUpdate(15);
        host.WindowState = WindowState.Normal;
        host.RaiseRender(46);

        Assert.AreEqual(15, updateDelta);
        Assert.AreEqual(2, updateInvoked);
        Assert.AreEqual(2, frameInvoked);
        Assert.AreEqual(46, frameDelta);
        Assert.AreEqual(1, renderInvoked);
    }

    /// <summary>Buffer-swap boundary events bracket both ordinary and resize-driven host swaps.</summary>
    [TestMethod]
    public void WindowLoop_BufferSwapEvents_BracketEveryHostSwap()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var beforeSwapCounts = new List<int>();
        var afterSwapCounts = new List<int>();
        loop.BeforeBufferSwap += () => beforeSwapCounts.Add(host.SwapBuffersCount);
        loop.AfterBufferSwap += () => afterSwapCounts.Add(host.SwapBuffersCount);

        host.RaiseRender();
        host.RaiseResize((1, 1));

        CollectionAssert.AreEqual(new[] { 0, 1 }, beforeSwapCounts);
        CollectionAssert.AreEqual(new[] { 1, 2 }, afterSwapCounts);
        Assert.AreEqual(2, host.SwapBuffersCount);
    }

    /// <summary>Minimized render frames do not publish buffer-swap boundary events because no host swap occurs.</summary>
    [TestMethod]
    public void WindowLoop_BufferSwapEvents_DoNotFireWhenMinimized()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var beforeInvoked = 0;
        var afterInvoked = 0;
        loop.BeforeBufferSwap += () => beforeInvoked++;
        loop.AfterBufferSwap += () => afterInvoked++;
        host.WindowState = WindowState.Minimized;

        host.RaiseRender();

        Assert.AreEqual(0, beforeInvoked);
        Assert.AreEqual(0, afterInvoked);
        Assert.AreEqual(0, host.SwapBuffersCount);
    }

    /// <summary>Unfocused windows keep updating and rendering while VSync is forced only for the duration of lost focus.</summary>
    [TestMethod]
    public void WindowLoop_Unfocused_KeepsRunningWithTemporaryVSync()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);
        var updateInvoked = 0;
        var renderInvoked = 0;
        loop.Update += (_) => updateInvoked++;
        loop.Render += () => renderInvoked++;

        host.RaiseUpdate();
        host.RaiseRender();

        Assert.AreEqual(1, updateInvoked);
        Assert.AreEqual(1, renderInvoked);
        Assert.IsFalse(screen.IsVSyncEnabled);
        Assert.IsTrue(host.IsVSyncEnabled);

        host.IsFocused = true;
        host.RaiseUpdate();

        Assert.AreEqual(2, updateInvoked);
        Assert.IsFalse(screen.IsVSyncEnabled);
        Assert.IsFalse(host.IsVSyncEnabled);
    }

    [TestMethod]
    public void WindowLoop_ResizeAndMove_SkipsUpdates()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var updateInvoked = 0;
        double updateDelta = 0;
        loop.Update += (e) => { updateDelta = e; updateInvoked++; };

        host.RaiseResize(new(10, 10));
        host.RaiseUpdate(1);
        host.RaiseUpdate(1);
        host.RaiseMove(new(5, 5));
        host.RaiseUpdate(1);
        host.RaiseUpdate(1);

        Assert.AreEqual(0, updateDelta);
        Assert.AreEqual(1, updateInvoked);
    }

    [TestMethod]
    public void WindowLoop_ResizeDuringUpdate_DoesNotReenter()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var updateInvoked = 0;
        loop.Update += (_) =>
        {
            updateInvoked++;
            if (updateInvoked == 1)
                host.RaiseResize(new(10, 10));
        };

        host.IsFocused = true;
        host.RaiseUpdate(1);

        Assert.AreEqual(1, updateInvoked);
    }

    [TestMethod]
    public void WindowLoop_ResizeDuringUpdate_DoesNotSkipFutureUpdates()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var updateInvoked = 0;
        var raiseResize = true;
        loop.Update += (_) =>
        {
            updateInvoked++;
            if (raiseResize)
            {
                raiseResize = false;
                host.RaiseResize(new(10, 10));
            }
        };

        host.IsFocused = true;
        host.RaiseUpdate(1);
        host.RaiseUpdate(1);

        Assert.AreEqual(2, updateInvoked);
    }

    [TestMethod]
    public void WindowLoop_ResizeDuringRender_DoesNotFireUpdate()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var updateInvoked = 0;
        loop.Update += (_) => updateInvoked++;
        loop.Render += () => host.RaiseResize(new(10, 10));

        host.IsFocused = true;
        host.RaiseRender(1);

        Assert.AreEqual(0, updateInvoked);
    }

    [TestMethod]
    public void WindowLoop_Keys_TriggerToggle()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);

        host.RaiseKeyDown(Keys.F11);
        host.RaiseKeyDown(Keys.F12);
        host.IsFocused = true;
        host.RaiseUpdate();
        host.RaiseRender();

        Assert.IsTrue(screen.IsFullscreen);
        Assert.IsTrue(screen.IsVSyncEnabled);
        Assert.AreEqual(WindowState.Fullscreen, host.WindowState);
        Assert.IsTrue(host.IsVSyncEnabled);
    }

    [TestMethod]
    public void WindowLoop_Run_ForwardsToHost()
    {
        var (host, loop) = WindowingTestFactory.Create();

        loop.Run();

        Assert.AreEqual(1, host.RunCount);
    }
}
