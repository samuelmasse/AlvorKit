namespace AlvorKit;

/// <summary>Maps retained profiler frames through selected assemblies and their Portable PDBs.</summary>
internal static class InterceptionAllocationSourceResolver
{
    /// <summary>Resolves one completed capture against the supplied assembly set.</summary>
    internal static InterceptionAllocationSourceReport Resolve(
        InterceptionAllocationCaptureResult capture,
        IReadOnlyList<Assembly> assemblies)
    {
        var modules = new Dictionary<Guid, InterceptionAllocationModuleSymbols>();
        try
        {
            foreach (var assembly in assemblies.Distinct())
            {
                foreach (var module in assembly.GetModules())
                {
                    modules.TryAdd(
                        module.ModuleVersionId,
                        new InterceptionAllocationModuleSymbols(module));
                }
            }

            var sourceSamples =
                new List<InterceptionAllocationSourceSample>(
                    capture.Samples.Count);
            foreach (var sample in capture.Samples)
            {
                var frames =
                    new List<InterceptionAllocationSourceFrame>(
                        sample.Frames.Count);
                foreach (var frame in sample.Frames)
                {
                    if (modules.TryGetValue(
                            frame.ModuleMvid,
                            out var symbols))
                    {
                        frames.Add(symbols.Resolve(frame));
                    }
                }

                if (frames.Count == 0)
                    continue;
                frames.Reverse();
                sourceSamples.Add(
                    new(
                        SampleWeight(capture, sample),
                        [.. frames]));
            }

            return new(
                capture.TotalObjectAllocations,
                capture.SampleInterval,
                capture.DroppedSamples,
                capture.FailedStackWalks,
                [.. sourceSamples]);
        }
        finally
        {
            foreach (var symbols in modules.Values)
                symbols.Dispose();
        }
    }

    /// <summary>Computes how many exact capture ordinals one retained sample represents.</summary>
    private static ulong SampleWeight(
        InterceptionAllocationCaptureResult capture,
        InterceptionAllocationSample sample)
    {
        if (sample.AllocationOrdinal > capture.TotalObjectAllocations)
            return 0;
        return Math.Min(
            capture.SampleInterval,
            capture.TotalObjectAllocations - sample.AllocationOrdinal + 1);
    }
}
