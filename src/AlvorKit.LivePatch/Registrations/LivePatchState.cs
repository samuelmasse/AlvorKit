namespace AlvorKit.LivePatch;

/// <summary>User-facing lifecycle of a scoped live behavior.</summary>
public enum LivePatchState
{
    /// <summary>The native exact wrapper is queued or compiling.</summary>
    Installing,

    /// <summary>The exact wrapper and managed handler are selectable.</summary>
    Active,

    /// <summary>No new calls can acquire the handler and native original IL is being restored.</summary>
    Removing,

    /// <summary>The handler was released and original behavior is selected.</summary>
    Removed,

    /// <summary>Installation or removal failed.</summary>
    Failed
}
