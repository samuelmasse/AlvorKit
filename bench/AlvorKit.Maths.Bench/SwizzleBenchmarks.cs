namespace AlvorKit;

[Bench]
public class SwizzleBenchmarks(SwizzleMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Swizzle",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Vec3.Xzy",
                "4,096 inputs; 256 passes",
                new(
                    new("CurrentVec3Xzy", measurements.Vec3XzyCurrentVec3Xzy),
                    [new("IntrinsicVec3Xzy", measurements.Vec3XzyIntrinsicVec3Xzy)])),
            BenchNode.Compare(
                "Vec3i.Xzy",
                "4,096 inputs; 256 passes",
                new(
                    new("CurrentVec3iXzy", measurements.Vec3iXzyCurrentVec3iXzy),
                    [new("IntrinsicVec3iXzy", measurements.Vec3iXzyIntrinsicVec3iXzy)])),
            BenchNode.Compare(
                "Vec4.Wzyx",
                "4,096 inputs; 256 passes",
                new(
                    new("CurrentVec4Reverse", measurements.Vec4WzyxCurrentVec4Reverse),
                    [new("IntrinsicVec4Reverse", measurements.Vec4WzyxIntrinsicVec4Reverse)])),
            BenchNode.Compare(
                "Vec4i.Wzyx",
                "4,096 inputs; 256 passes",
                new(
                    new("CurrentVec4iReverse", measurements.Vec4iWzyxCurrentVec4iReverse),
                    [new("IntrinsicVec4iReverse", measurements.Vec4iWzyxIntrinsicVec4iReverse)]))
    ]);
}
