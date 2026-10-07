namespace AlvorKit;

internal static class Vector2PairMultiplyAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft, Vec2[] alvorRight, Vec2[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = alvorLeft[i] * alvorRight[i];
        }
    }
}
