namespace AlvorKit;

[Bench]
public class ConversionBenchmarks(ConversionMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Conversion",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Vec3dToVec3",
                "4,096 inputs; 256 passes",
                new(
                    new("CastVec3dToVec3", measurements.Vec3dToVec3CastVec3dToVec3),
                    [new("ManualVec3dToVec3", measurements.Vec3dToVec3ManualVec3dToVec3)])),
            BenchNode.Compare(
                "Vec3ToVec3d",
                "4,096 inputs; 256 passes",
                new(
                    new("CastVec3ToVec3d", measurements.Vec3ToVec3dCastVec3ToVec3d),
                    [new("ManualVec3ToVec3d", measurements.Vec3ToVec3dManualVec3ToVec3d)])),
            BenchNode.Compare(
                "Vec2iToVec2u",
                "4,096 inputs; 256 passes",
                new(
                    new("CastVec2iToVec2u", measurements.Vec2iToVec2uCastVec2iToVec2u),
                    [new("ManualVec2iToVec2u", measurements.Vec2iToVec2uManualVec2iToVec2u)]))
    ]);
}
