namespace AlvorKit;

[Bench]
public class MathsVectorsBenchmarks(Vector2Benchmarks vector2, Vector3Benchmarks vector3, Vector4Benchmarks vector4)
{
    public BenchNode[] Create() => [vector2.Create(), vector3.Create(), vector4.Create()];
}
