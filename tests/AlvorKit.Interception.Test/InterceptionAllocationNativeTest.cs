using AlvorKit.Interception.Profiler;

namespace AlvorKit.Interception.Test;

[TestClass]
public unsafe class InterceptionAllocationNativeTest
{
    /// <summary>Maps generated allocation ABI records into a completed managed capture.</summary>
    [TestMethod]
    public void End_MapsSamplesAndResolutionFailures()
    {
        var api = new AllocationApi();
        var native = new InterceptionAllocationNative(api);

        var result = native.End();

        Assert.AreEqual(11UL, result.TotalObjectAllocations);
        Assert.AreEqual(8U, result.SampleInterval);
        Assert.AreEqual(1UL, result.DroppedSamples);
        Assert.AreEqual(2UL, result.FailedStackWalks);
        Assert.AreEqual(1U, result.UnresolvedFrames);
        Assert.AreEqual(unchecked((int)0x80004005), result.FirstFrameResolutionHResult);
        Assert.HasCount(1, result.Samples);
        Assert.HasCount(1, result.Samples[0].Frames);
        Assert.AreEqual(0x0600_0001, result.Samples[0].Frames[0].MethodToken);
        Assert.AreEqual(7, result.Samples[0].Frames[0].IlOffset);
    }

    /// <summary>Generated ABI double that returns one resolved and one unresolved frame.</summary>
    private class AllocationApi : InterceptionProfilerApiNoop
    {
        /// <inheritdoc/>
        public override int EndAllocationCapture(out InterceptionProfilerAllocationSummary summary)
        {
            summary = new()
            {
                TotalObjectAllocations = 11,
                SampledObjectAllocations = 1,
                DroppedSamples = 1,
                FailedStackWalks = 2,
                SampleInterval = 8,
                MaximumFramesPerSample = 2
            };
            return 0;
        }

        /// <inheritdoc/>
        public override int GetAllocationSample(
            uint sampleIndex,
            out InterceptionProfilerAllocationSample sample,
            InterceptionProfilerAllocationFrame* frames,
            uint frameCapacity)
        {
            sample = new()
            {
                AllocationOrdinal = 1,
                ClassId = 42,
                FrameCount = 2
            };
            frames[0] = new() { FunctionId = 1, InstructionPointer = 100 };
            frames[1] = new() { FunctionId = 2, InstructionPointer = 200 };
            return 0;
        }

        /// <inheritdoc/>
        public override int ResolveAllocationFrame(
            in InterceptionProfilerAllocationFrame frame,
            out InterceptionProfilerResolvedFrame resolved)
        {
            resolved = new();
            if (frame.FunctionId == 2)
                return unchecked((int)0x80004005);

            resolved.MethodToken = 0x0600_0001;
            resolved.IlOffset = 7;
            resolved.HasIlOffset = 1;
            return 0;
        }
    }
}
