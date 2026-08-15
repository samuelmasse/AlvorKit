namespace AlvorKit;

/// <summary>Stores one raw elapsed-time and allocation sample.</summary>
internal readonly record struct MockPerformanceSample(
    long ElapsedTicks,
    long? AllocatedBytes);
