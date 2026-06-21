namespace AlvorKit.Script.AgentLease;

/// <summary>Supported top-level commands for the advisory lease helper.</summary>
internal enum AgentLeaseCommandKind
{
    /// <summary>Create or replace an advisory lease for the current work.</summary>
    Start,

    /// <summary>Refresh an existing advisory lease and optionally update its details.</summary>
    Touch,

    /// <summary>Show active leases, with optional stale leases.</summary>
    List,

    /// <summary>Check proposed paths for active overlapping leases.</summary>
    Check,

    /// <summary>Remove a completed advisory lease.</summary>
    Done,

    /// <summary>Write a short conflict note for an unavoidable overlap.</summary>
    Conflict
}

/// <summary>Parsed command-line request for one lease helper operation.</summary>
/// <param name="Kind">Top-level command to execute.</param>
/// <param name="RepoRoot">Repository root that contains the <c>out/agents</c> coordination directory.</param>
/// <param name="Agent">Optional explicit agent identifier.</param>
/// <param name="TaskDescription">Optional human-readable task summary.</param>
/// <param name="Mode">Optional coordination mode override.</param>
/// <param name="Paths">Repository-relative file paths, directories, or globs associated with the command.</param>
/// <param name="Notes">Optional short note to write into a lease.</param>
/// <param name="Reason">Optional conflict reason for conflict-note commands.</param>
/// <param name="Timeout">Lease duration counted from the command execution time.</param>
/// <param name="IncludeStale">Whether list output should include expired leases.</param>
internal sealed record AgentLeaseCommand(
    AgentLeaseCommandKind Kind,
    string RepoRoot,
    string? Agent,
    string? TaskDescription,
    string? Mode,
    IReadOnlyList<string> Paths,
    string? Notes,
    string? Reason,
    TimeSpan Timeout,
    bool IncludeStale)
{
    /// <summary>Default lease lifetime for fresh and refreshed leases.</summary>
    public static TimeSpan DefaultTimeout => TimeSpan.FromMinutes(5);
}
