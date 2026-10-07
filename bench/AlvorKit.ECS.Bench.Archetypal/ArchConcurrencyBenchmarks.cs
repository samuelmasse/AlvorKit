namespace AlvorKit;

[Bench]
public class ArchConcurrencyBenchmarks
{
    public BenchNode Create() => BenchNode.Group(
        "Concurrency",
        "independent arena owners",
        [
            BenchNode.Measure(
                "Get1Owners",
                "one arena per owner; 1,048,576 calls per thread",
                () => ArchConcurrencyMeasurements.Get<RunArch>(1048576, 1)),
            BenchNode.Measure(
                "Get4Owners",
                "one arena per owner; 1,048,576 calls per thread",
                () => ArchConcurrencyMeasurements.Get<RunArch>(1048576, 4)),
            BenchNode.Measure(
                "Set1Owners",
                "one arena per owner; 1,048,576 calls per thread",
                () => ArchConcurrencyMeasurements.Set<RunArch>(1048576, 1)),
            BenchNode.Measure(
                "Set4Owners",
                "one arena per owner; 1,048,576 calls per thread",
                () => ArchConcurrencyMeasurements.Set<RunArch>(1048576, 4)),
            BenchNode.Measure(
                "Resolve4Owners",
                "fresh process; 4,096 first-time transitions across four arenas",
                () => ArchIsolatedMeasurement.Run("ConcurrentResolve"))
    ]);
}
