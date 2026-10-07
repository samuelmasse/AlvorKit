namespace AlvorKit;

internal static class Vector4LerpAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorLeft, Vec4[] alvorRight, Vec4[] alvorOutput, float[] Amount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Vec4.Lerp(alvorLeft[i], alvorRight[i], Amount[i]);
        }
    }
}
