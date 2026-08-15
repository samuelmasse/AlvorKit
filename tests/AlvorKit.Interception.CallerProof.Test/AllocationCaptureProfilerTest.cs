namespace AlvorKit;

[TestClass]
public class AllocationCaptureProfilerTest
{
    /// <summary>Counts every object without reserving or dropping stack samples.</summary>
    [TestMethod]
    public void CountOnlyCaptureSkipsSampleBookkeeping()
    {
        var profiler = RequireAllocationProfiler();
        var options = new InterceptionAllocationCaptureOptions
        {
            SampleInterval = 1,
            MaximumSamples = 0,
            MaximumFramesPerSample = 0
        };
        Capture(profiler, 1, options);

        var result = Capture(profiler, 32, options);

        Assert.AreEqual(33UL, result.TotalObjectAllocations);
        Assert.HasCount(0, result.Samples);
        Assert.AreEqual(0UL, result.DroppedSamples);
        Assert.AreEqual(0UL, result.FailedStackWalks);
    }

    /// <summary>Counts every array and class allocation and resolves each retained stack to this source file.</summary>
    [TestMethod]
    public void ExactCaptureCountsAndAttributesManagedObjects()
    {
        var profiler = RequireAllocationProfiler();
        var options = new InterceptionAllocationCaptureOptions
        {
            SampleInterval = 1,
            MaximumSamples = 128,
            MaximumFramesPerSample = 32
        };
        Capture(profiler, 1, options);

        var result = Capture(profiler, 32, options);
        var report = result.ResolveSources(
            Assembly.GetExecutingAssembly());

        Assert.AreEqual(33UL, result.TotalObjectAllocations);
        Assert.AreEqual(33UL, report.AttributedObjectAllocations);
        Assert.AreEqual(0UL, result.DroppedSamples);
        Assert.AreEqual(0UL, result.FailedStackWalks);
        Assert.AreEqual(0U, result.UnresolvedFrames);
        Assert.IsTrue(report.IsLineAttributionExact);
        Assert.IsTrue(
            report.TopLines.Any(static line =>
                line.AttributedObjectAllocations == 32));
        Assert.IsTrue(
            report.TopLines.Any(static line =>
                line.AttributedObjectAllocations == 1));
    }

    private static InterceptionAllocationCaptureResult Capture(
        InterceptionProfiler profiler,
        int count,
        InterceptionAllocationCaptureOptions options)
    {
        using var capture = profiler.BeginAllocationCapture(options);
        var objects = AllocateObjects(count);
        var result = capture.Complete();
        GC.KeepAlive(objects);
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ProofObject[] AllocateObjects(int count)
    {
        var objects = new ProofObject[count];
        for (var index = 0; index < objects.Length; ++index)
            objects[index] = new(index);
        return objects;
    }

    private static InterceptionProfiler RequireAllocationProfiler()
    {
        if (Environment.GetEnvironmentVariable(
                InterceptionProfiler.PathEnvironmentVariable) is null)
        {
            Assert.Inconclusive(
                "This proof requires the isolated interception-profiler launcher.");
        }

        var profiler = InterceptionProfiler.Connect();
        if (!profiler.Capabilities.Flags.HasFlag(
                InterceptionCapability.AllocationCapture))
        {
            Assert.Inconclusive(
                "This proof requires --allocation-profiling at process startup.");
        }
        return profiler;
    }

    private record ProofObject(int Value);
}
