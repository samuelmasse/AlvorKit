namespace AlvorKit;

internal static class Vector4BoundsAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorLeft, Vec4[] alvorRight, Vec4[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Vec4.Clamp(Vec4.Abs(alvorLeft[i] - alvorRight[i]), 0.25f, 5f);
        }
    }
}
