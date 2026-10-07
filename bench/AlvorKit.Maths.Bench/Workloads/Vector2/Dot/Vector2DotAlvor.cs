namespace AlvorKit;

internal static class Vector2DotAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft, Vec2[] alvorRight, float[] ScalarOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                ScalarOutput[i] = Vec2.Dot(alvorLeft[i], alvorRight[i]);
        }
    }
}
