namespace AlvorKit;

[Bench]
public class ArchRowsBenchmarks(ArchRowsMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Rows",
        "JIT call shape",
        [
            BenchNode.Measure(
                "GetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotRowGetRotating),
            BenchNode.Measure(
                "SetRotating",
                "1,048,576 calls; 1,024 Ents; four arenas; sixteen signatures",
                measurements.HotRowSetRotating)
    ]);
}
