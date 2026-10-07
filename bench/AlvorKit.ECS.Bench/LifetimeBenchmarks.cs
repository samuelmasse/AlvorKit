namespace AlvorKit;

[Bench]
public class LifetimeBenchmarks(LifetimeMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Lifetime",
        "1,048,576 complete operations",
        [
            BenchNode.Measure(
                "ComponentSetExisting",
                "prepared sparse component; setup and teardown excluded",
                measurements.ComponentSetExisting),
            BenchNode.Measure(
                "ComponentGetExisting",
                "prepared sparse component; setup and teardown excluded",
                measurements.ComponentGetExisting),
            BenchNode.Measure(
                "ComponentHasExisting",
                "prepared sparse component; setup and teardown excluded",
                measurements.ComponentHasExisting),
            BenchNode.Measure(
                "ComponentUnsetSet",
                "prepared sparse component; setup and teardown excluded",
                measurements.ComponentUnsetSet),
            BenchNode.Measure(
                "EntPtrAllocSetDispose",
                "allocation, writes, and disposal included",
                measurements.EntPtrAllocSetDispose),
            BenchNode.Measure(
                "ArenaAllocSetDispose",
                "allocation, writes, and disposal included",
                measurements.ArenaAllocSetDispose),
            BenchNode.Measure(
                "ArenaAllocSetBulkDispose",
                "allocation, writes, and disposal included",
                measurements.ArenaAllocSetBulkDispose)
    ]);
}
