namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Owns one active allocation-counting window in the current profiled process.</summary>
public class InterceptionAllocationCapture : IDisposable
{
    /// <summary>Connected profiler that owns the native capture window.</summary>
    private readonly InterceptionProfiler profiler;
    /// <summary>Zero while active and one after completion or disposal.</summary>
    private int state;

    /// <summary>Creates a handle for one already-started profiler capture.</summary>
    internal InterceptionAllocationCapture(InterceptionProfiler profiler)
    {
        this.profiler = profiler;
    }

    /// <summary>Ends the exact capture window, resolves its sampled stacks, and returns the result.</summary>
    public InterceptionAllocationCaptureResult Complete()
    {
        if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
        {
            throw new InvalidOperationException(
                "This allocation capture has already ended.");
        }

        return profiler.CompleteAllocationCapture();
    }

    /// <summary>Ends an unfinished capture while discarding its result.</summary>
    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref state, 1, 0) == 0)
            profiler.DiscardAllocationCapture();
    }
}
