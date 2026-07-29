namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Terminal runtime outcome of one source-generated metadata delta.</summary>
public enum SourceUpdateApplyStatus
{
    /// <summary>The delta committed and metadata-update handlers completed.</summary>
    Applied,

    /// <summary>The delta committed, but one or more metadata-update handlers failed.</summary>
    AppliedWithHandlerWarnings,

    /// <summary>The request was rejected before runtime mutation.</summary>
    Rejected,

    /// <summary>The apply result is ambiguous and the process must restart.</summary>
    RestartRequired
}
