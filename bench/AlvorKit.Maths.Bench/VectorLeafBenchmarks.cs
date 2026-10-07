namespace AlvorKit;

[Bench]
public class VectorLeafBenchmarks(VectorLeafMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "VectorLeaf",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Abs",
                "4,096 inputs; 256 passes",
                new(new("AlvorAbs", measurements.AbsAlvorAbs), [new("NumericsAbs", measurements.AbsNumericsAbs)])),
            BenchNode.Compare(
                "Clamp",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorClamp", measurements.ClampAlvorClamp),
                    [new("NumericsClamp", measurements.ClampNumericsClamp)]))
    ]);
}
