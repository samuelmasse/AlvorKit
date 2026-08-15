namespace AlvorKit;

/// <summary>Identifies when an invocation argument snapshot was observed.</summary>
internal enum MockSnapshotPhase
{
    /// <summary>The argument state before matcher and behavior execution.</summary>
    Entry,

    /// <summary>The argument state after normal behavior completion and writeback.</summary>
    Exit
}
