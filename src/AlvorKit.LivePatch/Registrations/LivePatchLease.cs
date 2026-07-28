namespace AlvorKit.LivePatch;

/// <summary>Atomic ownership lease for one scope/instance/global handler registration.</summary>
public sealed class LivePatchLease : IDisposable
{
    private readonly LivePatchSession session;
    private int disposed;

    internal LivePatchLease(
        LivePatchSession session,
        ulong patchId)
    {
        this.session = session;
        PatchId = patchId;
    }

    /// <summary>Gets the stable managed patch ID used for status, replacement, and removal.</summary>
    public ulong PatchId { get; }

    /// <summary>Reads the current structured patch state.</summary>
    public LivePatchSnapshot Snapshot() => session.Get(PatchId);

    /// <summary>Atomically publishes a new exact handler while preserving this selector and patch ID.</summary>
    public void Replace(object? handlerInstance, MethodInfo handlerMethod)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref disposed) != 0,
            this);
        session.Replace(PatchId, handlerInstance, handlerMethod);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
            session.Remove(PatchId);
    }
}
