namespace AlvorKit;

internal static class FloatValueSemanticsVec4EqualsAlvorVec4Equals
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorLeft4, Vec4[] alvorRight4, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = alvorLeft4[i].Equals(alvorRight4[i]) ? 1 : 0;
        }
    }
}
