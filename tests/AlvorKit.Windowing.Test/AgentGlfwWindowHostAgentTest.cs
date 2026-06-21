namespace AlvorKit.Windowing.Test;

[TestClass]
public class AgentGlfwWindowHostAgentTest
{
    /// <summary>Verifies that agent updates run with exactly supplied deltas and accumulated total time.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_Update_UsesExactDeltasAndTotalTime()
    {
        using var host = CreateAgent();
        using var loop = new WindowLoop(host);
        var deltas = new double[2];
        var times = new double[2];
        var count = 0;
        loop.Update += (delta) =>
        {
            deltas[count] = delta;
            times[count] = host.Time;
            count++;
        };

        host.Agent.Update(0.02);
        host.Agent.Update(0.03);

        Assert.AreEqual(2, count);
        Assert.AreEqual(0.02, deltas[0]);
        Assert.AreEqual(0.03, deltas[1]);
        Assert.AreEqual(0.02, times[0]);
        Assert.AreEqual(0.05, times[1], 0.0000001);
        Assert.AreEqual(2, host.UpdateCount);
        Assert.AreEqual(0.05, host.Time, 0.0000001);
    }

    /// <summary>Verifies that renders are explicitly requested and buffer swaps are counted.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_Render_OnlyRunsWhenRequested()
    {
        using var host = CreateAgent();
        using var loop = new WindowLoop(host);
        var updates = 0;
        var renders = 0;
        loop.Update += (_) => updates++;
        loop.Render += () => renders++;

        host.Agent.Advance(3, 0.01);
        host.Agent.Render();

        Assert.AreEqual(3, updates);
        Assert.AreEqual(1, renders);
        Assert.AreEqual(1, host.RenderCount);
        Assert.AreEqual(1, host.SwapBuffersCount);
    }

    /// <summary>Verifies that per-update mouse panning produces deterministic position and delta state.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_UpdateWithMouseDelta_PansPerUpdate()
    {
        using var host = CreateAgent(new(100, 100), "mouse test", false, true);
        using var loop = new WindowLoop(host);
        var mouse = new Mouse(loop);

        host.Agent.Update(0);
        mouse.Track = true;
        host.Agent.Advance(2, 0.016, new(2, 3));

        Assert.AreEqual(new Vec2(4, 6), mouse.Position);
        Assert.AreEqual(new Vec2(2, 3), mouse.Delta);
    }

    /// <summary>Verifies that press transitions advance per update even when no render is requested.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_KeyPress_LastOneUpdateWithoutRendering()
    {
        using var host = CreateAgent();
        using var loop = new WindowLoop(host);
        var keyboard = new Keyboard(loop);
        var pressedDuringFirstUpdate = false;
        var pressedDuringSecondUpdate = true;
        loop.Update += (_) =>
        {
            pressedDuringFirstUpdate |= host.UpdateCount == 1 && keyboard.IsKeyPressed(WindowKey.Space);
            pressedDuringSecondUpdate &= host.UpdateCount != 2 || keyboard.IsKeyPressed(WindowKey.Space);
        };

        host.Agent.PressKey(WindowKey.Space);
        host.Agent.Update(0.016);
        host.Agent.Update(0.016);

        Assert.IsTrue(pressedDuringFirstUpdate);
        Assert.IsFalse(pressedDuringSecondUpdate);
        Assert.IsTrue(keyboard.IsKeyDown(WindowKey.Space));
    }

    /// <summary>Verifies that closing the agent host is idempotent and stops future frames.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_Close_StopsFutureFrames()
    {
        using var host = CreateAgent();
        using var loop = new WindowLoop(host);
        var unloads = 0;
        var updates = 0;
        loop.Unload += () => unloads++;
        loop.Update += (_) => updates++;

        host.Close();
        host.Close();
        host.Agent.Update(1);
        host.Agent.Render();

        Assert.AreEqual(1, unloads);
        Assert.AreEqual(0, updates);
        Assert.AreEqual(0, host.UpdateCount);
        Assert.AreEqual(0, host.RenderCount);
    }

