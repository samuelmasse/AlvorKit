namespace AlvorKit.Maths.Demo.Bench;

/// <summary>Measures the conversion directions that occur most often in Craftdig.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ConversionBenchmarks
{
    private const int BatchSize = 4_096;
    private readonly Vec3d[] doubles = new Vec3d[BatchSize];
    private readonly Vec3[] floats = new Vec3[BatchSize];
    private readonly Vec3[] floatOutput = new Vec3[BatchSize];
    private readonly Vec3d[] doubleOutput = new Vec3d[BatchSize];
    private readonly Vec2i[] ints = new Vec2i[BatchSize];
    private readonly Vec2u[] uintOutput = new Vec2u[BatchSize];

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125;
            doubles[i] = (x + 1.25, x - 2.5, x + 3.125);
            floats[i] = ((float)(x + 1.25), (float)(x - 2.5), (float)(x + 3.125));
            ints[i] = (i - 2_048, 2_048 - (i * 3));
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3dToVec3")]
    public void CastVec3dToVec3() { for (var i = 0; i < BatchSize; i++) floatOutput[i] = (Vec3)doubles[i]; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3dToVec3")]
    public void ManualVec3dToVec3() { for (var i = 0; i < BatchSize; i++) { var v = doubles[i]; floatOutput[i] = new((float)v.X, (float)v.Y, (float)v.Z); } }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3ToVec3d")]
    public void CastVec3ToVec3d() { for (var i = 0; i < BatchSize; i++) doubleOutput[i] = (Vec3d)floats[i]; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3ToVec3d")]
    public void ManualVec3ToVec3d() { for (var i = 0; i < BatchSize; i++) { var v = floats[i]; doubleOutput[i] = new(v.X, v.Y, v.Z); } }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2iToVec2u")]
    public void CastVec2iToVec2u() { for (var i = 0; i < BatchSize; i++) uintOutput[i] = (Vec2u)ints[i]; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2iToVec2u")]
    public void ManualVec2iToVec2u() { for (var i = 0; i < BatchSize; i++) { var v = ints[i]; uintOutput[i] = new((uint)v.X, (uint)v.Y); } }
}

/// <summary>Measures direct and default-comparer value semantics used by fixed-vector hash containers.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class EqualityComparerBenchmarks
{
    private const int BatchSize = 4_096;
    private readonly Vec2i[] left2 = new Vec2i[BatchSize];
    private readonly Vec2i[] right2 = new Vec2i[BatchSize];
    private readonly Vec3i[] left3 = new Vec3i[BatchSize];
    private readonly Vec3i[] right3 = new Vec3i[BatchSize];
    private readonly int[] output = new int[BatchSize];

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            left2[i] = (i - 2_048, 2_048 - (i * 3));
            right2[i] = i % 8 == 0 ? left2[i] : left2[i] + Vec2i.One;
            left3[i] = (i - 2_048, 2_048 - (i * 3), (i * 7) - 1_024);
            right3[i] = i % 8 == 0 ? left3[i] : left3[i] + Vec3i.One;
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2i")]
    public void DirectVec2i() { for (var i = 0; i < BatchSize; i++) output[i] = left2[i].Equals(right2[i]) ? 0 : left2[i].GetHashCode(); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec2i")]
    public void ComparerVec2i()
    {
        var comparer = EqualityComparer<Vec2i>.Default;
        for (var i = 0; i < BatchSize; i++) output[i] = comparer.Equals(left2[i], right2[i]) ? 0 : comparer.GetHashCode(left2[i]);
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3i")]
    public void DirectVec3i() { for (var i = 0; i < BatchSize; i++) output[i] = left3[i].Equals(right3[i]) ? 0 : left3[i].GetHashCode(); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3i")]
    public void ComparerVec3i()
    {
        var comparer = EqualityComparer<Vec3i>.Default;
        for (var i = 0; i < BatchSize; i++) output[i] = comparer.Equals(left3[i], right3[i]) ? 0 : comparer.GetHashCode(left3[i]);
    }
}

/// <summary>Screens representative generated swizzle getters against safe hardware shuffles.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SwizzleBenchmarks
{
    private const int BatchSize = 4_096;
    private readonly Vec3[] float3 = new Vec3[BatchSize];
    private readonly Vec3[] float3Output = new Vec3[BatchSize];
    private readonly Vec3i[] int3 = new Vec3i[BatchSize];
    private readonly Vec3i[] int3Output = new Vec3i[BatchSize];
    private readonly Vec4[] float4 = new Vec4[BatchSize];
    private readonly Vec4[] float4Output = new Vec4[BatchSize];
    private readonly Vec4i[] int4 = new Vec4i[BatchSize];
    private readonly Vec4i[] int4Output = new Vec4i[BatchSize];

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            float3[i] = (x + 1, x + 2, x + 3);
            int3[i] = (i, i + 1, i + 2);
            float4[i] = (x + 1, x + 2, x + 3, x + 4);
            int4[i] = (i, i + 1, i + 2, i + 3);
        }

        if (float3[7].XZY != IntrinsicXzy(float3[7]) || int3[7].XZY != IntrinsicXzy(int3[7]) ||
            float4[7].WZYX != IntrinsicReverse(float4[7]) || int4[7].WZYX != IntrinsicReverse(int4[7]))
            throw new InvalidOperationException("Intrinsic swizzle candidate changed the result.");
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3.XZY")]
    public void CurrentVec3Xzy() { for (var i = 0; i < BatchSize; i++) float3Output[i] = float3[i].XZY; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3.XZY")]
    public void IntrinsicVec3Xzy() { for (var i = 0; i < BatchSize; i++) float3Output[i] = IntrinsicXzy(float3[i]); }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3i.XZY")]
    public void CurrentVec3iXzy() { for (var i = 0; i < BatchSize; i++) int3Output[i] = int3[i].XZY; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec3i.XZY")]
    public void IntrinsicVec3iXzy() { for (var i = 0; i < BatchSize; i++) int3Output[i] = IntrinsicXzy(int3[i]); }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4.WZYX")]
    public void CurrentVec4Reverse() { for (var i = 0; i < BatchSize; i++) float4Output[i] = float4[i].WZYX; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4.WZYX")]
    public void IntrinsicVec4Reverse() { for (var i = 0; i < BatchSize; i++) float4Output[i] = IntrinsicReverse(float4[i]); }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4i.WZYX")]
    public void CurrentVec4iReverse() { for (var i = 0; i < BatchSize; i++) int4Output[i] = int4[i].WZYX; }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Vec4i.WZYX")]
    public void IntrinsicVec4iReverse() { for (var i = 0; i < BatchSize; i++) int4Output[i] = IntrinsicReverse(int4[i]); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vec3 IntrinsicXzy(Vec3 value)
    {
        if (!Sse.IsSupported)
            return value.XZY;
        var packed = Vector128.Create(value.X, value.Y, value.Z, 0f);
        var shuffled = Sse.Shuffle(packed, packed, 0xd8);
        return new(shuffled[0], shuffled[1], shuffled[2]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vec3i IntrinsicXzy(Vec3i value)
    {
        if (!Sse2.IsSupported)
            return value.XZY;
        var shuffled = Sse2.Shuffle(Vector128.Create(value.X, value.Y, value.Z, 0), 0xd8);
        return new(shuffled[0], shuffled[1], shuffled[2]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vec4 IntrinsicReverse(Vec4 value)
    {
        if (!Sse.IsSupported)
            return value.WZYX;
        var packed = Unsafe.BitCast<Vec4, Vector128<float>>(value);
        return Unsafe.BitCast<Vector128<float>, Vec4>(Sse.Shuffle(packed, packed, 0x1b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vec4i IntrinsicReverse(Vec4i value)
    {
        if (!Sse2.IsSupported)
            return value.WZYX;
        var packed = Unsafe.BitCast<Vec4i, Vector128<int>>(value);
        return Unsafe.BitCast<Vector128<int>, Vec4i>(Sse2.Shuffle(packed, 0x1b));
    }
}

/// <summary>Measures whether JIT method hints improve a representative composed Vec3 hot path.</summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 3, exportCombinedDisassemblyReport: true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class InliningHintBenchmarks
{
    private const int BatchSize = 4_096;
    private readonly Vec3[] left = new Vec3[BatchSize];
    private readonly Vec3[] right = new Vec3[BatchSize];
    private readonly Vec3[] output = new Vec3[BatchSize];

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            left[i] = (x + 1.25f, x + 2.5f, x + 3.75f);
            right[i] = (x * 0.25f + 4.5f, x * -0.5f + 2.25f, x * 0.75f - 1.5f);
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize)]
    public void DefaultJit() { for (var i = 0; i < BatchSize; i++) output[i] = DefaultHotPath(left[i], right[i]); }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void AggressiveInlining() { for (var i = 0; i < BatchSize; i++) output[i] = InlineHotPath(left[i], right[i]); }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void AggressiveOptimization() { for (var i = 0; i < BatchSize; i++) output[i] = OptimizeHotPath(left[i], right[i]); }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void BothHints() { for (var i = 0; i < BatchSize; i++) output[i] = BothHotPath(left[i], right[i]); }

    [Benchmark(OperationsPerInvoke = BatchSize)]
    public void NoInliningControl() { for (var i = 0; i < BatchSize; i++) output[i] = NoInlineHotPath(left[i], right[i]); }

    private static Vec3 DefaultHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vec3 InlineHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Vec3 OptimizeHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Vec3 BothHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vec3 NoInlineHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;
}

/// <summary>Separates the Vec4 collision-style bounds pipeline into its Abs and Clamp leaves.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class VectorLeafBenchmarks
{
    private const int BatchSize = 4_096;
    private readonly Vec4[] alvorInput = new Vec4[BatchSize];
    private readonly Vec4[] alvorOutput = new Vec4[BatchSize];
    private readonly System.Numerics.Vector4[] systemInput = new System.Numerics.Vector4[BatchSize];
    private readonly System.Numerics.Vector4[] systemOutput = new System.Numerics.Vector4[BatchSize];

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            alvorInput[i] = (x - 4.5f, x + 2.25f, x - 1.5f, x + 7.75f);
            systemInput[i] = alvorInput[i];
        }
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Abs")]
    public void AlvorAbs() { for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec4.Abs(alvorInput[i]); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Abs")]
    public void NumericsAbs() { for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector4.Abs(systemInput[i]); }

    [Benchmark(Baseline = true, OperationsPerInvoke = BatchSize), BenchmarkCategory("Clamp")]
    public void AlvorClamp() { for (var i = 0; i < BatchSize; i++) alvorOutput[i] = Vec4.Clamp(alvorInput[i], 0.25f, 5f); }

    [Benchmark(OperationsPerInvoke = BatchSize), BenchmarkCategory("Clamp")]
    public void NumericsClamp()
    {
        var min = new System.Numerics.Vector4(0.25f);
        var max = new System.Numerics.Vector4(5f);
        for (var i = 0; i < BatchSize; i++) systemOutput[i] = System.Numerics.Vector4.Clamp(systemInput[i], min, max);
    }
}
