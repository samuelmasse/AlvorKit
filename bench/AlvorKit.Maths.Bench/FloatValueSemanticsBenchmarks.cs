namespace AlvorKit;

[Bench]
public class FloatValueSemanticsBenchmarks(FloatValueSemanticsMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "FloatValueSemantics",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Vec2Equals",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorVec2Equals", measurements.Vec2EqualsAlvorVec2Equals),
                    [new("NumericsVec2Equals", measurements.Vec2EqualsNumericsVec2Equals)])),
            BenchNode.Compare(
                "Vec2Hash",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorVec2Hash", measurements.Vec2HashAlvorVec2Hash),
                    [new("NumericsVec2Hash", measurements.Vec2HashNumericsVec2Hash)])),
            BenchNode.Compare(
                "Vec3Equals",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorVec3Equals", measurements.Vec3EqualsAlvorVec3Equals),
                    [new("NumericsVec3Equals", measurements.Vec3EqualsNumericsVec3Equals)])),
            BenchNode.Compare(
                "Vec3Hash",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorVec3Hash", measurements.Vec3HashAlvorVec3Hash),
                    [new("NumericsVec3Hash", measurements.Vec3HashNumericsVec3Hash)])),
            BenchNode.Compare(
                "Vec4Equals",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorVec4Equals", measurements.Vec4EqualsAlvorVec4Equals),
                    [new("NumericsVec4Equals", measurements.Vec4EqualsNumericsVec4Equals)])),
            BenchNode.Compare(
                "Vec4Hash",
                "4,096 inputs; 256 passes",
                new(
                    new("AlvorVec4Hash", measurements.Vec4HashAlvorVec4Hash),
                    [new("NumericsVec4Hash", measurements.Vec4HashNumericsVec4Hash)]))
    ]);
}
