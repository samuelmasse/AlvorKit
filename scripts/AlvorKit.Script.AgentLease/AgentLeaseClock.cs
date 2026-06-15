namespace AlvorKit.Script.AgentLease;

/// <summary>Supplies timestamps to lease operations.</summary>
internal interface IAgentLeaseClock
{
    /// <summary>Current UTC timestamp used for lease expiration and refresh calculations.</summary>
    DateTimeOffset UtcNow { get; }
}

/// <summary>Clock implementation backed by the system UTC clock.</summary>
internal sealed class SystemAgentLeaseClock : IAgentLeaseClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
