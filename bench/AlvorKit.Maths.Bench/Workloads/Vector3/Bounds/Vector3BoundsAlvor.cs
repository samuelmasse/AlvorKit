namespace AlvorKit;

internal static class Vector3BoundsAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] alvorLeft, Vec3[] alvorRight, Vec3[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Vec3.Clamp(Vec3.Abs(alvorLeft[i] - alvorRight[i]), 0.25f, 5f);
        }
    }
}
