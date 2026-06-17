namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Stores one measured ECS benchmark result.</summary>
public sealed record EcsBenchResult(
    string Name,
    int Operations,
    double BestNanosecondsPerOperation,
    double MeanNanosecondsPerOperation,
    double AllocatedBytesPerOperation);
