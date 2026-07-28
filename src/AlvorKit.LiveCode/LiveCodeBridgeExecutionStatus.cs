namespace AlvorKit.LiveCode;

/// <summary>Terminal status of one structured bridge invocation.</summary>
public enum LiveCodeBridgeExecutionStatus
{
    /// <summary>The bridge completed successfully.</summary>
    Completed,

    /// <summary>The requested bridge name is not registered.</summary>
    NotFound,

    /// <summary>The requested bridge version does not match the registered contract.</summary>
    VersionMismatch,

    /// <summary>The bridge threw or rejected its payload.</summary>
    Failed
}
