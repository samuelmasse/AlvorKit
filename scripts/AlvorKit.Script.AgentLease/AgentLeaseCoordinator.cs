namespace AlvorKit.Script.AgentLease;

/// <summary>Coordinates advisory lease operations against the repository lease store.</summary>
/// <param name="repository">Lease file repository.</param>
/// <param name="clock">Clock used for deterministic expiration and refresh behavior.</param>
/// <param name="currentAgent">Function that returns the ambient agent identifier, usually from an environment variable.</param>
internal sealed partial class AgentLeaseCoordinator(
    AgentLeaseRepository repository,
    IAgentLeaseClock clock,
    Func<string?> currentAgent)
{
    /// <summary>Executes a parsed lease helper command.</summary>
    /// <param name="command">Command to execute.</param>
    public AgentLeaseResult Execute(AgentLeaseCommand command) =>
        command.Kind switch
        {
            AgentLeaseCommandKind.Start => Start(command),
            AgentLeaseCommandKind.Touch => Touch(command),
            AgentLeaseCommandKind.List => List(command),
            AgentLeaseCommandKind.Check => Check(command),
            AgentLeaseCommandKind.Done => Done(command),
            AgentLeaseCommandKind.Conflict => Conflict(command),
            _ => AgentLeaseResult.Success(AgentLeaseCommandParser.HelpText)
        };

    /// <summary>Creates or replaces a lease and reports any active overlapping leases.</summary>
    /// <param name="command">Start command with task, mode, and path claims.</param>
    private AgentLeaseResult Start(AgentLeaseCommand command)
    {
        var now = clock.UtcNow;
        var agent = ResolveAgent(command.Agent, generateWhenMissing: true, now);
        var paths = NormalizePaths(command.Paths);
        var lease = new AgentLease
        {
            Agent = agent,
            Task = Required(command.TaskDescription, "start requires --task."),
            Mode = AgentLeaseModes.Normalize(command.Mode ?? AgentLeaseModes.Default),
            Paths = paths,
            StartedAt = now,
            UpdatedAt = now,
            ExpiresAt = now + command.Timeout,
            Notes = BlankAsNull(command.Notes)
        };

        repository.Write(lease);
        return WithOverlapLines($"Started lease {agent} until {lease.ExpiresAt:O}.", ActiveOverlaps(paths, agent, now));
    }

    /// <summary>Refreshes an existing lease and optionally updates task, mode, paths, or notes.</summary>
    /// <param name="command">Touch command with optional replacement fields.</param>
    private AgentLeaseResult Touch(AgentLeaseCommand command)
    {
        var now = clock.UtcNow;
        var agent = ResolveAgent(command.Agent, generateWhenMissing: false, now);
        var existing = repository.Read(agent) ?? throw new InvalidOperationException($"No lease found for agent '{agent}'.");
        var paths = command.Paths.Count == 0 ? existing.Paths : NormalizePaths(command.Paths);
        var updated = existing with
        {
            Task = command.TaskDescription ?? existing.Task,
            Mode = command.Mode is null ? existing.Mode : AgentLeaseModes.Normalize(command.Mode),
            Paths = paths,
            UpdatedAt = now,
            ExpiresAt = now + command.Timeout,
            Notes = command.Notes is null ? existing.Notes : BlankAsNull(command.Notes)
        };

        repository.Write(updated);
        return WithOverlapLines($"Touched lease {agent} until {updated.ExpiresAt:O}.", ActiveOverlaps(paths, agent, now));
    }

    /// <summary>Lists active leases and optionally stale leases.</summary>
    /// <param name="command">List command controlling stale lease visibility.</param>
    private AgentLeaseResult List(AgentLeaseCommand command)
    {
        var now = clock.UtcNow;
        var leases = repository.ReadAll();
        var visible = command.IncludeStale ? leases : leases.Where(lease => lease.IsActive(now)).ToArray();
        var lines = visible.Select(lease => FormatLease(lease, now)).ToList();

        if (lines.Count == 0)
            lines.Add(command.IncludeStale ? "No agent leases found." : "No active agent leases found.");

        var staleCount = leases.Count(lease => !lease.IsActive(now));
        if (!command.IncludeStale && staleCount > 0)
            lines.Add($"Stale leases omitted: {staleCount}. Use --include-stale to show them.");

        return new(0, lines);
    }

    /// <summary>Checks path claims for active overlaps and returns exit code 2 when any are present.</summary>
    /// <param name="command">Check command containing proposed path claims.</param>
    private AgentLeaseResult Check(AgentLeaseCommand command)
    {
        var agent = ResolveOptionalAgent(command.Agent);
        var overlaps = ActiveOverlaps(NormalizePaths(command.Paths), agent, clock.UtcNow);
        return overlaps.Count == 0
            ? AgentLeaseResult.Success("No active overlapping leases.")
            : AgentLeaseResult.Conflict([.. OverlapLines("Active overlapping leases:", overlaps)]);
    }

    /// <summary>Deletes one lease after work is complete.</summary>
    /// <param name="command">Done command identifying the lease to remove.</param>
    private AgentLeaseResult Done(AgentLeaseCommand command)
    {
        var agent = ResolveAgent(command.Agent, generateWhenMissing: false, clock.UtcNow);
        var deleted = repository.Delete(agent);
        return AgentLeaseResult.Success(deleted ? $"Removed lease {agent}." : $"No lease found for agent {agent}.");
    }

    /// <summary>Writes a conflict note for unavoidable overlapping work.</summary>
    /// <param name="command">Conflict command with reason and path claims.</param>
    private AgentLeaseResult Conflict(AgentLeaseCommand command)
    {
        var now = clock.UtcNow;
        var agent = ResolveAgent(command.Agent, generateWhenMissing: true, now);
        var paths = NormalizePaths(command.Paths);
        var overlaps = ActiveOverlaps(paths, agent, now);
        var task = command.TaskDescription ?? "Unspecified task";
        var path = repository.WriteConflictNote(agent, task, paths, Required(command.Reason, "conflict requires --reason."), overlaps, now);
        return AgentLeaseResult.Success($"Wrote conflict note {path}.");
    }

    /// <summary>Returns active leases whose paths may overlap the supplied path claims.</summary>
    /// <param name="paths">Normalized path claims to compare.</param>
    /// <param name="agentToIgnore">Optional owning agent to exclude from overlap checks.</param>
    /// <param name="now">Current UTC timestamp for staleness filtering.</param>
    private IReadOnlyList<AgentLease> ActiveOverlaps(IReadOnlyList<string> paths, string? agentToIgnore, DateTimeOffset now) =>
        repository.ReadAll()
            .Where(lease => lease.IsActive(now))
            .Where(lease => !string.Equals(lease.Agent, agentToIgnore, StringComparison.OrdinalIgnoreCase))
            .Where(lease => paths.Any(path => lease.Paths.Any(existing => AgentLeasePath.MayOverlap(path, existing))))
            .OrderBy(lease => lease.Agent, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>Resolves the agent identifier from explicit input, environment, or generated fallback.</summary>
    /// <param name="agent">Explicit agent identifier supplied by the command.</param>
    /// <param name="generateWhenMissing">Whether a deterministic command may create a fresh identifier.</param>
    /// <param name="now">Current UTC timestamp for generated identifiers.</param>
    private string ResolveAgent(string? agent, bool generateWhenMissing, DateTimeOffset now) =>
        ResolveOptionalAgent(agent) ?? (generateWhenMissing ? GenerateAgent(now) : throw new InvalidOperationException(
            "Pass --agent or set ALVORKIT_AGENT_ID so the helper does not update another agent's lease."));

    /// <summary>Resolves an explicit or ambient agent identifier when one exists.</summary>
    /// <param name="agent">Explicit agent identifier supplied by the command.</param>
    private string? ResolveOptionalAgent(string? agent) =>
        BlankAsNull(agent) ?? BlankAsNull(currentAgent());

}
