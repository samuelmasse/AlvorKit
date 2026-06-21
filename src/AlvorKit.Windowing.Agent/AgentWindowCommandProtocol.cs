namespace AlvorKit.Windowing;

/// <summary>Applies parsed agent commands to the agent-controlled window host.</summary>
internal class AgentWindowCommandProtocol(AgentGlfwWindowHost host, TextWriter output, Action<string>? screenshot)
{
    private string? executionError;

    internal bool ContinueReading { get; private set; } = true;

    internal void Reset()
    {
        ContinueReading = true;
        executionError = null;
    }

    internal void ThrowIfFailed()
    {
        if (executionError is not null)
            throw new InvalidOperationException(executionError);
    }

    internal void Update(double delta, Vec2 mouseDelta) => host.Agent.Update(delta, mouseDelta);

    internal void Advance(int count, double delta, Vec2 mouseDelta) => host.Agent.Advance(count, delta, mouseDelta);

    internal void Render(double delta) => host.Agent.Render(delta);

    internal void Step(double delta) => host.Agent.Step(delta);

    internal void Key(Keys key, AgentWindowKeyCommandAction action)
    {
        switch (action)
        {
            case AgentWindowKeyCommandAction.Down:
                host.Agent.PressKey(key);
                break;
            case AgentWindowKeyCommandAction.Up:
                host.Agent.ReleaseKey(key);
                break;
            case AgentWindowKeyCommandAction.Repeat:
                host.Agent.RepeatKey(key);
                break;
        }
    }

    internal void Mouse(MouseButton button, AgentWindowMouseCommandAction action)
    {
        if (action == AgentWindowMouseCommandAction.Down)
            host.Agent.PressMouse(button);
        else host.Agent.ReleaseMouse(button);
    }

    internal void MoveMouse(Vec2 position) => host.Agent.MoveMouse(position);

    internal void PanMouse(Vec2 delta) => host.Agent.PanMouse(delta);

    internal void ScrollMouse(Vec2 delta) => host.Agent.ScrollMouse(delta);

    internal void ResizeWindow(Vec2u size) => host.Agent.ResizeWindow(size);

    internal void Text(string[]? words) => host.Agent.EnterText(JoinedWords(words));

    internal void Clipboard(string[]? words) => host.Clipboard = JoinedWords(words);

    internal void Screenshot(string path)
    {
        if (screenshot is null)
        {
            executionError = "This agent window host does not support screenshots.";
            return;
        }

        screenshot(path);
    }

    internal void SetFocus(bool value) => host.Agent.SetFocus(value);

    internal void SetVisible(bool value) => host.IsVisible = value;

    internal void Close() => host.Close();

    internal void StopReading() => ContinueReading = false;

    internal void WriteState()
    {
        var mouse = host.MousePosition;
        output.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "time={0:0.########} updates={1} renders={2} mouse=<{3:0.###} {4:0.###}>",
            host.Time,
            host.UpdateCount,
            host.RenderCount,
            mouse.X,
            mouse.Y));
    }

    internal bool TryOptionalVector(float? x, float? y, out Vec2 value)
    {
        if (!x.HasValue && !y.HasValue)
        {
            value = default;
            return true;
        }

        if (!x.HasValue || !y.HasValue)
        {
            executionError = "Expected zero or two vector components.";
            value = default;
            return false;
        }

        value = new(x.Value, y.Value);
        return true;
    }

    private static string JoinedWords(string[]? words) =>
        words is null || words.Length == 0 ? string.Empty : string.Join(' ', words);
}
