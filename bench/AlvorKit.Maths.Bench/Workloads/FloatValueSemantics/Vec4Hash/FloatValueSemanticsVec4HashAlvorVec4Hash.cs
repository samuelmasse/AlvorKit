namespace AlvorKit;

internal static class FloatValueSemanticsVec4HashAlvorVec4Hash
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorLeft4, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = alvorLeft4[i].GetHashCode();
        }
    }
}
