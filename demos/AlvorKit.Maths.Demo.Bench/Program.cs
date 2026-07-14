BenchmarkSwitcher
    .FromAssembly(typeof(Vector2Benchmarks).Assembly)
    .Run(args);

/// <summary>Application-representative vector operation classes found across AlvorZone.</summary>
public enum VectorOperation
{
    Add,
    Subtract,
    PairMultiply,
    Scale,
    Divide,
    Lerp,
    Bounds,
    Dot,
    Normalize,
    ValueSemantics,
    Cross,
}

/// <summary>Shared input and output storage for contiguous vector throughput benchmarks.</summary>
public abstract class VectorBenchmarksBase
{
    protected const int BatchSize = 4_096;

    protected readonly float[] Amount = new float[BatchSize];
    protected readonly float[] Scalar = new float[BatchSize];
    protected readonly float[] ScalarOutput = new float[BatchSize];
    protected readonly int[] IntOutput = new int[BatchSize];

    protected void SetupScalars()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            Amount[index] = (index % 101) / 100f;
            Scalar[index] = 0.75f + ((index % 17) * 0.03125f);
        }
    }

    protected static float Input(int index) => (((index * 17) % 101) - 50) * 0.03125f;
}

/// <summary>Compares AlvorKit and System.Numerics two-component vector throughput.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Vector2Benchmarks : VectorBenchmarksBase
{
    private readonly Vec2[] alvorLeft = new Vec2[BatchSize];
    private readonly Vec2[] alvorRight = new Vec2[BatchSize];
    private readonly Vec2[] alvorOutput = new Vec2[BatchSize];
    private readonly System.Numerics.Vector2[] systemLeft = new System.Numerics.Vector2[BatchSize];
    private readonly System.Numerics.Vector2[] systemRight = new System.Numerics.Vector2[BatchSize];
    private readonly System.Numerics.Vector2[] systemOutput = new System.Numerics.Vector2[BatchSize];

    [Params(
        VectorOperation.Add,
        VectorOperation.Subtract,
        VectorOperation.PairMultiply,
        VectorOperation.Scale,
        VectorOperation.Divide,
        VectorOperation.Lerp,
        VectorOperation.Bounds,
        VectorOperation.Dot,
        VectorOperation.Normalize,
        VectorOperation.ValueSemantics)]
    public VectorOperation Operation { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupScalars();
        for (var index = 0; index < BatchSize; index++)
        {
            var x = Input(index);
            alvorLeft[index] = (x + 1.25f, x - 2.5f);
            alvorRight[index] = (x + 3.75f, x + 4.5f);
            systemLeft[index] = new(x + 1.25f, x - 2.5f);
            systemRight[index] = new(x + 3.75f, x + 4.5f);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void Alvor() => RunAlvor(Operation);

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void Numerics() => RunSystem(Operation);

    private void RunAlvor(VectorOperation operation)
    {
        switch (operation)
        {
            case VectorOperation.Add: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] + alvorRight[i]; break;
            case VectorOperation.Subtract: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] - alvorRight[i]; break;
            case VectorOperation.PairMultiply: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] * alvorRight[i]; break;
            case VectorOperation.Scale: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] * Scalar[i]; break;
            case VectorOperation.Divide: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] / Scalar[i]; break;
            case VectorOperation.Lerp: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec2.Lerp(alvorLeft[i], alvorRight[i], Amount[i]); break;
            case VectorOperation.Bounds: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec2.Clamp(Vec2.Abs(alvorLeft[i] - alvorRight[i]), 0.25f, 5f); break;
            case VectorOperation.Dot: for (var i = 0; i < BatchSize; i++) ScalarOutput[i] = Vec2.Dot(alvorLeft[i], alvorRight[i]); break;
            case VectorOperation.Normalize: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec2.Normalize(alvorLeft[i]); break;
            case VectorOperation.ValueSemantics: for (var i = 0; i < BatchSize; i++) IntOutput[i] = alvorLeft[i].Equals(alvorRight[i]) ? 0 : alvorLeft[i].GetHashCode(); break;
        }
    }

    private void RunSystem(VectorOperation operation)
    {
        switch (operation)
        {
            case VectorOperation.Add: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] + systemRight[i]; break;
            case VectorOperation.Subtract: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] - systemRight[i]; break;
            case VectorOperation.PairMultiply: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] * systemRight[i]; break;
            case VectorOperation.Scale: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] * Scalar[i]; break;
            case VectorOperation.Divide: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] / Scalar[i]; break;
            case VectorOperation.Lerp: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector2.Lerp(systemLeft[i], systemRight[i], Amount[i]); break;
            case VectorOperation.Bounds: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector2.Clamp(System.Numerics.Vector2.Abs(systemLeft[i] - systemRight[i]), new(0.25f), new(5f)); break;
            case VectorOperation.Dot: for (var i = 0; i < BatchSize; i++) ScalarOutput[i] = System.Numerics.Vector2.Dot(systemLeft[i], systemRight[i]); break;
            case VectorOperation.Normalize: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector2.Normalize(systemLeft[i]); break;
            case VectorOperation.ValueSemantics: for (var i = 0; i < BatchSize; i++) IntOutput[i] = systemLeft[i].Equals(systemRight[i]) ? 0 : systemLeft[i].GetHashCode(); break;
        }
    }
}

