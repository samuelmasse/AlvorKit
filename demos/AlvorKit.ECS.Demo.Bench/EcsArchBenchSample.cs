namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Stores one isolated raw AFR-02 worker sample.</summary>
public sealed record EcsArchBenchSample(
    string ScenarioId,
    int SampleIndex,
    string Unit,
    long Operations,
    long ElapsedTicks,
    long AllocatedBytes,
    long RetainedManagedBytesDelta,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    EcsArchFootprint Footprint);
