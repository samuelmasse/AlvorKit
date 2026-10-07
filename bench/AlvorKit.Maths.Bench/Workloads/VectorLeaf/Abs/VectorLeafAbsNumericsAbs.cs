namespace AlvorKit;

internal static class VectorLeafAbsNumericsAbs
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemInput,
        System.Numerics.Vector4[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Vector4.Abs(systemInput[i]);
        }
    }
}
