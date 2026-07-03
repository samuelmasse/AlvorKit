namespace AlvorKit.Windowing;

/// <summary>Stores deterministic window state owned by an agent-mode GLFW host.</summary>
internal sealed class AgentWindowState
{
    private readonly Vec2u monitorSize = new(1920u, 1080u);
    private readonly float monitorScale = 1f;
    private Vec2u clientSize;

    /// <summary>Gets or sets the simulated client size.</summary>
    internal Vec2u ClientSize
    {
        get => clientSize;
        set
        {
            ValidateClientSize(value);
            clientSize = value;
        }
    }

    /// <summary>Gets the simulated monitor size.</summary>
    internal Vec2u MonitorSize => monitorSize;

    /// <summary>Gets the simulated monitor scale.</summary>
    internal float MonitorScale => monitorScale;

    /// <summary>Gets whether the simulated window is fullscreen.</summary>
    internal bool IsFullscreen => WindowState == WindowState.Fullscreen;

    /// <summary>Gets or sets whether the simulated window has been asked to exit.</summary>
    internal bool IsExiting { get; set; }

    /// <summary>Gets or sets whether the simulated window reports focus.</summary>
    internal bool IsFocused { get; set; } = true;

    /// <summary>Gets or sets whether the simulated window is visible.</summary>
    internal bool IsVisible { get; set; }

    /// <summary>Gets or sets the simulated mouse position.</summary>
    internal Vec2 MousePosition { get; set; }

    /// <summary>Gets or sets the simulated window state.</summary>
    internal WindowState WindowState { get; set; }

    /// <summary>Gets or sets the simulated cursor mode.</summary>
    internal CursorMode CursorMode { get; set; }

    /// <summary>Gets or sets the simulated cursor shape.</summary>
    internal CursorShape CursorShape { get; set; }

    /// <summary>Gets or sets whether simulated vertical sync is enabled.</summary>
    internal bool IsVSyncEnabled { get; set; }

    /// <summary>Gets or sets the simulated window title.</summary>
    internal string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets simulated clipboard text.</summary>
    internal string Clipboard { get; set; } = string.Empty;

    /// <summary>Gets deterministic input state observed by diagnostic agent commands.</summary>
    internal AgentWindowInputState Input { get; } = new();

    /// <summary>Gets deterministic seconds elapsed since this host entered agent mode.</summary>
    internal double Time { get; private set; }

    /// <summary>Gets the number of agent-requested update callbacks issued by this host.</summary>
    internal int UpdateCount { get; private set; }

    /// <summary>Gets the number of agent-requested render callbacks issued by this host.</summary>
    internal int RenderCount { get; private set; }

    /// <summary>Gets or sets the number of buffer swaps requested in agent mode.</summary>
    internal int SwapBuffersCount { get; set; }

    /// <summary>Gets or sets the number of times the agent command loop was started.</summary>
    internal int RunCount { get; set; }

    /// <summary>Initializes state from the native window values observed when the host was created.</summary>
    internal void Initialize(Vec2u initialClientSize, string initialTitle, bool initialIsVisible, bool initialIsVSyncEnabled)
    {
        ClientSize = initialClientSize;
        Title = initialTitle;
        IsVisible = initialIsVisible;
        IsVSyncEnabled = initialIsVSyncEnabled;
    }

    /// <summary>Marks the host as exiting if it has not already been closed.</summary>
    /// <returns><see langword="true" /> when this call changed the close state.</returns>
    internal bool TryClose()
    {
        if (IsExiting)
            return false;

        IsExiting = true;
        return true;
    }

    /// <summary>Advances deterministic time and records one update.</summary>
    internal bool TryUpdate(double deltaSeconds)
    {
        ValidateDelta(deltaSeconds);
        if (IsExiting)
            return false;

        Time += deltaSeconds;
        UpdateCount++;
        return true;
    }

    /// <summary>Records one render request without advancing deterministic time.</summary>
    internal bool TryRender(double deltaSeconds)
    {
        ValidateDelta(deltaSeconds);
        if (IsExiting)
            return false;

        RenderCount++;
        return true;
    }

    /// <summary>Rejects negative update counts before a batch starts.</summary>
    internal static void ValidateCount(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Frame count must be non-negative.");
    }

    /// <summary>Rejects negative, infinite, and NaN frame deltas.</summary>
    private static void ValidateDelta(double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Frame delta must be a finite non-negative value.");
    }

    /// <summary>Rejects drawable client sizes that cannot produce a usable framebuffer.</summary>
    private static void ValidateClientSize(Vec2u size)
    {
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Client size must be positive on both axes.");

        if (size.X > int.MaxValue || size.Y > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(size), "Client size must fit the native signed 32-bit window API.");
    }
}
