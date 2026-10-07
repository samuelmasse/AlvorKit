namespace AlvorKit;

/// <summary>Frozen lifecycle callback arrays: notifications run before index removals.</summary>
internal readonly record struct EntIdxLifecycleCallbacks(
    ReadOnlyMemory<EntLifecycleHandler> Clearing,
    ReadOnlyMemory<EntLifecycleHandler> Disposing,
    ReadOnlyMemory<EntLifecycleHandler> Removing);
