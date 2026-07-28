namespace AlvorKit.LivePatch;

internal sealed class LivePatchRegistration(
    ulong patchId,
    string name,
    InterceptionTarget target,
    LivePatchSelector selector,
    LivePatchMethodSlot method)
{
    internal ulong PatchId { get; } = patchId;

    internal string Name { get; } = name;

    internal InterceptionTarget Target { get; } = target;

    internal LivePatchSelector Selector { get; } = selector;

    internal LivePatchMethodSlot Method { get; } = method;

    internal LivePatchState State { get; set; } = LivePatchState.Installing;

    internal string? Failure { get; set; }
}
