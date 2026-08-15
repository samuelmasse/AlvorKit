namespace AlvorKit;

/// <summary>Controls exact object counting and bounded stack sampling for one allocation capture.</summary>
public record InterceptionAllocationCaptureOptions
{
    /// <summary>Gets the number of allocations represented by each retained stack sample.</summary>
    public uint SampleInterval { get; init; } = 1024;

    /// <summary>Gets the maximum number of stack samples retained in preallocated native memory.</summary>
    public uint MaximumSamples { get; init; } = 16_384;

    /// <summary>Gets the maximum number of managed frames retained for each sample.</summary>
    public uint MaximumFramesPerSample { get; init; } = 64;

    /// <summary>Rejects settings that exceed the fixed native capture bounds.</summary>
    internal void Validate()
    {
        if (SampleInterval == 0)
            throw new ArgumentOutOfRangeException(nameof(SampleInterval));
        if (MaximumSamples > 65_536)
            throw new ArgumentOutOfRangeException(nameof(MaximumSamples));
        if (MaximumFramesPerSample > 128)
            throw new ArgumentOutOfRangeException(
                nameof(MaximumFramesPerSample));
        if (MaximumSamples != 0 && MaximumFramesPerSample == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumFramesPerSample));
        }
    }
}
