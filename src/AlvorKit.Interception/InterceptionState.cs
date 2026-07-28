namespace AlvorKit.Interception;

/// <summary>Describes one profiler request's lifecycle.</summary>
public enum InterceptionState
{
    /// <summary>The profiler is unavailable.</summary>
    Unavailable = 0,

    /// <summary>No operation is represented.</summary>
    Idle = 1,

    /// <summary>The request is waiting in the bounded native queue.</summary>
    Queued = 2,

    /// <summary>The ReJIT request has been submitted to CoreCLR.</summary>
    Requested = 3,

    /// <summary>CoreCLR is compiling replacement IL.</summary>
    Applying = 4,

    /// <summary>The requested patch version is active.</summary>
    Active = 5,

    /// <summary>The original method and its inliners are being restored.</summary>
    Removing = 6,

    /// <summary>The original method is active and the patch record was released.</summary>
    Removed = 7,

    /// <summary>The request failed; a prior active version remains active when replacement failed.</summary>
    Failed = 8
}
