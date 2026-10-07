namespace AlvorKit;

internal static class Vector2ValueSemanticsAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft, Vec2[] alvorRight, int[] IntOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                IntOutput[i] = alvorLeft[i].Equals(alvorRight[i]) ? 0 : alvorLeft[i].GetHashCode();
        }
    }
}
