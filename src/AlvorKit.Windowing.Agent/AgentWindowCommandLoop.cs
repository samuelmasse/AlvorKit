namespace AlvorKit.Windowing;

/// <summary>Runs scripted agent-window commands against one deterministic host.</summary>
/// <param name="host">Host that receives parsed commands and screenshot renders.</param>
/// <param name="gl">OpenGL layer used by framebuffer screenshots.</param>
/// <param name="input">Optional command input stream; defaults to standard input.</param>
/// <param name="output">Optional command output stream; defaults to standard output.</param>
/// <param name="screenshotSave">Optional screenshot save callback for tests that avoid framebuffer reads.</param>
internal sealed class AgentWindowCommandLoop(
    AgentGlfwWindowHost host,
    GlLayer gl,
    TextReader? input = null,
    TextWriter? output = null,
    Action<GlLayer, Vec2u, string>? screenshotSave = null)
{
    /// <summary>Saves requested screenshots from the host framebuffer.</summary>
    private readonly AgentWindowScreenshot screenshot = new(gl, screenshotSave);

    /// <summary>Runs the interactive command stream until input ends or a quit command is received.</summary>
    internal void Run()
    {
        var writer = output ?? Console.Out;
        var runner = new AgentWindowCommandRunner(host, writer, CapturePng);
        runner.WriteHelp();
        runner.Run(input ?? Console.In);
    }

    /// <summary>Renders the current agent frame and writes the selected framebuffer to a PNG file.</summary>
    private void CapturePng(string path)
    {
        host.Agent.Render();
        screenshot.Save(host.ClientSize, path);
    }
}
