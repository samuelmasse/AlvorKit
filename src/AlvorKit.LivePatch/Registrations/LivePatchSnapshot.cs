namespace AlvorKit.LivePatch;

/// <summary>Structured cold-path evidence for one scoped handler registration.</summary>
public sealed record LivePatchSnapshot(
    ulong PatchId,
    string Name,
    InterceptionTarget Target,
    string Selector,
    LivePatchState State,
    ulong NativePatchId,
    ulong NativeRequestId,
    InterceptionOperation NativeOperation,
    InterceptionState NativeState,
    TimeSpan RejitElapsed,
    int HResult,
    string? Failure);
