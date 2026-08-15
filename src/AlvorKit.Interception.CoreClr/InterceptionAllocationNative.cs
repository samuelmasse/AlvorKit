namespace AlvorKit;

/// <summary>Adapts the generated profiler ABI to completed managed allocation captures.</summary>
/// <param name="api">Generated ABI surface connected to the already-loaded profiler.</param>
internal unsafe class InterceptionAllocationNative(InterceptionProfilerApi api)
{
    /// <summary>Starts one native capture with preallocated sample and frame storage.</summary>
    internal void Begin(InterceptionAllocationCaptureOptions options)
    {
        InterceptionProfilerAllocationCapture request = new()
        {
            Size = (uint)sizeof(InterceptionProfilerAllocationCapture),
            AbiVersion = InterceptionProfiler.NativeAbiVersion,
            SampleInterval = options.SampleInterval,
            MaximumSamples = options.MaximumSamples,
            MaximumFramesPerSample = options.MaximumFramesPerSample
        };
        Marshal.ThrowExceptionForHR(api.BeginAllocationCapture(in request));
    }

    /// <summary>Ends the active native capture and resolves every retained raw frame.</summary>
    internal InterceptionAllocationCaptureResult End()
    {
        Marshal.ThrowExceptionForHR(api.EndAllocationCapture(out var summary));

        var samples = new InterceptionAllocationSample[(int)summary.SampledObjectAllocations];
        var frameCapacity = (int)summary.MaximumFramesPerSample;
        InterceptionProfilerAllocationFrame* nativeFrames =
            stackalloc InterceptionProfilerAllocationFrame[frameCapacity];
        var unresolvedFrameCount = 0u;
        int? firstFrameResolutionHResult = null;
        for (var sampleIndex = 0; sampleIndex < samples.Length; ++sampleIndex)
        {
            Marshal.ThrowExceptionForHR(
                api.GetAllocationSample((uint)sampleIndex, out var sample, nativeFrames, (uint)frameCapacity));

            var resolvedFrames = new InterceptionAllocationStackFrame[sample.FrameCount];
            var resolvedCount = 0;
            for (var frameIndex = 0; frameIndex < sample.FrameCount; ++frameIndex)
            {
                var status = api.ResolveAllocationFrame(in nativeFrames[frameIndex], out var resolved);
                if (status < 0)
                {
                    ++unresolvedFrameCount;
                    firstFrameResolutionHResult ??= status;
                    continue;
                }

                resolvedFrames[resolvedCount++] = new(
                    InterceptionProfiler.FromNative(resolved.ModuleMvid),
                    resolved.MethodToken,
                    resolved.HasIlOffset != 0 ? (int)resolved.IlOffset : null);
            }

            if (resolvedCount != resolvedFrames.Length)
                resolvedFrames = resolvedFrames.AsSpan(0, resolvedCount).ToArray();
            samples[sampleIndex] = new(
                sample.AllocationOrdinal,
                sample.ClassId,
                sample.StackHresult,
                resolvedFrames);
        }

        return new(
            summary.TotalObjectAllocations,
            summary.SampleInterval,
            summary.DroppedSamples,
            summary.FailedStackWalks,
            unresolvedFrameCount,
            firstFrameResolutionHResult,
            samples);
    }

    /// <summary>Ends the active native capture without retaining its summary or samples.</summary>
    internal void EndAndDiscard() =>
        Marshal.ThrowExceptionForHR(api.EndAllocationCapture(out _));
}
