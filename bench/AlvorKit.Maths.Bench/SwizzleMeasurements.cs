namespace AlvorKit;

[Bench]
public class SwizzleMeasurements
{
    private object? retained;

    public BenchResult Vec3XzyCurrentVec3Xzy()
    {
        var fixture = new SwizzleFixture();
        var float3 = fixture.Float3;
        var float3Output = fixture.Float3Output;
        var timer = BenchTimer.Start();
        SwizzleVec3XzyCurrentVec3Xzy.Run(float3, float3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3XzyIntrinsicVec3Xzy()
    {
        if (!Sse2.IsSupported)
            throw new PlatformNotSupportedException("Intrinsic swizzle benchmarks require SSE2.");
        var fixture = new SwizzleFixture();
        var float3 = fixture.Float3;
        var float3Output = fixture.Float3Output;
        var timer = BenchTimer.Start();
        SwizzleVec3XzyIntrinsicVec3Xzy.Run(float3, float3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3iXzyCurrentVec3iXzy()
    {
        var fixture = new SwizzleFixture();
        var int3 = fixture.Int3;
        var int3Output = fixture.Int3Output;
        var timer = BenchTimer.Start();
        SwizzleVec3iXzyCurrentVec3iXzy.Run(int3, int3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec3iXzyIntrinsicVec3iXzy()
    {
        if (!Sse2.IsSupported)
            throw new PlatformNotSupportedException("Intrinsic swizzle benchmarks require SSE2.");
        var fixture = new SwizzleFixture();
        var int3 = fixture.Int3;
        var int3Output = fixture.Int3Output;
        var timer = BenchTimer.Start();
        SwizzleVec3iXzyIntrinsicVec3iXzy.Run(int3, int3Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4WzyxCurrentVec4Reverse()
    {
        var fixture = new SwizzleFixture();
        var float4 = fixture.Float4;
        var float4Output = fixture.Float4Output;
        var timer = BenchTimer.Start();
        SwizzleVec4WzyxCurrentVec4Reverse.Run(float4, float4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4WzyxIntrinsicVec4Reverse()
    {
        if (!Sse2.IsSupported)
            throw new PlatformNotSupportedException("Intrinsic swizzle benchmarks require SSE2.");
        var fixture = new SwizzleFixture();
        var float4 = fixture.Float4;
        var float4Output = fixture.Float4Output;
        var timer = BenchTimer.Start();
        SwizzleVec4WzyxIntrinsicVec4Reverse.Run(float4, float4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4iWzyxCurrentVec4iReverse()
    {
        var fixture = new SwizzleFixture();
        var int4 = fixture.Int4;
        var int4Output = fixture.Int4Output;
        var timer = BenchTimer.Start();
        SwizzleVec4iWzyxCurrentVec4iReverse.Run(int4, int4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }

    public BenchResult Vec4iWzyxIntrinsicVec4iReverse()
    {
        if (!Sse2.IsSupported)
            throw new PlatformNotSupportedException("Intrinsic swizzle benchmarks require SSE2.");
        var fixture = new SwizzleFixture();
        var int4 = fixture.Int4;
        var int4Output = fixture.Int4Output;
        var timer = BenchTimer.Start();
        SwizzleVec4iWzyxIntrinsicVec4iReverse.Run(int4, int4Output, 256);
        var result = timer.Stop(4096 * 256, "value");
        retained = fixture;
        return result;
    }
}
