var profiler = InterceptionProfiler.Connect();

const int smallEntityCount = 100;
const int largeEntityCount = 10_000;
var countOnly = new InterceptionAllocationCaptureOptions
{
    MaximumSamples = 0,
    MaximumFramesPerSample = 0
};

// Warm JIT and profiler paths before the measurements.
AllocationScenario.ObjectsPerEntity(1);
AllocationScenario.StructArraySlab(1);
CaptureObjects(profiler, 1, countOnly);
CaptureSlab(profiler, 1, countOnly);

var smallObjects = CaptureObjects(
    profiler,
    smallEntityCount,
    countOnly);
var largeObjects = CaptureObjects(
    profiler,
    largeEntityCount,
    countOnly);
var smallSlab = CaptureSlab(
    profiler,
    smallEntityCount,
    countOnly);
var largeSlab = CaptureSlab(
    profiler,
    largeEntityCount,
    countOnly);

Console.WriteLine("Exact managed object allocations");
Console.WriteLine(
    "implementation       N=100      N=10,000   growth/entity");
PrintRow(
    "class per entity",
    smallObjects,
    largeObjects,
    smallEntityCount,
    largeEntityCount);
PrintRow(
    "struct array slab",
    smallSlab,
    largeSlab,
    smallEntityCount,
    largeEntityCount);

// A bounded exact-stack run gives allocation attribution by source line.
var exactStacks = new InterceptionAllocationCaptureOptions
{
    SampleInterval = 1,
    MaximumSamples = 512,
    MaximumFramesPerSample = 64
};
using var lineCapture = profiler.BeginAllocationCapture(exactStacks);
var lineObjects = AllocationScenario.ObjectsPerEntity(128);
var lineResult = lineCapture.Complete();
GC.KeepAlive(lineObjects);

var report = lineResult.ResolveSources(
    Assembly.GetExecutingAssembly());
var chartPath = Path.GetFullPath(
    args.FirstOrDefault() ??
    Path.Combine(
        "out",
        "allocation-demo",
        "managed-allocations.speedscope.json"));
report.WriteSpeedscope(chartPath);

Console.WriteLine();
Console.WriteLine(
    $"Line capture: {lineResult.TotalObjectAllocations:N0} exact objects; " +
    $"{report.AttributedObjectAllocations:N0} attributed");
Console.WriteLine(
    $"Retained samples: {lineResult.Samples.Count:N0}; " +
    $"dropped: {lineResult.DroppedSamples:N0}; " +
    $"stack-walk failures: {lineResult.FailedStackWalks:N0}; " +
    $"unresolved frames: {lineResult.UnresolvedFrames:N0}; " +
    $"first resolve HRESULT: " +
    $"{lineResult.FirstFrameResolutionHResult?.ToString("X8") ?? "none"}");
foreach (var line in report.TopLines.Take(5))
{
    Console.WriteLine(
        $"{line.AttributedObjectAllocations,6:N0}  " +
        $"{line.Document}:{line.Line}  {line.Method}");
}
Console.WriteLine(
    $"Line attribution exact: {report.IsLineAttributionExact}");
Console.WriteLine($"Speedscope chart: {chartPath}");

// Count one class-backed implementation without retaining stack samples.
static ulong CaptureObjects(
    InterceptionProfiler profiler,
    int count,
    InterceptionAllocationCaptureOptions options)
{
    using var capture = profiler.BeginAllocationCapture(options);
    var particles = AllocationScenario.ObjectsPerEntity(count);
    var result = capture.Complete();
    GC.KeepAlive(particles);
    return result.TotalObjectAllocations;
}

// Count one contiguous managed-array implementation.
static ulong CaptureSlab(
    InterceptionProfiler profiler,
    int count,
    InterceptionAllocationCaptureOptions options)
{
    using var capture = profiler.BeginAllocationCapture(options);
    var particles = AllocationScenario.StructArraySlab(count);
    var result = capture.Complete();
    GC.KeepAlive(particles);
    return result.TotalObjectAllocations;
}

// Report the scale-dependent allocation slope so constant setup objects cancel.
static void PrintRow(
    string name,
    ulong small,
    ulong large,
    int smallCount,
    int largeCount)
{
    var growth = (double)(large - small) / (largeCount - smallCount);
    Console.WriteLine(
        $"{name,-20} {small,7:N0}      {large,7:N0}   {growth,8:N3}");
}

/// <summary>Allocation shapes compared by the managed profiler walkthrough.</summary>
internal static class AllocationScenario
{
    /// <summary>Allocates one reference object per entity plus one constant-size reference array.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static DemoParticle[] ObjectsPerEntity(int count)
    {
        var particles = new DemoParticle[count];
        for (var index = 0; index < particles.Length; ++index)
            particles[index] = new(index, index * 0.5f);
        return particles;
    }

    /// <summary>Allocates one reference array whose elements are inline value types.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static DemoParticleValue[] StructArraySlab(int count)
    {
        var particles = new DemoParticleValue[count];
        for (var index = 0; index < particles.Length; ++index)
            particles[index] = new(index, index * 0.5f);
        return particles;
    }
}

/// <summary>Reference-backed demo entity that allocates once per instance.</summary>
internal record DemoParticle(int Id, float Position);

/// <summary>Inline demo entity stored directly inside one managed array.</summary>
internal readonly record struct DemoParticleValue(
    int Id,
    float Position);
