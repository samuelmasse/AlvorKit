namespace AlvorKit;

[Bench]
public class EqualityComparerBenchmarks(EqualityComparerMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "EqualityComparer",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Vec2i",
                "4,096 inputs; 256 passes",
                new(
                    new("DirectVec2i", measurements.Vec2iDirectVec2i),
                    [new("ComparerVec2i", measurements.Vec2iComparerVec2i)])),
            BenchNode.Compare(
                "Vec3i",
                "4,096 inputs; 256 passes",
                new(
                    new("DirectVec3i", measurements.Vec3iDirectVec3i),
                    [new("ComparerVec3i", measurements.Vec3iComparerVec3i)]))
    ]);
}
