namespace AlvorKit;

/// <summary>Lifecycle of one local agent workspace associated with a live game process.</summary>
public enum LiveWorkspaceStatus
{
    /// <summary>The workspace may receive new observations and interventions.</summary>
    Active,

    /// <summary>The workspace was audited and intentionally closed.</summary>
    Closed
}
