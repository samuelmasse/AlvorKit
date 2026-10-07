namespace AlvorKit;

[Bench]
public class ArchStructuralBenchmarks
{
    public BenchNode Create() => BenchNode.Group(
        "Structural",
        "transition and catalog operations",
        [
            BenchNode.Measure(
                "AddCached",
                "4,096 pre-sized rows",
                () => ArchStructuralMeasurements.AddCached<RunArch>(4096)),
            BenchNode.Measure(
                "AddGrowth",
                "fresh process; JIT prewarm in a separate group; catalog remains cold",
                () => ArchIsolatedMeasurement.Run("AddGrowth")),
            BenchNode.Measure(
                "AddUnknown",
                "fresh process; JIT prewarm in a separate group; catalog remains cold",
                () => ArchIsolatedMeasurement.Run("AddUnknown")),
            BenchNode.Measure(
                "RemoveCached",
                "4,096 pre-sized rows",
                () => ArchStructuralMeasurements.RemoveCached<RunArch>(4096)),
            BenchNode.Measure(
                "RemoveUnknown",
                "fresh process; JIT prewarm in a separate group; catalog remains cold",
                () => ArchIsolatedMeasurement.Run("RemoveUnknown")),
            BenchNode.Measure(
                "CompactFirst",
                "4,096 pre-sized rows",
                () => ArchStructuralMeasurements.Compaction<RunArch>(4096, CompactionPosition.First)),
            BenchNode.Measure(
                "CompactMiddle",
                "4,096 pre-sized rows",
                () => ArchStructuralMeasurements.Compaction<RunArch>(4096, CompactionPosition.Middle)),
            BenchNode.Measure(
                "CompactLast",
                "4,096 pre-sized rows",
                () => ArchStructuralMeasurements.Compaction<RunArch>(4096, CompactionPosition.Last)),
            BenchNode.Measure(
                "UniqueSignature",
                "fresh process; JIT prewarm in a separate group; catalog remains cold",
                () => ArchIsolatedMeasurement.Run("UniqueSignature")),
            BenchNode.Measure(
                "LowOccupancy",
                "fresh process; JIT prewarm in a separate group; catalog remains cold",
                () => ArchIsolatedMeasurement.Run("LowOccupancy")),
            BenchNode.Measure(
                "HighOccupancy",
                "fresh process; JIT prewarm in a separate group; catalog remains cold",
                () => ArchIsolatedMeasurement.Run("HighOccupancy")),
            BenchNode.Group(
                "Transition",
                "cached graph edge lookup",
                [
                    ..new[] { 1, 4, 8, 16, 32 }.Select(
                        width => BenchNode.Measure(
                            $"Width{width}",
                            "1,048,576 lookups",
                            () => ArchStructuralMeasurements.TransitionLookup<RunArch>(width)))
            ])
    ]);
}
