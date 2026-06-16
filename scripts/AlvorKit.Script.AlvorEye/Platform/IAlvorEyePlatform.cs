namespace AlvorKit.Script.AlvorEye;

/// <summary>Operating-system adapter used by AlvorEye execution.</summary>
internal interface IAlvorEyePlatform
{
    /// <summary>Capabilities available on the current platform.</summary>
    PlatformCapabilities Capabilities { get; }

    /// <summary>Waits for a visible target window.</summary>
    Task<TargetWindow> WaitForWindowAsync(ScenarioWindow window, CancellationToken cancellationToken);

    /// <summary>Places a target window in a known visible location.</summary>
    void PlaceWindow(TargetWindow window, ScenarioWindow settings);

    /// <summary>Captures a target window to a PNG file.</summary>
    void CaptureWindow(TargetWindow window, string outputPath);

    /// <summary>Sends one key press or release.</summary>
    void SendKey(TargetWindow window, string key, KeyInputMode mode);

    /// <summary>Sends Unicode text input.</summary>
    void SendText(TargetWindow window, string text);

    /// <summary>Moves the pointer relative to the target window.</summary>
    void MoveMouse(TargetWindow window, int x, int y);

    /// <summary>Clicks a mouse button relative to the target window.</summary>
    void ClickMouse(TargetWindow window, int x, int y, string button);

    /// <summary>Drags a mouse button between two target-window-relative points.</summary>
    void DragMouse(TargetWindow window, int x, int y, int toX, int toY, string button, TimeSpan duration);

    /// <summary>Freezes a target process.</summary>
    void FreezeProcess(int processId);

    /// <summary>Resumes a target process.</summary>
    void ResumeProcess(int processId);
}
