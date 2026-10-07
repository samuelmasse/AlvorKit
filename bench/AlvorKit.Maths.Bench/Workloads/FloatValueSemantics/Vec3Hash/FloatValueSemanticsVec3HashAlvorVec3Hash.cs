namespace AlvorKit;

internal static class FloatValueSemanticsVec3HashAlvorVec3Hash
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] alvorLeft3, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = alvorLeft3[i].GetHashCode();
        }
    }
}