    /// <summary>Verifies that agent host properties are observable and use fixed agent defaults.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_Properties_Work()
    {
        using var host = CreateAgent(new(3, 4), "agent", false, false);

        host.IsVisible = true;
        host.ClientSize = new(5, 7);
        host.MousePosition = new(5, 6);
        host.WindowState = WindowState.Fullscreen;
        host.CursorMode = WindowCursorMode.Disabled;
        host.IsVSyncEnabled = true;
        host.Title = "renamed";
        host.Dispose();
        host.Dispose();

        Assert.IsTrue(host.IsVisible);
        Assert.IsTrue(host.IsFocused);
        Assert.IsTrue(host.IsFullscreen);
        Assert.AreEqual(new Vec2u(5u, 7u), host.ClientSize);
        Assert.AreEqual(new Vec2u(1920u, 1080u), host.MonitorSize);
        Assert.AreEqual(1, host.MonitorScale);
        Assert.AreEqual(new Vec2(5, 6), host.MousePosition);
        Assert.AreEqual(WindowCursorMode.Disabled, host.CursorMode);
        Assert.IsTrue(host.IsVSyncEnabled);
        Assert.AreEqual("renamed", host.Title);
    }

    /// <summary>Verifies that all agent input helpers raise the expected host events.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_InputHelpers_RaiseEvents()
    {
        using var host = CreateAgent();
        var keyRepeats = 0;
        var keyUps = 0;
        var mouseDowns = 0;
        var mouseUps = 0;
        var textInputs = 0;
        var resized = Vec2u.Zero;
        var moved = Vec2i.Zero;
        var wheel = Vec2.Zero;
        host.KeyDown += (e) => keyRepeats += e.IsRepeat ? 1 : 0;
        host.KeyUp += (_) => keyUps++;
        host.MouseDown += (_) => mouseDowns++;
        host.MouseUp += (_) => mouseUps++;
        host.MouseWheel += (e) => wheel = e.Offset;
        host.TextInput += (_) => textInputs++;
        host.Resize += (e) => resized = e.Size;
        host.Move += (e) => moved = e.Position;

        host.Agent.RepeatKey(WindowKey.A);
        host.Agent.ReleaseKey(WindowKey.A);
        host.Agent.PressMouse(WindowMouseButton.Left);
        host.Agent.ReleaseMouse(WindowMouseButton.Left);
        host.Agent.ScrollMouse(new(1, -1));
        host.Agent.EnterText(new Rune('x'));
        host.Agent.EnterText("yz");
        host.Agent.SetFocus(false);
        host.Agent.ResizeWindow(new(7, 8));
        host.Agent.MoveWindow(new(9, 10));

        Assert.AreEqual(1, keyRepeats);
        Assert.AreEqual(1, keyUps);
        Assert.AreEqual(1, mouseDowns);
        Assert.AreEqual(1, mouseUps);
        Assert.AreEqual(new Vec2(1, -1), wheel);
        Assert.AreEqual(3, textInputs);
        Assert.IsFalse(host.IsFocused);
        Assert.AreEqual(new Vec2u(7u, 8u), resized);
        Assert.AreEqual(new Vec2i(9, 10), moved);
    }

    /// <summary>Verifies validation and native-free GL lookup fallback paths.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_InvalidControlInputs_Throw()
    {
        using var host = CreateAgent();

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.Agent.Update(-1));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.Agent.Render(double.NaN));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.Agent.Advance(-1, 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.ClientSize = new Vec2u(0u, 1u));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => host.Agent.ResizeWindow(new Vec2u(1u, 0u)));
        Assert.AreEqual(3, host.GetProcAddress("abc"));
    }

    /// <summary>Verifies the convenience step method runs exactly one update and render.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_Step_RunsOneUpdateAndRender()
    {
        using var host = CreateAgent();
        using var loop = new WindowLoop(host);
        var updates = 0;
        var renders = 0;
        loop.Update += (_) => updates++;
        loop.Render += () => renders++;

        host.Agent.Step(0.125);

        Assert.AreEqual(1, updates);
        Assert.AreEqual(1, renders);
        Assert.AreEqual(0.125, host.Time);
    }

    private static AgentGlfwWindowHost CreateAgent(
        Vec2u? clientSize = null,
        string title = "AlvorKit.Windowing",
        bool isVisible = false,
        bool isVSyncEnabled = true) =>
        new(new(new GlNoop()), clientSize ?? new(800, 600), title, isVisible, isVSyncEnabled);
}
