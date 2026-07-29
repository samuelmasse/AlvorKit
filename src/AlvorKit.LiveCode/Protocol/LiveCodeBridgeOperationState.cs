namespace AlvorKit.LiveCode;

/// <summary>Current lifecycle state of one accepted two-phase bridge invocation.</summary>
public enum LiveCodeBridgeOperationState
{
    /// <summary>The operation is queued for the game thread's next safe-frame pump.</summary>
    Pending,

    /// <summary>The game-thread pump is executing the bridge.</summary>
    Running,

    /// <summary>The bridge finished and its terminal result is available.</summary>
    Completed
}
