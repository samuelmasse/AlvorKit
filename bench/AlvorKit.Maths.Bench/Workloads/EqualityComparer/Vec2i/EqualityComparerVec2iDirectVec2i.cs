namespace AlvorKit;

internal static class EqualityComparerVec2iDirectVec2i
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2i[] left2, Vec2i[] right2, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = left2[i].Equals(right2[i]) ? 0 : left2[i].GetHashCode();
        }
    }
}
