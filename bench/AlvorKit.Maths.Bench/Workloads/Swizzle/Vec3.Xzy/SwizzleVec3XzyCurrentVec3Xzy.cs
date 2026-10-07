namespace AlvorKit;

internal static class SwizzleVec3XzyCurrentVec3Xzy
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] float3, Vec3[] float3Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                float3Output[i] = float3[i].Xzy;
        }
    }
}
