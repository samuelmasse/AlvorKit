namespace AlvorKit;

[Bench]
public class Plane3Benchmarks(Plane3Measurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Plane3",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Normalize",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.NormalizeAlvor), [new("Numerics", measurements.NormalizeNumerics)])),
            BenchNode.Compare(
                "Dot",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.DotAlvor), [new("Numerics", measurements.DotNumerics)])),
            BenchNode.Compare(
                "Evaluate",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.EvaluateAlvor), [new("Numerics", measurements.EvaluateNumerics)])),
            BenchNode.Compare(
                "DotNormal",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.DotNormalAlvor), [new("Numerics", measurements.DotNormalNumerics)])),
            BenchNode.Compare(
                "TransformQuaternion",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.TransformQuaternionAlvor),
                    [new("Numerics", measurements.TransformQuaternionNumerics)])),
            BenchNode.Compare(
                "TransformMatrix",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.TransformMatrixAlvor),
                    [new("Numerics", measurements.TransformMatrixNumerics)])),
            BenchNode.Compare(
                "CreateFromPoints",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.CreateFromPointsAlvor),
                    [new("Numerics", measurements.CreateFromPointsNumerics)])),
            BenchNode.Compare(
                "Equals",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.EqualsAlvor), [new("Numerics", measurements.EqualsNumerics)])),
            BenchNode.Compare(
                "GetHashCode",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.GetHashCodeAlvor),
                    [new("Numerics", measurements.GetHashCodeNumerics)]))
    ]);
}
