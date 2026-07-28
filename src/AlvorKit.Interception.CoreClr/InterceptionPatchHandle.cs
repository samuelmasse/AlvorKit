namespace AlvorKit.Interception;

/// <summary>Stable ownership handle for installing, replacing, and removing one exact native patch.</summary>
internal sealed class InterceptionPatchHandle :
    IInterceptionGenerationPatchHandle
{
    private readonly InterceptionProfiler profiler;
    private long lastRequestId;
    private int removing;

    internal InterceptionPatchHandle(
        InterceptionProfiler profiler,
        ulong patchId,
        InterceptionTarget target,
        ulong installRequestId)
    {
        this.profiler = profiler;
        PatchId = patchId;
        Target = target;
        lastRequestId = checked((long)installRequestId);
    }

    /// <summary>Gets the stable patch ID retained across replacements.</summary>
    public ulong PatchId { get; }

    /// <summary>Gets the exact method owned by this handle.</summary>
    public InterceptionTarget Target { get; }

    /// <summary>Gets the most recently enqueued request ID.</summary>
    public ulong LastRequestId =>
        checked((ulong)Interlocked.Read(ref lastRequestId));

    /// <summary>Atomically requests another method body for the same patch and target.</summary>
    public ulong Replace(InterceptionPlan plan)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref removing) != 0,
            this);
        var requestId = profiler.Replace(this, plan);
        Interlocked.Exchange(ref lastRequestId, checked((long)requestId));
        return requestId;
    }

    /// <summary>Atomically requests another exact dispatch plan for the same patch and target.</summary>
    public ulong Replace(InterceptionDispatchPlan plan)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref removing) != 0,
            this);
        var requestId = profiler.Replace(this, plan);
        Interlocked.Exchange(ref lastRequestId, checked((long)requestId));
        return requestId;
    }

    /// <summary>Atomically requests another immutable generation for the same patch and target.</summary>
    public ulong Replace(InterceptionGenerationPlan plan)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref removing) != 0,
            this);
        var requestId = profiler.Replace(this, plan);
        Interlocked.Exchange(ref lastRequestId, checked((long)requestId));
        return requestId;
    }

    /// <summary>Requests restoration of original IL and prevents subsequent replacement through this handle.</summary>
    public ulong Remove()
    {
        if (Interlocked.CompareExchange(ref removing, 1, 0) != 0)
            return LastRequestId;

        try
        {
            var requestId = profiler.Remove(this);
            Interlocked.Exchange(ref lastRequestId, checked((long)requestId));
            return requestId;
        }
        catch
        {
            Volatile.Write(ref removing, 0);
            throw;
        }
    }

    /// <summary>Reads the current completion for this handle's latest request.</summary>
    public InterceptionCompletion GetCompletion() =>
        profiler.GetCompletion(LastRequestId);

    /// <summary>Waits for the current request to activate, remove, or fail.</summary>
    public InterceptionCompletion WaitFor(
        TimeSpan timeout,
        TimeSpan? pollInterval = null) =>
        profiler.WaitFor(LastRequestId, timeout, pollInterval);

    /// <inheritdoc />
    public ValueTask<InterceptionCompletion> WaitForAsync(
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default) =>
        profiler.WaitForAsync(
            LastRequestId,
            timeout,
            pollInterval,
            cancellationToken);

    /// <inheritdoc />
    public async ValueTask<InterceptionCompletion> RemoveAsync(
        TimeSpan timeout,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        var requestId = Remove();
        return await profiler.WaitForAsync(
            requestId,
            timeout,
            pollInterval,
            cancellationToken);
    }

    /// <summary>
    /// Requests removal without waiting for original IL restoration.
    /// </summary>
    public void Dispose()
    {
        if (Volatile.Read(ref removing) == 0)
            _ = Remove();
    }

}
