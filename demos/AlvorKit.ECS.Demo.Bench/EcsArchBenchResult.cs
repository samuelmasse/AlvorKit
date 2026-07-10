namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Stores aggregate timing and raw samples for one AFR-02 scenario.</summary>
public sealed record EcsArchBenchResult(
    string ScenarioId,
    string Category,
    string Unit,
    int Width,
    long Operations,
    double BestNanosecondsPerOperation,
    double MedianNanosecondsPerOperation,
    double MeanNanosecondsPerOperation,
    double MeanAllocatedBytesPerOperation,
    EcsArchFootprint Footprint,
    EcsArchBenchSample[] Samples);
