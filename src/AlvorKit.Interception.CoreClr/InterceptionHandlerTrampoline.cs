namespace AlvorKit.Interception;

/// <summary>
/// Stable exact managed entry point whose submitted handler reference is released
/// after deactivation and the final acquired invocation.
/// </summary>
internal sealed class InterceptionHandlerTrampoline :
    IInterceptionHandlerTrampoline
{
    private readonly InterceptionTrampolineState state;
    private readonly nint entryPoint;
    private Delegate? handler;
    private int disposed;

    internal InterceptionHandlerTrampoline(
        nint entryPoint,
        Delegate handler,
        InterceptionTrampolineState state)
    {
        this.entryPoint = entryPoint;
        this.handler = handler;
        this.state = state;
    }

    /// <summary>Gets the first contained submitted-handler exception, if any.</summary>
    public Exception? Failure => state.Failure;

    internal Exception? ConsumeFailure() => state.ConsumeFailure();

    Exception? IInterceptionHandlerTrampoline.ConsumeFailure() =>
        ConsumeFailure();

    /// <summary>Reserves an invocation while this handler is active.</summary>
    public bool TryAcquire(out nint entryPoint)
    {
        if (Volatile.Read(ref disposed) == 0 && state.TryAcquire())
        {
            entryPoint = this.entryPoint;
            return true;
        }

        entryPoint = 0;
        return false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;

        state.Deactivate();
        handler = null;
    }
}
