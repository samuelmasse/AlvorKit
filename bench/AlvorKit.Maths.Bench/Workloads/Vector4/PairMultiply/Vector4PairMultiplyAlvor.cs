namespace AlvorKit;

internal static class Vector4PairMultiplyAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorLeft, Vec4[] alvorRight, Vec4[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = alvorLeft[i] * alvorRight[i];
        }
    }
}
