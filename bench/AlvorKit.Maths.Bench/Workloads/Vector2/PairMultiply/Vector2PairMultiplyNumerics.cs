namespace AlvorKit;

internal static class Vector2PairMultiplyNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector2[] systemLeft,
        System.Numerics.Vector2[] systemRight,
        System.Numerics.Vector2[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = systemLeft[i] * systemRight[i];
        }
    }
}
