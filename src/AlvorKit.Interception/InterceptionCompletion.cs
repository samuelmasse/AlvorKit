namespace AlvorKit;

/// <summary>Structured completion state for one native profiler request.</summary>
public readonly record struct InterceptionCompletion(
    ulong RequestId,
    ulong PatchId,
    InterceptionOperation Operation,
    InterceptionState State,
    int HResult,
    InterceptionPatchFlags PatchFlags,
    InterceptionTarget Target,
    uint RejitStartedCallbacks,
    uint ParameterCallbacks,
    uint RejitFinishedCallbacks,
    uint RejitErrorCallbacks,
    TimeSpan Elapsed)
{
    /// <summary>Whether the request reached a terminal state.</summary>
    public bool IsTerminal =>
        State is InterceptionState.Active or
            InterceptionState.Removed or
            InterceptionState.Failed;

    /// <summary>Throws the native HRESULT when this completion represents failure.</summary>
    public void ThrowIfFailed()
    {
        if (State == InterceptionState.Failed)
            Marshal.ThrowExceptionForHR(HResult);
    }
}
