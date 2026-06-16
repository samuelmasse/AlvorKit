namespace AlvorKit.Script.AlvorEye;

/// <summary>Windows implementation of AlvorEye platform automation.</summary>
[ExcludeFromCodeCoverage]
[SupportedOSPlatform("windows6.1")]
internal sealed partial class WindowsAlvorEyePlatform : IAlvorEyePlatform
{
    /// <summary>Window restore command.</summary>
    private const int ShowRestore = 9;

    /// <summary>Closest monitor lookup flag.</summary>
    private const uint MonitorDefaultToNearest = 2;

    /// <summary>Top-most window insertion marker.</summary>
    private static readonly nint TopMost = new(-1);

    /// <inheritdoc/>
    public PlatformCapabilities Capabilities { get; } = new()
    {
        WindowDiscovery = true,
        WindowPlacement = true,
        Capture = true,
        Keyboard = true,
        Mouse = true,
        ProcessFreeze = true
    };

    /// <summary>Creates the Windows adapter and marks the process DPI-aware.</summary>
    public WindowsAlvorEyePlatform() => WindowsNative.SetProcessDPIAware();

    /// <inheritdoc/>
    public async Task<TargetWindow> WaitForWindowAsync(ScenarioWindow window, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.Add(window.Timeout);
        do
        {
            if (TryFindWindow(window.Title, window.Exact, out var target))
                return target;

            await Task.Delay(100, cancellationToken);
        } while (DateTimeOffset.UtcNow < deadline);

        throw new InvalidOperationException($"Timed out waiting for a visible window matching '{window.Title}'.");
    }

    /// <inheritdoc/>
    public void PlaceWindow(TargetWindow window, ScenarioWindow settings)
    {
        WindowsNative.ShowWindow(window.Handle, ShowRestore);
        var rect = ReadWindowRect(window);
        var monitor = WindowsNative.MonitorFromWindow(window.Handle, MonitorDefaultToNearest);
        var monitorInfo = new WindowsNative.MonitorInfo();
        monitorInfo.Size = Marshal.SizeOf(monitorInfo);
        if (!WindowsNative.GetMonitorInfo(monitor, ref monitorInfo))
            throw new InvalidOperationException("Could not read monitor bounds.");

        var width = settings.Width ?? rect.Right - rect.Left;
        var height = settings.Height ?? rect.Bottom - rect.Top;
        WindowsNative.SetWindowPos(window.Handle, TopMost, monitorInfo.Work.Left + 24, monitorInfo.Work.Top + 24, width, height, 0x0040);
        WindowsNative.SetForegroundWindow(window.Handle);
    }

    /// <inheritdoc/>
    public void CaptureWindow(TargetWindow window, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var rect = ReadWindowRect(window);
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new(width, height));
        bitmap.Save(outputPath, ImageFormat.Png);
    }

    /// <summary>Finds a visible top-level window by title.</summary>
    private static bool TryFindWindow(string title, bool exact, out TargetWindow target)
    {
        TargetWindow found = default;
        WindowsNative.EnumWindows((handle, _) =>
        {
            if (!WindowsNative.IsWindowVisible(handle))
                return true;

            var buffer = new StringBuilder(512);
            WindowsNative.GetWindowText(handle, buffer, buffer.Capacity);
            var candidate = buffer.ToString();
            var matches = exact ? candidate == title : candidate.Contains(title, StringComparison.OrdinalIgnoreCase);
            if (!matches)
                return true;

            WindowsNative.GetWindowThreadProcessId(handle, out var processId);
            found = new(handle, candidate, (int)processId);
            return false;
        }, 0);

        target = found;
        return found.Handle != 0;
    }

    /// <summary>Reads the full target window rectangle.</summary>
    private static WindowsNative.Rect ReadWindowRect(TargetWindow window)
    {
        if (!WindowsNative.GetWindowRect(window.Handle, out var rect))
            throw new InvalidOperationException("Could not read target window bounds.");
        if (rect.Right <= rect.Left || rect.Bottom <= rect.Top)
            throw new InvalidOperationException("The target window has invalid bounds.");
        return rect;
    }
}
