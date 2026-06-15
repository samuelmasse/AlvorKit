namespace AlvorKit.Script.AgentLease;

/// <summary>Command result with process exit code and console lines.</summary>
/// <param name="ExitCode">Process exit code to return to the caller.</param>
/// <param name="Lines">Console lines to print in order.</param>
internal sealed record AgentLeaseResult(int ExitCode, IReadOnlyList<string> Lines)
{
    /// <summary>Creates a successful command result.</summary>
    /// <param name="lines">Lines to print to standard output.</param>
    public static AgentLeaseResult Success(params string[] lines) => new(0, lines);

    /// <summary>Creates a result for an advisory conflict.</summary>
    /// <param name="lines">Lines describing the overlapping active leases.</param>
    public static AgentLeaseResult Conflict(params string[] lines) => new(2, lines);
}
