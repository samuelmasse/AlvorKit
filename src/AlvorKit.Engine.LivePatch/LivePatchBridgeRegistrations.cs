namespace AlvorKit.Engine;

/// <summary>
/// Owns submitted handler contexts and tracks their transition from retained
/// through collectible unload.
/// </summary>
internal sealed class LivePatchBridgeRegistrations
{
    private readonly Dictionary<ulong, LivePatchSubmittedPatch> submitted = [];
    private readonly Dictionary<ulong, WeakReference> unloaded = [];

    /// <summary>Returns a stable snapshot of retained submissions.</summary>
    internal LivePatchSubmittedPatch[] ActiveSnapshot() => [.. submitted.Values];

    /// <summary>Tries to read a retained submission.</summary>
    internal bool TryGet(ulong patchId, out LivePatchSubmittedPatch item) =>
        submitted.TryGetValue(patchId, out item!);

    /// <summary>Retains a newly installed submission.</summary>
    internal void Add(
        LivePatchLease lease,
        LivePatchLoadedSubmission loaded,
        InjectorScopeId executorScopeId)
    {
        submitted.Add(
            lease.PatchId,
            Create(lease, loaded, executorScopeId));
    }

    /// <summary>Publishes a replacement context and begins unloading its predecessor.</summary>
    internal void Replace(
        LivePatchSubmittedPatch existing,
        LivePatchLoadedSubmission loaded,
        InjectorScopeId executorScopeId)
    {
        existing.Context.Unload();
        unloaded[existing.Lease.PatchId] = existing.ContextReference;
        submitted[existing.Lease.PatchId] = Create(
            existing.Lease,
            loaded,
            executorScopeId);
    }

    /// <summary>Stops retaining a submission and starts collectible unload.</summary>
    internal void Release(LivePatchSubmittedPatch item)
    {
        if (!submitted.Remove(item.Lease.PatchId))
            return;
        item.Lease.Dispose();
        item.Context.Unload();
        unloaded[item.Lease.PatchId] = item.ContextReference;
    }

    /// <summary>Reports one submitted context's retention or collection state.</summary>
    internal object ContextState(ulong patchId)
    {
        if (submitted.TryGetValue(patchId, out var active))
        {
            return new
            {
                state = "retained",
                active.EntryType,
                active.HandlerMethod,
                executorScopeId = active.ExecutorScopeId.Value
            };
        }
        if (unloaded.TryGetValue(patchId, out var weak))
            return new { state = weak.IsAlive ? "unloading" : "collected" };
        return new { state = "not-owned" };
    }

    /// <summary>Reports every retained and unloading submission in patch order.</summary>
    internal object[] ContextStates() =>
    [
        .. submitted.Values
            .OrderBy(item => item.Lease.PatchId)
            .Select(item => new
            {
                patchId = item.Lease.PatchId,
                state = "retained",
                item.EntryType,
                item.HandlerMethod,
                executorScopeId = item.ExecutorScopeId.Value
            })
            .Cast<object>(),
        .. unloaded
            .OrderBy(item => item.Key)
            .Select(item => new
            {
                patchId = item.Key,
                state = item.Value.IsAlive ? "unloading" : "collected"
            })
            .Cast<object>()
    ];

    private static LivePatchSubmittedPatch Create(
        LivePatchLease lease,
        LivePatchLoadedSubmission loaded,
        InjectorScopeId executorScopeId) =>
        new(
            lease,
            loaded.Context,
            new(loaded.Context, trackResurrection: false),
            executorScopeId,
            loaded.EntryType,
            loaded.HandlerMethod.Name);
}
