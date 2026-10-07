namespace AlvorKit;

internal static class EqualityComparerVec3iComparerVec3i
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3i[] left3, Vec3i[] right3, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var comparer = EqualityComparer<Vec3i>.Default;

            for (var i = 0; i < 4096; i++)
                output[i] = comparer.Equals(left3[i], right3[i]) ? 0 : comparer.GetHashCode(left3[i]);
        }
    }
}
