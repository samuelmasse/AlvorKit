namespace AlvorKit;

[Bench]
public class MathsBenchmarks(
    MathsVectorsBenchmarks vectors,
    MathsValuesBenchmarks values,
    MathsGeometryBenchmarks geometry,
    MathsHelpersBenchmarks helpers) : IBenchSuiteProvider
{
    public BenchSuite Create() => new(
        "AlvorKit.Maths.Bench",
        [.. vectors.Create(), .. values.Create(), .. geometry.Create(), .. helpers.Create()]);
}
