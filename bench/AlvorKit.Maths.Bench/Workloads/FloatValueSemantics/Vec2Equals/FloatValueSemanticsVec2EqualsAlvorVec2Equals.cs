namespace AlvorKit;

internal static class FloatValueSemanticsVec2EqualsAlvorVec2Equals
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft2, Vec2[] alvorRight2, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = alvorLeft2[i].Equals(alvorRight2[i]) ? 1 : 0;
        }
    }
}
