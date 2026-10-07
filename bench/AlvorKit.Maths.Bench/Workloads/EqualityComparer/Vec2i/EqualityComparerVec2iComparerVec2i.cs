namespace AlvorKit;

internal static class EqualityComparerVec2iComparerVec2i
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2i[] left2, Vec2i[] right2, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var comparer = EqualityComparer<Vec2i>.Default;

            for (var i = 0; i < 4096; i++)
                output[i] = comparer.Equals(left2[i], right2[i]) ? 0 : comparer.GetHashCode(left2[i]);
        }
    }
}