/// <summary>Compares AlvorKit and System.Numerics three-component vector throughput.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Vector3Benchmarks : VectorBenchmarksBase
{
    private readonly Vec3[] alvorLeft = new Vec3[BatchSize];
    private readonly Vec3[] alvorRight = new Vec3[BatchSize];
    private readonly Vec3[] alvorOutput = new Vec3[BatchSize];
    private readonly System.Numerics.Vector3[] systemLeft = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector3[] systemRight = new System.Numerics.Vector3[BatchSize];
    private readonly System.Numerics.Vector3[] systemOutput = new System.Numerics.Vector3[BatchSize];

    [Params(
        VectorOperation.Add,
        VectorOperation.Subtract,
        VectorOperation.PairMultiply,
        VectorOperation.Scale,
        VectorOperation.Divide,
        VectorOperation.Lerp,
        VectorOperation.Bounds,
        VectorOperation.Dot,
        VectorOperation.Normalize,
        VectorOperation.ValueSemantics,
        VectorOperation.Cross)]
    public VectorOperation Operation { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupScalars();
        for (var index = 0; index < BatchSize; index++)
        {
            var x = Input(index);
            alvorLeft[index] = (x + 1.25f, x - 2.5f, x + 3.125f);
            alvorRight[index] = (x + 3.75f, x + 4.5f, x - 1.875f);
            systemLeft[index] = new(x + 1.25f, x - 2.5f, x + 3.125f);
            systemRight[index] = new(x + 3.75f, x + 4.5f, x - 1.875f);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void Alvor() => RunAlvor(Operation);

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void Numerics() => RunSystem(Operation);

    private void RunAlvor(VectorOperation operation)
    {
        switch (operation)
        {
            case VectorOperation.Add: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] + alvorRight[i]; break;
            case VectorOperation.Subtract: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] - alvorRight[i]; break;
            case VectorOperation.PairMultiply: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] * alvorRight[i]; break;
            case VectorOperation.Scale: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] * Scalar[i]; break;
            case VectorOperation.Divide: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] / Scalar[i]; break;
            case VectorOperation.Lerp: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec3.Lerp(alvorLeft[i], alvorRight[i], Amount[i]); break;
            case VectorOperation.Bounds: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec3.Clamp(Vec3.Abs(alvorLeft[i] - alvorRight[i]), 0.25f, 5f); break;
            case VectorOperation.Dot: for (var i = 0; i < BatchSize; i++) ScalarOutput[i] = Vec3.Dot(alvorLeft[i], alvorRight[i]); break;
            case VectorOperation.Normalize: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec3.Normalize(alvorLeft[i]); break;
            case VectorOperation.ValueSemantics: for (var i = 0; i < BatchSize; i++) IntOutput[i] = alvorLeft[i].Equals(alvorRight[i]) ? 0 : alvorLeft[i].GetHashCode(); break;
            case VectorOperation.Cross: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec3.Cross(alvorLeft[i], alvorRight[i]); break;
        }
    }

    private void RunSystem(VectorOperation operation)
    {
        switch (operation)
        {
            case VectorOperation.Add: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] + systemRight[i]; break;
            case VectorOperation.Subtract: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] - systemRight[i]; break;
            case VectorOperation.PairMultiply: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] * systemRight[i]; break;
            case VectorOperation.Scale: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] * Scalar[i]; break;
            case VectorOperation.Divide: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] / Scalar[i]; break;
            case VectorOperation.Lerp: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector3.Lerp(systemLeft[i], systemRight[i], Amount[i]); break;
            case VectorOperation.Bounds: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector3.Clamp(System.Numerics.Vector3.Abs(systemLeft[i] - systemRight[i]), new(0.25f), new(5f)); break;
            case VectorOperation.Dot: for (var i = 0; i < BatchSize; i++) ScalarOutput[i] = System.Numerics.Vector3.Dot(systemLeft[i], systemRight[i]); break;
            case VectorOperation.Normalize: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector3.Normalize(systemLeft[i]); break;
            case VectorOperation.ValueSemantics: for (var i = 0; i < BatchSize; i++) IntOutput[i] = systemLeft[i].Equals(systemRight[i]) ? 0 : systemLeft[i].GetHashCode(); break;
            case VectorOperation.Cross: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector3.Cross(systemLeft[i], systemRight[i]); break;
        }
    }
}

