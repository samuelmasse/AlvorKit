namespace AlvorKit.Script.AlvorEye;

/// <summary>Platform adapter used when the current operating system has no v1 implementation.</summary>
[ExcludeFromCodeCoverage]
internal sealed class UnsupportedAlvorEyePlatform(string platformName) : IAlvorEyePlatform
{
    /// <inheritdoc/>
    public PlatformCapabilities Capabilities { get; } = new();

    /// <inheritdoc/>
    public Task<TargetWindow> WaitForWindowAsync(ScenarioWindow window, CancellationToken cancellationToken) =>
        throw Unsupported();

    /// <inheritdoc/>
    public void PlaceWindow(TargetWindow window, ScenarioWindow settings) => throw Unsupported();

    /// <inheritdoc/>
    public void CaptureWindow(TargetWindow window, string outputPath) => throw Unsupported();

    /// <inheritdoc/>
    public void SendKey(TargetWindow window, string key, KeyInputMode mode) => throw Unsupported();

    /// <inheritdoc/>
    public void SendText(TargetWindow window, string text) => throw Unsupported();

    /// <inheritdoc/>
    public void MoveMouse(TargetWindow window, int x, int y) => throw Unsupported();

    /// <inheritdoc/>
    public void ClickMouse(TargetWindow window, int x, int y, string button) => throw Unsupported();

    /// <inheritdoc/>
    public void DragMouse(TargetWindow window, int x, int y, int toX, int toY, string button, TimeSpan duration) =>
        throw Unsupported();

    /// <inheritdoc/>
    public void FreezeProcess(int processId) => throw Unsupported();

    /// <inheritdoc/>
    public void ResumeProcess(int processId) => throw Unsupported();

    /// <summary>Creates a consistent unsupported-platform exception.</summary>
    private NotSupportedException Unsupported() => new($"AlvorEye v1 has no {platformName} platform adapter yet.");
}
