namespace AlvorKit;

internal static class Vector4PairMultiplyNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemLeft,
        System.Numerics.Vector4[] systemRight,
        System.Numerics.Vector4[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = systemLeft[i] * systemRight[i];
        }
    }
}
