namespace AlvorKit.Interception;

/// <summary>Coordinates atomic handler removal with calls that already acquired its exact trampoline.</summary>
internal sealed class InterceptionTrampolineState(Action clearHandler)
{
    private int active = 1;
    private int inFlight;
    private int cleared;
    private Exception? failure;

    /// <summary>Gets the first submitted-handler exception, if one deactivated this trampoline.</summary>
    public Exception? Failure => Volatile.Read(ref failure);

    internal Exception? ConsumeFailure() =>
        Interlocked.Exchange(ref failure, null);

    /// <summary>Attempts to reserve one invocation before its function pointer is returned.</summary>
    internal bool TryAcquire()
    {
        if (Volatile.Read(ref active) == 0)
            return false;

        Interlocked.Increment(ref inFlight);
        if (Volatile.Read(ref active) != 0)
            return true;

        Release();
        return false;
    }

    /// <summary>Releases one invocation in a generated trampoline's finally block.</summary>
    internal void Release()
    {
        if (Interlocked.Decrement(ref inFlight) == 0 &&
            Volatile.Read(ref active) == 0)
        {
            Clear();
        }
    }

    /// <summary>
    /// Records the first submitted-handler exception and disables future dispatch.
    /// The generated exact trampoline then returns the signature's default value.
    /// </summary>
    internal void Fail(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _ = Interlocked.CompareExchange(ref failure, exception, null);
        Deactivate();
    }

    internal void Deactivate()
    {
        Volatile.Write(ref active, 0);
        if (Volatile.Read(ref inFlight) == 0)
            Clear();
    }

    private void Clear()
    {
        if (Interlocked.Exchange(ref cleared, 1) == 0)
            clearHandler();
    }
}
