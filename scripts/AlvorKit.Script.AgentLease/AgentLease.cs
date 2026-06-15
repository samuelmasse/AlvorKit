namespace AlvorKit.Script.AgentLease;

/// <summary>Describes one advisory lease written by an agent coordinating repository work.</summary>
internal sealed record AgentLease
{
    /// <summary>Stable identifier for the agent that owns the advisory lease.</summary>
    public required string Agent { get; init; }

    /// <summary>Short human-readable summary of the work currently in progress.</summary>
    public required string Task { get; init; }

    /// <summary>Coordination mode, such as write, generate, format, test, cleanup, or review.</summary>
    public required string Mode { get; init; }

    /// <summary>Repository-relative file paths, directories, or globs that the agent expects to touch.</summary>
    public required IReadOnlyList<string> Paths { get; init; }

    /// <summary>UTC timestamp when the lease was first created.</summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>UTC timestamp when the lease was last refreshed.</summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>UTC timestamp after which other agents should treat the lease as stale.</summary>
    public required DateTimeOffset ExpiresAt { get; init; }

    /// <summary>Optional short coordination note for unusual risk, broad commands, or sequencing constraints.</summary>
    public string? Notes { get; init; }

    /// <summary>Returns whether the lease is still active at the supplied UTC timestamp.</summary>
    /// <param name="now">Current UTC timestamp used for deterministic staleness checks.</param>
    public bool IsActive(DateTimeOffset now) => ExpiresAt > now;
}
