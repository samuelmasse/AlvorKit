namespace AlvorKit;

internal static class Vector4DotAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorLeft, Vec4[] alvorRight, float[] ScalarOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                ScalarOutput[i] = Vec4.Dot(alvorLeft[i], alvorRight[i]);
        }
    }
}
