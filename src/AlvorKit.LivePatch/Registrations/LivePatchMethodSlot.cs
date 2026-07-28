namespace AlvorKit.LivePatch;

internal sealed class LivePatchMethodSlot(
    ulong slotId,
    LivePatchSlot dispatch,
    IInterceptionPatchHandle nativePatch,
    InterceptionClaimLease claimLease)
{
    private InterceptionCompletion completion;
    private bool retiring;

    internal ulong SlotId { get; } = slotId;

    internal LivePatchSlot Dispatch { get; } = dispatch;

    internal IInterceptionPatchHandle NativePatch { get; } = nativePatch;

    internal InterceptionClaimLease ClaimLease { get; } = claimLease;

    internal bool Finished { get; private set; }

    internal InterceptionCompletion Completion => completion;

    internal void RefreshClaimSelector() =>
        ClaimLease.UpdateSelector(Dispatch.SelectorDescription);

    internal void BeginRetire()
    {
        if (retiring)
            return;
        retiring = true;
        LivePatchRuntime.Detach(SlotId, Dispatch);
        if (completion.State == InterceptionState.Active)
            _ = NativePatch.Remove();
    }

    internal bool Pump()
    {
        var current = NativePatch.GetCompletion();
        var changed = current != completion;
        completion = current;
        if (current.State == InterceptionState.Failed)
        {
            LivePatchRuntime.Detach(SlotId, Dispatch);
            Finished = true;
            ReleaseClaim();
        }
        else if (retiring &&
            current.State == InterceptionState.Active &&
            current.Operation is InterceptionOperation.Install or InterceptionOperation.Replace)
        {
            _ = NativePatch.Remove();
            changed = true;
        }
        else if (current.State == InterceptionState.Removed)
        {
            Finished = true;
            ReleaseClaim();
        }

        return changed;
    }

    internal void ReleaseClaim() => ClaimLease.Dispose();
}
