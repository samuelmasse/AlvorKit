namespace AlvorKit;

[Bench]
public class Vector4Benchmarks(Vector4Measurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "Vector4",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Add",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.AddAlvor), [new("Numerics", measurements.AddNumerics)])),
            BenchNode.Compare(
                "Subtract",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.SubtractAlvor), [new("Numerics", measurements.SubtractNumerics)])),
            BenchNode.Compare(
                "PairMultiply",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.PairMultiplyAlvor),
                    [new("Numerics", measurements.PairMultiplyNumerics)])),
            BenchNode.Compare(
                "Scale",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.ScaleAlvor), [new("Numerics", measurements.ScaleNumerics)])),
            BenchNode.Compare(
                "Divide",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.DivideAlvor), [new("Numerics", measurements.DivideNumerics)])),
            BenchNode.Compare(
                "Lerp",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.LerpAlvor), [new("Numerics", measurements.LerpNumerics)])),
            BenchNode.Compare(
                "Bounds",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.BoundsAlvor), [new("Numerics", measurements.BoundsNumerics)])),
            BenchNode.Compare(
                "Dot",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.DotAlvor), [new("Numerics", measurements.DotNumerics)])),
            BenchNode.Compare(
                "Normalize",
                "4,096 inputs; 256 passes",
                new(new("Alvor", measurements.NormalizeAlvor), [new("Numerics", measurements.NormalizeNumerics)])),
            BenchNode.Compare(
                "ValueSemantics",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.ValueSemanticsAlvor),
                    [new("Numerics", measurements.ValueSemanticsNumerics)]))
    ]);
}
