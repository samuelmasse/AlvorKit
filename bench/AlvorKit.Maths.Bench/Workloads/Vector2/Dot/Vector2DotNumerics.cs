namespace AlvorKit;

internal static class Vector2DotNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector2[] systemLeft,
        System.Numerics.Vector2[] systemRight,
        float[] ScalarOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                ScalarOutput[i] = System.Numerics.Vector2.Dot(systemLeft[i], systemRight[i]);
        }
    }
}
