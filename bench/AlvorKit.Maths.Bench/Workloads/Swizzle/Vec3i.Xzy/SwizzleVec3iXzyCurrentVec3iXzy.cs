namespace AlvorKit;

internal static class SwizzleVec3iXzyCurrentVec3iXzy
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3i[] int3, Vec3i[] int3Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                int3Output[i] = int3[i].Xzy;
        }
    }
}
