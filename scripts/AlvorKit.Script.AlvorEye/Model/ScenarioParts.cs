namespace AlvorKit.Script.AlvorEye;

/// <summary>Process launch settings for a scenario.</summary>
internal sealed class ScenarioRun
{
    /// <summary>Executable file path.</summary>
    public required string Executable { get; init; }

    /// <summary>Command-line arguments passed to the executable.</summary>
    public IReadOnlyList<string> Args { get; init; } = [];

    /// <summary>Optional working directory.</summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>Environment variables added to the child process.</summary>
    public IReadOnlyDictionary<string, string> Environment { get; init; } = new Dictionary<string, string>();
}

/// <summary>Window discovery and placement settings for a scenario.</summary>
internal sealed class ScenarioWindow
{
    /// <summary>Title string used to find the target window.</summary>
    public required string Title { get; init; }

    /// <summary>Whether the title must match exactly.</summary>
    public bool Exact { get; init; }

    /// <summary>Timeout for finding the target window.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);

    /// <summary>Desired full-window width after placement, or null to keep the existing width.</summary>
    public int? Width { get; init; }

    /// <summary>Desired full-window height after placement, or null to keep the existing height.</summary>
    public int? Height { get; init; }
}

/// <summary>Output settings for a scenario.</summary>
internal sealed class ScenarioOutput
{
    /// <summary>Optional run id used for the default output directory.</summary>
    public string? RunId { get; init; }

    /// <summary>Optional explicit output directory.</summary>
    public string? Directory { get; init; }
}

/// <summary>Freeze settings for handoff actions.</summary>
internal sealed class FreezeOptions
{
    /// <summary>Freeze strategy; v1 supports <c>processSuspend</c>.</summary>
    public string Strategy { get; init; } = "processSuspend";

    /// <summary>Delay after resume before executing queued input.</summary>
    public TimeSpan ResumeSettle { get; init; } = TimeSpan.FromMilliseconds(250);
}
