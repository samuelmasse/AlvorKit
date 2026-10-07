namespace AlvorKit;

internal static class VectorLeafAbsAlvorAbs
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec4[] alvorInput, Vec4[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Vec4.Abs(alvorInput[i]);
        }
    }
}
