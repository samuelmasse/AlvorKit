namespace AlvorKit;

internal static class Vector3DotAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] alvorLeft, Vec3[] alvorRight, float[] ScalarOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                ScalarOutput[i] = Vec3.Dot(alvorLeft[i], alvorRight[i]);
        }
    }
}
