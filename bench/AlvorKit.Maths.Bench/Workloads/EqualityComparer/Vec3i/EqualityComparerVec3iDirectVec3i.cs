namespace AlvorKit;

internal static class EqualityComparerVec3iDirectVec3i
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3i[] left3, Vec3i[] right3, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = left3[i].Equals(right3[i]) ? 0 : left3[i].GetHashCode();
        }
    }
}
