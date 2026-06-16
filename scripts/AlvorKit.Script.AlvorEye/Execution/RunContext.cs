namespace AlvorKit.Script.AlvorEye;

/// <summary>Mutable execution state for one AlvorEye run or session.</summary>
internal sealed class RunContext(
    string repoRoot,
    AlvorEyeScenario scenario,
    IAlvorEyePlatform platform,
    SessionStore sessionStore,
    RunManifest manifest,
    string framesDirectory)
{
    /// <summary>Repository root used for relative output paths.</summary>
    public string RepoRoot { get; } = repoRoot;

    /// <summary>Scenario being executed.</summary>
    public AlvorEyeScenario Scenario { get; } = scenario;

    /// <summary>Platform adapter receiving OS calls.</summary>
    public IAlvorEyePlatform Platform { get; } = platform;

    /// <summary>Persistent session store.</summary>
    public SessionStore SessionStore { get; } = sessionStore;

    /// <summary>Run manifest being built.</summary>
    public RunManifest Manifest { get; } = manifest;

    /// <summary>Frame output directory.</summary>
    public string FramesDirectory { get; } = framesDirectory;

    /// <summary>Target window used by actions.</summary>
    public TargetWindow Target { get; set; }

    /// <summary>Launched process, when AlvorEye owns it.</summary>
    public Process? Process { get; set; }

    /// <summary>Background log copy tasks for the launched process.</summary>
    public List<Task> LogCopyTasks { get; } = [];

    /// <summary>Persistent session state, when executing a session.</summary>
    public SessionState? Session { get; set; }

    /// <summary>Next frame index.</summary>
    public int FrameIndex { get; set; }

    /// <summary>Last captured frame path.</summary>
    public string? LastFramePath { get; set; }
}
