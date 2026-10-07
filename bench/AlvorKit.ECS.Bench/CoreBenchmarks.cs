namespace AlvorKit;

[Bench]
public class CoreBenchmarks(CoreMeasurements measurements, LifetimeBenchmarks lifetime) : IBenchSuiteProvider
{
    public BenchSuite Create() => new("AlvorKit.ECS.Bench",
    [
        lifetime.Create(),
        BenchNode.Measure("SparseSet", "sparse Set on prepared handles", measurements.SparseSet),
        BenchNode.Measure("Handles", "three archetypal components through handles", measurements.Handles),
        BenchNode.Measure("Rows", "three archetypal components through rows", measurements.Rows),
        BenchNode.Measure("Spans", "three archetypal components through chunk spans", measurements.Spans),
        BenchNode.Measure("Simd", "three archetypal components through Vector256 chunk spans", measurements.Simd),
        BenchNode.Measure("CreateSparse", "create 1,048,576 Ents with three values; storage growth included", measurements.CreateSparse),
        BenchNode.Measure("CreateSetters", "create 1,048,576 Ents with three values; storage growth included", measurements.CreateSetters),
        BenchNode.Measure("CreateShape", "create 1,048,576 Ents with three values; storage growth included", measurements.CreateShape),
    ]);
}
