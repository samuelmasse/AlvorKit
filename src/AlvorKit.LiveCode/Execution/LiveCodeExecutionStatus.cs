namespace AlvorKit;

/// <summary>Describes the terminal state of a submitted LiveCode command.</summary>
public enum LiveCodeExecutionStatus
{
    /// <summary>The command ran to completion.</summary>
    Completed,

    /// <summary>The selected scope ended before the command reached the game thread.</summary>
    ScopeEnded,

    /// <summary>The frozen-only lane rejected the command because game frames were still advancing.</summary>
    GameRunning,

    /// <summary>The submitted assembly or entry type did not satisfy the command contract.</summary>
    InvalidCommand,

    /// <summary>Construction or execution threw an exception.</summary>
    Failed
}
