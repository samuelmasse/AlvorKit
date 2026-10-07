namespace AlvorKit;

internal static class SwizzleVec4WzyxCurrentVec4Reverse
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] float4, Vec4[] float4Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                float4Output[i] = float4[i].Wzyx;
        }
    }
}
