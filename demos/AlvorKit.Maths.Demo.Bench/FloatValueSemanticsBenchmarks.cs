namespace AlvorKit.Maths.Demo.Bench;

/// <summary>Separates floating-vector equality from hashing against the matching System.Numerics shapes.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class FloatValueSemanticsBenchmarks
{
    private const int BatchSize = 4_096;
    private readonly Vec2[] alvorLeft2 = new Vec2[BatchSize];
    private readonly Vec2[] alvorRight2 = new Vec2[BatchSize];
    private readonly Vec3[] alvorLeft3 = new Vec3[BatchSize];
    private readonly Vec3[] alvorRight3 = new Vec3[BatchSize];
    private readonly Vec4[] alvorLeft4 = new Vec4[BatchSize];
    private readonly Vec4[] alvorRight4 = new Vec4[BatchSize];
    private readonly System.Numerics.Vector2[] systemLeft2 = new System.Numerics.Vector2[BatchSize];
    private readonly System.Numerics.Vector2[] systemRight2 = new System.Numerics.Vector2[BatchSize];
    private readonly System.Numerics.Vector3[] systemLeft3 = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector3[] systemRight3 = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector4[] systemLeft4 = new System.Numerics.Vector4[BatchSize];
    private readonly System.Numerics.Vector4[] systemRight4 = new System.Numerics.Vector4[BatchSize];
    private readonly int[] output = new int[BatchSize];

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            alvorLeft2[i] = (x + 1.25f, x - 2.5f);
            alvorRight2[i] = (x + 3.75f, x + 4.5f);
            alvorLeft3[i] = (x + 1.25f, x - 2.5f, x + 3.125f);
            alvorRight3[i] = (x + 3.75f, x + 4.5f, x - 1.875f);
            alvorLeft4[i] = (x + 1.25f, x - 2.5f, x + 3.125f, x + 0.75f);
            alvorRight4[i] = (x + 3.75f, x + 4.5f, x - 1.875f, x + 2.25f);
            systemLeft2[i] = alvorLeft2[i];
            systemRight2[i] = alvorRight2[i];
            systemLeft3[i] = alvorLeft3[i];
            systemRight3[i] = alvorRight3[i];
            systemLeft4[i] = alvorLeft4[i];
            systemRight4[i] = alvorRight4[i];
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2 Equals")]
    public void AlvorVec2Equals() { for (var i = 0; i < BatchSize; i++) output[i] = alvorLeft2[i].Equals(alvorRight2[i]) ? 1 : 0; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2 Equals")]
    public void NumericsVec2Equals() { for (var i = 0; i < BatchSize; i++) output[i] = systemLeft2[i].Equals(systemRight2[i]) ? 1 : 0; }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2 Hash")]
    public void AlvorVec2Hash() { for (var i = 0; i < BatchSize; i++) output[i] = alvorLeft2[i].GetHashCode(); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2 Hash")]
    public void NumericsVec2Hash() { for (var i = 0; i < BatchSize; i++) output[i] = systemLeft2[i].GetHashCode(); }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3 Equals")]
    public void AlvorVec3Equals() { for (var i = 0; i < BatchSize; i++) output[i] = alvorLeft3[i].Equals(alvorRight3[i]) ? 1 : 0; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3 Equals")]
    public void NumericsVec3Equals() { for (var i = 0; i < BatchSize; i++) output[i] = systemLeft3[i].Equals(systemRight3[i]) ? 1 : 0; }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3 Hash")]
    public void AlvorVec3Hash() { for (var i = 0; i < BatchSize; i++) output[i] = alvorLeft3[i].GetHashCode(); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3 Hash")]
    public void NumericsVec3Hash() { for (var i = 0; i < BatchSize; i++) output[i] = systemLeft3[i].GetHashCode(); }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4 Equals")]
    public void AlvorVec4Equals() { for (var i = 0; i < BatchSize; i++) output[i] = alvorLeft4[i].Equals(alvorRight4[i]) ? 1 : 0; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4 Equals")]
    public void NumericsVec4Equals() { for (var i = 0; i < BatchSize; i++) output[i] = systemLeft4[i].Equals(systemRight4[i]) ? 1 : 0; }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4 Hash")]
    public void AlvorVec4Hash() { for (var i = 0; i < BatchSize; i++) output[i] = alvorLeft4[i].GetHashCode(); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4 Hash")]
    public void NumericsVec4Hash() { for (var i = 0; i < BatchSize; i++) output[i] = systemLeft4[i].GetHashCode(); }
}
