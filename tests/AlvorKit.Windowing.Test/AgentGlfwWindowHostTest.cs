namespace AlvorKit.Windowing.Test;

[TestClass]
public class AgentGlfwWindowHostTest
{
    /// <summary>Verifies that agent mode is implemented by the GLFW-derived host itself.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_WhenAgentPresent_DerivesFromGlfwHost()
    {
        using var host = CreateAgent(new(321, 234), "Game host", true, false);

        Assert.IsInstanceOfType<GlfwWindowHost>(host);
        Assert.IsTrue(host.IsAgentMode);
        Assert.AreEqual(new Vector2(321, 234), host.ClientSize);
        Assert.AreEqual("Game host", host.Title);
        Assert.IsTrue(host.IsVisible);
        Assert.IsFalse(host.IsVSyncEnabled);
    }

    /// <summary>Verifies that agent mode owns deterministic state directly on the GLFW-derived host.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_WhenAgentPresent_StoresAgentState()
    {
        using var host = CreateAgent(new(64, 48));

        host.IsVisible = true;
        host.ClientSize = new(-4, 0);
        host.MousePosition = new(7, 8);
        host.WindowState = WindowState.Fullscreen;
        host.CursorMode = WindowCursorMode.Hidden;
        host.IsVSyncEnabled = false;
        host.Title = "Outer";
        host.Clipboard = "Board";
        host.SwapBuffers();

        Assert.IsTrue(host.IsVisible);
        Assert.IsTrue(host.IsFocused);
        Assert.IsTrue(host.IsFullscreen);
        Assert.AreEqual(new Vector2(1, 1), host.ClientSize);
        Assert.AreEqual(new Vector2(1920, 1080), host.MonitorSize);
        Assert.AreEqual(1, host.MonitorScale);
        Assert.AreEqual(new Vector2(7, 8), host.MousePosition);
        Assert.AreEqual(WindowCursorMode.Hidden, host.CursorMode);
        Assert.IsFalse(host.IsVSyncEnabled);
        Assert.AreEqual("Outer", host.Title);
        Assert.AreEqual("Board", host.Clipboard);
        Assert.AreEqual(1, host.SwapBuffersCount);
        Assert.AreEqual(4, host.GetProcAddress("Draw"));
    }

    /// <summary>Verifies that selected agent mode runs the interactive command loop.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_WhenAgentPresent_RunsAgentCommands()
    {
        using var input = new StringReader("""
            updates 2 0.5 3 4
            state
            quit
            """);
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var host = CreateAgent(new(64, 48), agentInput: input, agentOutput: output);

        host.Run();

        Assert.AreEqual(1, host.RunCount);
        Assert.AreEqual(2, host.UpdateCount);
        Assert.AreEqual(new Vector2(6, 8), host.MousePosition);
        var text = output.ToString();
        StringAssert.Contains(text, "Usage:");
        StringAssert.Contains(text, "updates=2");
    }

    /// <summary>Verifies that agent screenshots render once and call the configured save path.</summary>
    [TestMethod]
    public void AgentGlfwWindowHost_WhenAgentPresent_RoutesScreenshotCommands()
    {
        using var input = new StringReader("""
            screenshot out\frame.png
            quit
            """);
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        var gl = CreateGl();
        var saved = 0;
        GlLayer? savedGl = null;
        var savedSize = Vector2.Zero;
        var savedPath = string.Empty;
        using var host = CreateAgent(
            new(64, 48),
            gl: gl,
            agentInput: input,
            agentOutput: output,
            screenshotSave: (layer, size, path) =>
            {
                saved++;
                savedGl = layer;
                savedSize = size;
                savedPath = path;
            });

        host.Run();

        Assert.AreEqual(1, host.RenderCount);
        Assert.AreEqual(1, saved);
        Assert.AreSame(gl, savedGl);
        Assert.AreEqual(new Vector2(64, 48), savedSize);
        Assert.AreEqual("out\\frame.png", savedPath);
    }

    private static AgentGlfwWindowHost CreateAgent(
        Vector2 clientSize,
        string title = "AlvorKit.Windowing",
        bool isVisible = false,
        bool isVSyncEnabled = true,
        GlLayer? gl = null,
        TextReader? agentInput = null,
        TextWriter? agentOutput = null,
        Action<GlLayer, Vector2, string>? screenshotSave = null) =>
        new(gl ?? CreateGl(), clientSize, title, isVisible, isVSyncEnabled, agentInput, agentOutput, screenshotSave);

    private static GlLayer CreateGl() => new(new GlNoop());
}
