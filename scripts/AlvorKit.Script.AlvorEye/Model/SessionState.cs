namespace AlvorKit.Script.AlvorEye;

/// <summary>Persistent state for a live AlvorEye session.</summary>
internal sealed class SessionState
{
    /// <summary>Session id used by handoff and resume commands.</summary>
    public required string SessionId { get; init; }

    /// <summary>Repository root associated with the session.</summary>
    public required string RepoRoot { get; init; }

    /// <summary>Run output directory.</summary>
    public required string RunDirectory { get; init; }

    /// <summary>Window title used to reacquire the session window.</summary>
    public required string WindowTitle { get; init; }

    /// <summary>Whether the window title is matched exactly.</summary>
    public bool ExactTitle { get; init; }

    /// <summary>Target process id, when known.</summary>
    public int ProcessId { get; init; }

    /// <summary>Whether the target is currently frozen.</summary>
    public bool Frozen { get; set; }

    /// <summary>Queued actions waiting for resume.</summary>
    public List<AlvorEyeAction> QueuedActions { get; init; } = [];
}
