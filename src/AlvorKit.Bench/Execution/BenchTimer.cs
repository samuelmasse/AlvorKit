namespace AlvorKit;

/// <summary>Captures time and current-thread managed allocation boundaries without allocating.</summary>
public readonly struct BenchTimer
{
    /// <summary>Stopwatch timestamp immediately preceding the workload.</summary>
    private readonly long started;
    /// <summary>Current-thread allocated bytes at the start boundary.</summary>
    private readonly long allocatedBefore;

    /// <summary>Elapsed workload time at the instant of access.</summary>
    public TimeSpan Elapsed => Stopwatch.GetElapsedTime(started);

    /// <summary>Stores a matched timestamp and current-thread allocation baseline.</summary>
    private BenchTimer(long started, long allocatedBefore)
    {
        this.started = started;
        this.allocatedBefore = allocatedBefore;
    }

    /// <summary>Captures counters immediately before the workload begins.</summary>
    public static BenchTimer Start()
    {
        var allocated = GC.GetAllocatedBytesForCurrentThread();
        return new(Stopwatch.GetTimestamp(), allocated);
    }

    /// <summary>Stops timing first, then captures managed bytes allocated on this thread during the workload.</summary>
    public BenchResult Stop(int operationCount, string unit)
    {
        var elapsed = Elapsed;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        return new(elapsed, operationCount, unit, "N0") { WorkloadAllocatedBytes = allocated };
    }
}