/// <summary>Compares AlvorKit and System.Numerics four-component vector throughput.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Vector4Benchmarks : VectorBenchmarksBase
{
    private readonly Vec4[] alvorLeft = new Vec4[BatchSize];
    private readonly Vec4[] alvorRight = new Vec4[BatchSize];
    private readonly Vec4[] alvorOutput = new Vec4[BatchSize];
    private readonly System.Numerics.Vector4[] systemLeft = new System.Numerics.Vector4[BatchSize];
    private readonly System.Numerics.Vector4[] systemRight = new System.Numerics.Vector4[BatchSize];
    private readonly System.Numerics.Vector4[] systemOutput = new System.Numerics.Vector4[BatchSize];

    [Params(
        VectorOperation.Add,
        VectorOperation.Subtract,
        VectorOperation.PairMultiply,
        VectorOperation.Scale,
        VectorOperation.Divide,
        VectorOperation.Lerp,
        VectorOperation.Bounds,
        VectorOperation.Dot,
        VectorOperation.Normalize,
        VectorOperation.ValueSemantics)]
    public VectorOperation Operation { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupScalars();
        for (var index = 0; index < BatchSize; index++)
        {
            var x = Input(index);
            alvorLeft[index] = (x + 1.25f, x - 2.5f, x + 3.125f, x + 0.75f);
            alvorRight[index] = (x + 3.75f, x + 4.5f, x - 1.875f, x + 2.25f);
            systemLeft[index] = new(x + 1.25f, x - 2.5f, x + 3.125f, x + 0.75f);
            systemRight[index] = new(x + 3.75f, x + 4.5f, x - 1.875f, x + 2.25f);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void Alvor() => RunAlvor(Operation);

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void Numerics() => RunSystem(Operation);

    private void RunAlvor(VectorOperation operation)
    {
        switch (operation)
        {
            case VectorOperation.Add: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] + alvorRight[i]; break;
            case VectorOperation.Subtract: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] - alvorRight[i]; break;
            case VectorOperation.PairMultiply: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] * alvorRight[i]; break;
            case VectorOperation.Scale: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] * Scalar[i]; break;
            case VectorOperation.Divide: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = alvorLeft[i] / Scalar[i]; break;
            case VectorOperation.Lerp: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec4.Lerp(alvorLeft[i], alvorRight[i], Amount[i]); break;
            case VectorOperation.Bounds: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec4.Clamp(Vec4.Abs(alvorLeft[i] - alvorRight[i]), 0.25f, 5f); break;
            case VectorOperation.Dot: for (var i = 0; i < BatchSize; i++) ScalarOutput[i] = Vec4.Dot(alvorLeft[i], alvorRight[i]); break;
            case VectorOperation.Normalize: for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec4.Normalize(alvorLeft[i]); break;
            case VectorOperation.ValueSemantics: for (var i = 0; i < BatchSize; i++) IntOutput[i] = alvorLeft[i].Equals(alvorRight[i]) ? 0 : alvorLeft[i].GetHashCode(); break;
        }
    }

    private void RunSystem(VectorOperation operation)
    {
        switch (operation)
        {
            case VectorOperation.Add: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] + systemRight[i]; break;
            case VectorOperation.Subtract: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] - systemRight[i]; break;
            case VectorOperation.PairMultiply: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] * systemRight[i]; break;
            case VectorOperation.Scale: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] * Scalar[i]; break;
            case VectorOperation.Divide: for (var i = 0; i < BatchSize; i++) systemOutput[i] = systemLeft[i] / Scalar[i]; break;
            case VectorOperation.Lerp: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector4.Lerp(systemLeft[i], systemRight[i], Amount[i]); break;
            case VectorOperation.Bounds: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector4.Clamp(System.Numerics.Vector4.Abs(systemLeft[i] - systemRight[i]), new(0.25f), new(5f)); break;
            case VectorOperation.Dot: for (var i = 0; i < BatchSize; i++) ScalarOutput[i] = System.Numerics.Vector4.Dot(systemLeft[i], systemRight[i]); break;
            case VectorOperation.Normalize: for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector4.Normalize(systemLeft[i]); break;
            case VectorOperation.ValueSemantics: for (var i = 0; i < BatchSize; i++) IntOutput[i] = systemLeft[i].Equals(systemRight[i]) ? 0 : systemLeft[i].GetHashCode(); break;
        }
    }
}
