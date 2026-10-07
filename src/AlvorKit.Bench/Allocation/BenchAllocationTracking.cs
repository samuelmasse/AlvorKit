namespace AlvorKit;

/// <summary>Connects to an explicitly enabled startup profiler; byte accounting works independently.</summary>
[Bench]
public class BenchAllocationTracking
{
    /// <summary>Optional native profiler attached when the process was started with allocation tracking.</summary>
    private readonly InterceptionProfiler? profiler;
    /// <summary>Counts every allocated object without retaining sample stacks.</summary>
    private readonly InterceptionAllocationCaptureOptions options = new()
    {
        SampleInterval = 1,
        MaximumSamples = 0,
        MaximumFramesPerSample = 0,
    };

    /// <summary>Whether object-count capture is available for this invocation.</summary>
    public bool Enabled => profiler is not null;

    /// <summary>Records the explicitly selected profiler mode.</summary>
    private BenchAllocationTracking(InterceptionProfiler? profiler) => this.profiler = profiler;

    /// <summary>Connects only when the profiler startup environment explicitly enables allocation tracking.</summary>
    public static BenchAllocationTracking Connect()
    {
        var enabled = Environment.GetEnvironmentVariable(
            InterceptionProfiler.AllocationProfilingEnvironmentVariable) == "1";
        return new(enabled ? InterceptionProfiler.Connect() : null);
    }

    /// <summary>Starts an object-count capture when profiling is enabled.</summary>
    public BenchAllocationCapture? Begin() =>
        profiler is null ? null : new(profiler.BeginAllocationCapture(options));
}

/// <summary>Owns one native-profiler allocation capture until disposed.</summary>
[ExcludeFromCodeCoverage(Justification = "Requires the native CLR allocation profiler loaded at process startup.")]
public class BenchAllocationCapture(InterceptionAllocationCapture capture) : IDisposable
{
    /// <summary>Finishes capture and returns the complete object allocation count.</summary>
    public ulong Complete() => capture.Complete().TotalObjectAllocations;

    /// <summary>Releases the native profiler capture.</summary>
    public void Dispose() => capture.Dispose();
}
