namespace AlvorKit;

internal static class FloatValueSemanticsVec3EqualsAlvorVec3Equals
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] alvorLeft3, Vec3[] alvorRight3, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = alvorLeft3[i].Equals(alvorRight3[i]) ? 1 : 0;
        }
    }
}
