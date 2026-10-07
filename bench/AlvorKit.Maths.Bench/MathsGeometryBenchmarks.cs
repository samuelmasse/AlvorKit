namespace AlvorKit;

[Bench]
public class MathsGeometryBenchmarks(SystemTypeParityBenchmarks systemTypeParity, Plane3Benchmarks plane3)
{
    public BenchNode[] Create() => [systemTypeParity.Create(), plane3.Create()];
}
