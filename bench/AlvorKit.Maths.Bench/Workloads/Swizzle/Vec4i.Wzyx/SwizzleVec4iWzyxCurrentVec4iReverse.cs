namespace AlvorKit;

internal static class SwizzleVec4iWzyxCurrentVec4iReverse
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4i[] int4, Vec4i[] int4Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                int4Output[i] = int4[i].Wzyx;
        }
    }
}
