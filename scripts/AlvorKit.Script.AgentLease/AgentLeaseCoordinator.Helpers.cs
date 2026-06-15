namespace AlvorKit.Script.AgentLease;

/// <summary>Formatting and normalization helpers for advisory lease coordination.</summary>
internal sealed partial class AgentLeaseCoordinator
{
    /// <summary>Builds a result from a primary message and optional overlap warnings.</summary>
    /// <param name="message">Primary success message.</param>
    /// <param name="overlaps">Active overlapping leases to append.</param>
    private static AgentLeaseResult WithOverlapLines(string message, IReadOnlyList<AgentLease> overlaps) =>
        overlaps.Count == 0
            ? AgentLeaseResult.Success(message)
            : AgentLeaseResult.Success([message, .. OverlapLines("Advisory overlap:", overlaps)]);

    /// <summary>Formats overlap warning lines.</summary>
    /// <param name="header">Header line before the overlap details.</param>
    /// <param name="overlaps">Leases to describe.</param>
    private static IEnumerable<string> OverlapLines(string header, IReadOnlyList<AgentLease> overlaps)
    {
        yield return header;
        foreach (var lease in overlaps)
            yield return $"- {lease.Agent}: {lease.Task} ({lease.Mode}; {string.Join(", ", lease.Paths)})";
    }

    /// <summary>Normalizes and validates user-supplied path claims.</summary>
    /// <param name="paths">Raw path claims from command-line input.</param>
    private static IReadOnlyList<string> NormalizePaths(IReadOnlyList<string> paths) =>
        paths.Select(AgentLeasePath.Normalize).Where(path => path.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>Formats one lease for human-readable list output.</summary>
    /// <param name="lease">Lease to describe.</param>
    /// <param name="now">Current UTC timestamp for active or stale status.</param>
    private static string FormatLease(AgentLease lease, DateTimeOffset now)
    {
        var status = lease.IsActive(now) ? "active" : "stale";
        return $"{status} {lease.Agent} {lease.Mode} expires={lease.ExpiresAt:O} paths={string.Join(", ", lease.Paths)} task={lease.Task}";
    }

    /// <summary>Generates a readable fallback agent identifier for new leases or conflict notes.</summary>
    /// <param name="now">Current UTC timestamp to include in the identifier.</param>
    private static string GenerateAgent(DateTimeOffset now) =>
        $"codex-{now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..34];

    /// <summary>Returns a non-empty string or throws a targeted error.</summary>
    /// <param name="value">Value to validate.</param>
    /// <param name="message">Error message for missing values.</param>
    private static string Required(string? value, string message) =>
        BlankAsNull(value) ?? throw new ArgumentException(message);

    /// <summary>Converts blank strings to null while preserving non-empty strings.</summary>
    /// <param name="value">String value to normalize.</param>
    private static string? BlankAsNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
