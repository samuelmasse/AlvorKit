namespace AlvorKit;

internal static class Vector2BoundsAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft, Vec2[] alvorRight, Vec2[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Vec2.Clamp(Vec2.Abs(alvorLeft[i] - alvorRight[i]), 0.25f, 5f);
        }
    }
}
