namespace AlvorKit;

internal static class Vector2LerpAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft, Vec2[] alvorRight, Vec2[] alvorOutput, float[] Amount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Vec2.Lerp(alvorLeft[i], alvorRight[i], Amount[i]);
        }
    }
}
