namespace AlvorKit;

[Bench]
public class SystemTypeParityBenchmarks(SystemTypeParityMeasurements measurements)
{
    public BenchNode Create() => BenchNode.Group(
        "SystemTypeParity",
        "contiguous input buffers; setup excluded",
        [
            BenchNode.Compare(
                "Matrix3x2Add",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix3x2AddAlvor),
                    [new("Numerics", measurements.Matrix3x2AddNumerics)])),
            BenchNode.Compare(
                "Matrix3x2Multiply",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix3x2MultiplyAlvor),
                    [new("Numerics", measurements.Matrix3x2MultiplyNumerics)])),
            BenchNode.Compare(
                "Matrix3x2Invert",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix3x2InvertAlvor),
                    [new("Numerics", measurements.Matrix3x2InvertNumerics)])),
            BenchNode.Compare(
                "Matrix3x2Transform",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix3x2TransformAlvor),
                    [new("Numerics", measurements.Matrix3x2TransformNumerics)])),
            BenchNode.Compare(
                "Matrix4x4Add",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix4x4AddAlvor),
                    [new("Numerics", measurements.Matrix4x4AddNumerics)])),
            BenchNode.Compare(
                "Matrix4x4Multiply",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix4x4MultiplyAlvor),
                    [new("Numerics", measurements.Matrix4x4MultiplyNumerics)])),
            BenchNode.Compare(
                "Matrix4x4Transpose",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix4x4TransposeAlvor),
                    [new("Numerics", measurements.Matrix4x4TransposeNumerics)])),
            BenchNode.Compare(
                "Matrix4x4Invert",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix4x4InvertAlvor),
                    [new("Numerics", measurements.Matrix4x4InvertNumerics)])),
            BenchNode.Compare(
                "Matrix4x4Transform",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.Matrix4x4TransformAlvor),
                    [new("Numerics", measurements.Matrix4x4TransformNumerics)])),
            BenchNode.Compare(
                "QuaternionAdd",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.QuaternionAddAlvor),
                    [new("Numerics", measurements.QuaternionAddNumerics)])),
            BenchNode.Compare(
                "QuaternionMultiply",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.QuaternionMultiplyAlvor),
                    [new("Numerics", measurements.QuaternionMultiplyNumerics)])),
            BenchNode.Compare(
                "QuaternionNormalize",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.QuaternionNormalizeAlvor),
                    [new("Numerics", measurements.QuaternionNormalizeNumerics)])),
            BenchNode.Compare(
                "QuaternionTransform",
                "4,096 inputs; 256 passes",
                new(
                    new("Alvor", measurements.QuaternionTransformAlvor),
                    [new("Numerics", measurements.QuaternionTransformNumerics)]))
    ]);
}
