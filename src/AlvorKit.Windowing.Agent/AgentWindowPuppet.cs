namespace AlvorKit.Windowing;

/// <summary>Executes a finite AlvorSense command batch under one exclusive native-input reservation.</summary>
/// <param name="host">Agent-capable host receiving synthetic window commands.</param>
/// <param name="loop">Window loop whose input state and framebuffer boundary are coordinated.</param>
/// <param name="gl">Graphics layer used to read captured framebuffers.</param>
internal sealed class AgentWindowPuppet(
    AgentGlfwWindowHost host,
    WindowLoop loop,
    GlLayer gl)
{
    private readonly AgentWindowScreenshot screenshot = new(gl);

    /// <summary>Executes commands on the window thread and returns all textual and framebuffer output.</summary>
    internal AgentWindowPuppetResult Run(IReadOnlyList<string> commands)
    {
        ArgumentNullException.ThrowIfNull(commands);

        using var reservation = host.ReserveInput();
        ResetInput();
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        var artifacts = new List<AgentWindowPuppetArtifact>();
        var runner = new AgentWindowCommandRunner(
            host,
            output,
            name => Capture(name, artifacts));
        var executed = 0;

        try
        {
            foreach (var command in commands)
            {
                if (!runner.Execute(command))
                    break;

                executed++;
            }

            return new(
                executed,
                output.ToString(),
                host.Time,
                host.UpdateCount,
                host.RenderCount,
                host.Agent.MousePosition,
                [.. artifacts]);
        }
        finally
        {
            ReleaseSyntheticInput();
            ResetInput();
        }
    }

    /// <summary>Renders and appends one client-named PNG capture.</summary>
    private void Capture(string name, List<AgentWindowPuppetArtifact> artifacts)
    {
        byte[]? png = null;
        void CaptureFramebuffer() => png = screenshot.Capture(host.ClientSize);

        loop.FramebufferReady += CaptureFramebuffer;
        try
        {
            host.Agent.Render();
        }
        finally
        {
            loop.FramebufferReady -= CaptureFramebuffer;
        }

        if (png is null)
            throw new InvalidOperationException("The game did not expose a drawable framebuffer for the screenshot command.");

        artifacts.Add(new(name, png));
    }

    /// <summary>Raises releases for every synthetic key and mouse button still held by the agent driver.</summary>
    private void ReleaseSyntheticInput()
    {
        foreach (var key in host.Agent.Input.HeldKeys.ToArray())
            host.Agent.ReleaseKey(key);
        foreach (var button in host.Agent.Input.HeldMouseButtons.ToArray())
            host.Agent.ReleaseMouse(button);
    }

    /// <summary>Clears driver and consumer input state at a transaction boundary.</summary>
    private void ResetInput()
    {
        host.Agent.Input.Reset();
        loop.ResetInput();
    }
}
