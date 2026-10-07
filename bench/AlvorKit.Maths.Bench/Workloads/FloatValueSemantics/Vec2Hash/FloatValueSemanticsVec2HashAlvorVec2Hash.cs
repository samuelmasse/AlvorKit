namespace AlvorKit;

internal static class FloatValueSemanticsVec2HashAlvorVec2Hash
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft2, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = alvorLeft2[i].GetHashCode();
        }
    }
}
