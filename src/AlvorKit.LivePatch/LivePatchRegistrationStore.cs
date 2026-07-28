namespace AlvorKit.LivePatch;

/// <summary>
/// Owns active LivePatch registrations and bounded terminal evidence while the
/// containing session holds its synchronization gate.
/// </summary>
internal sealed class LivePatchRegistrationStore
{
    private readonly Dictionary<ulong, LivePatchRegistration> active = [];
    private readonly Dictionary<ulong, LivePatchSnapshot> history = [];

    /// <summary>Adds one newly installed registration.</summary>
    internal void Add(LivePatchRegistration registration) =>
        active.Add(registration.PatchId, registration);

    /// <summary>Returns a stable snapshot of all active registrations.</summary>
    internal LivePatchRegistration[] ActiveSnapshot() => [.. active.Values];

    /// <summary>Returns active registrations that share one native method slot.</summary>
    internal LivePatchRegistration[] ForMethod(LivePatchMethodSlot method) =>
        [.. active.Values.Where(item => ReferenceEquals(item.Method, method))];

    /// <summary>Reports whether the patch is currently active.</summary>
    internal bool ContainsActive(ulong patchId) => active.ContainsKey(patchId);

    /// <summary>Reports whether terminal evidence exists for the patch.</summary>
    internal bool ContainsHistory(ulong patchId) => history.ContainsKey(patchId);

    /// <summary>Clears active registrations after their owned resources are released.</summary>
    internal void ClearActive() => active.Clear();

    /// <summary>Requires an active registration.</summary>
    internal LivePatchRegistration Require(ulong patchId) =>
        active.TryGetValue(patchId, out var registration)
            ? registration
            : throw new KeyNotFoundException($"LivePatch {patchId} is not active.");

    /// <summary>Returns active and retained terminal evidence in stable patch order.</summary>
    internal LivePatchSnapshot[] List() =>
    [
        .. history.Values
            .Concat(active.Values.Select(Snapshot))
            .OrderBy(item => item.PatchId)
    ];

    /// <summary>Reads one active or retained terminal patch.</summary>
    internal LivePatchSnapshot Get(ulong patchId)
    {
        if (active.TryGetValue(patchId, out var registration))
            return Snapshot(registration);
        if (history.TryGetValue(patchId, out var snapshot))
            return snapshot;
        throw new KeyNotFoundException($"LivePatch {patchId} does not exist.");
    }

    /// <summary>Moves an active registration into bounded terminal history.</summary>
    internal void Complete(
        LivePatchRegistration registration,
        LivePatchState state)
    {
        registration.State = state;
        history[registration.PatchId] = Snapshot(registration);
        active.Remove(registration.PatchId);
        while (history.Count > 256)
            history.Remove(history.Keys.Min());
    }

    /// <summary>Copies final native completion evidence into every related history record.</summary>
    internal void RefreshTerminalNativeEvidence(LivePatchMethodSlot method)
    {
        var completion = method.Completion;
        var patchIds = history
            .Where(item => item.Value.NativePatchId == method.NativePatch.PatchId)
            .Select(item => item.Key)
            .ToArray();
        foreach (var patchId in patchIds)
        {
            history[patchId] = history[patchId] with
            {
                NativeRequestId = method.NativePatch.LastRequestId,
                NativeOperation = completion.Operation,
                NativeState = completion.State,
                RejitElapsed = completion.Elapsed,
                HResult = completion.HResult
            };
        }
    }

    private static LivePatchSnapshot Snapshot(LivePatchRegistration registration)
    {
        var completion = registration.Method.Completion;
        return new(
            registration.PatchId,
            registration.Name,
            registration.Target,
            registration.Selector.ToString(),
            registration.State,
            registration.Method.NativePatch.PatchId,
            registration.Method.NativePatch.LastRequestId,
            completion.Operation,
            completion.State,
            completion.Elapsed,
            completion.HResult,
            registration.Failure);
    }
}
