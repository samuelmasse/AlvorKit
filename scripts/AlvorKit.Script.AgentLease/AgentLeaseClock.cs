namespace AlvorKit.Script.AgentLease;

/// <summary>Supplies timestamps to lease operations.</summary>
internal interface AgentLeaseClock
{
    /// <summary>Current UTC timestamp used for lease expiration and refresh calculations.</summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>Clock implementation backed by the system UTC clock.</summary>
internal sealed class SystemAgentLeaseClock : AgentLeaseClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
