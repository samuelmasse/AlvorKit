namespace AlvorKit;

internal static class Vector2ScaleAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2[] alvorLeft, Vec2[] alvorOutput, float[] Scalar, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = alvorLeft[i] * Scalar[i];
        }
    }
}
