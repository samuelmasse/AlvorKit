namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Fake platform adapter for action-executor unit tests.</summary>
internal sealed class FakeAlvorEyePlatform : IAlvorEyePlatform
{
    /// <summary>Recorded operation names.</summary>
    public List<string> Calls { get; } = [];

    /// <inheritdoc/>
    public PlatformCapabilities Capabilities { get; } = new();

    /// <inheritdoc/>
    public Task<TargetWindow> WaitForWindowAsync(ScenarioWindow window, CancellationToken cancellationToken) =>
        Task.FromResult(new TargetWindow(1, window.Title, 123));

    /// <inheritdoc/>
    public void PlaceWindow(TargetWindow window, ScenarioWindow settings) => Calls.Add("place");

    /// <inheritdoc/>
    public void CaptureWindow(TargetWindow window, string outputPath)
    {
        Calls.Add("capture");
        TestPng.WriteRed(outputPath);
    }

    /// <inheritdoc/>
    public void SendKey(TargetWindow window, string key, KeyInputMode mode) => Calls.Add($"key:{key}:{mode}");

    /// <inheritdoc/>
    public void SendText(TargetWindow window, string text) => Calls.Add($"text:{text}");

    /// <inheritdoc/>
    public void MoveMouse(TargetWindow window, int x, int y) => Calls.Add($"move:{x}:{y}");

    /// <inheritdoc/>
    public void ClickMouse(TargetWindow window, int x, int y, string button) => Calls.Add($"click:{button}");

    /// <inheritdoc/>
    public void DragMouse(TargetWindow window, int x, int y, int toX, int toY, string button, TimeSpan duration) =>
        Calls.Add($"drag:{button}");

    /// <inheritdoc/>
    public void FreezeProcess(int processId) => Calls.Add($"freeze:{processId}");

    /// <inheritdoc/>
    public void ResumeProcess(int processId) => Calls.Add($"resume:{processId}");
}
