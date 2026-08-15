namespace AlvorKit;

/// <summary>Exact allocation totals plus bounded sampled stacks from one completed capture.</summary>
public class InterceptionAllocationCaptureResult
{
    /// <summary>Retained allocation samples in capture order.</summary>
    private readonly InterceptionAllocationSample[] samples;
    /// <summary>Exact object count for the complete capture window.</summary>
    private readonly ulong totalObjectAllocations;
    /// <summary>Distance between scheduled retained stack samples.</summary>
    private readonly uint sampleInterval;
    /// <summary>Scheduled samples omitted after native storage filled.</summary>
    private readonly ulong droppedSamples;
    /// <summary>Scheduled samples whose CoreCLR stack walk failed.</summary>
    private readonly ulong failedStackWalks;
    /// <summary>Retained frames that could not be resolved to metadata.</summary>
    private readonly uint unresolvedFrames;
    /// <summary>First failed native frame-resolution HRESULT, when present.</summary>
    private readonly int? firstFrameResolutionHResult;

    /// <summary>Gets the exact number of individual managed objects allocated inside the capture window.</summary>
    public ulong TotalObjectAllocations => totalObjectAllocations;

    /// <summary>Gets the configured distance between retained stack samples.</summary>
    public uint SampleInterval => sampleInterval;

    /// <summary>Gets the number of due samples omitted because the native sample buffer filled.</summary>
    public ulong DroppedSamples => droppedSamples;

    /// <summary>Gets the number of samples for which CoreCLR could not walk the managed stack.</summary>
    public ulong FailedStackWalks => failedStackWalks;

    /// <summary>Gets the number of stack frames that could not be mapped to metadata.</summary>
    public uint UnresolvedFrames => unresolvedFrames;

    /// <summary>Gets the first native HRESULT observed while resolving a sampled frame.</summary>
    public int? FirstFrameResolutionHResult =>
        firstFrameResolutionHResult;

    /// <summary>Gets the retained allocation samples.</summary>
    public IReadOnlyList<InterceptionAllocationSample> Samples => samples;

    /// <summary>Creates an immutable completed allocation capture.</summary>
    internal InterceptionAllocationCaptureResult(
        ulong totalObjectAllocations,
        uint sampleInterval,
        ulong droppedSamples,
        ulong failedStackWalks,
        uint unresolvedFrames,
        int? firstFrameResolutionHResult,
        InterceptionAllocationSample[] samples)
    {
        this.totalObjectAllocations = totalObjectAllocations;
        this.sampleInterval = sampleInterval;
        this.droppedSamples = droppedSamples;
        this.failedStackWalks = failedStackWalks;
        this.unresolvedFrames = unresolvedFrames;
        this.firstFrameResolutionHResult = firstFrameResolutionHResult;
        this.samples = samples;
    }

    /// <summary>Maps retained frames from selected assemblies to methods and Portable PDB source lines.</summary>
    public InterceptionAllocationSourceReport ResolveSources(
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return InterceptionAllocationSourceResolver.Resolve(this, assemblies);
    }
}
