namespace AlvorKit.Script.AlvorEye;

/// <summary>Executable AlvorEye scenario loaded from JSON.</summary>
internal sealed class AlvorEyeScenario
{
    /// <summary>Process launch settings, or null when attaching to an existing window.</summary>
    public ScenarioRun? Run { get; init; }

    /// <summary>Window discovery and placement settings.</summary>
    public required ScenarioWindow Window { get; init; }

    /// <summary>Output directory settings.</summary>
    public required ScenarioOutput Output { get; init; }

    /// <summary>Freeze behavior used by handoff actions.</summary>
    public FreezeOptions Freeze { get; init; } = new();

    /// <summary>Ordered actions executed by run and session startup.</summary>
    public IReadOnlyList<AlvorEyeAction> Timeline { get; init; } = [];
}
