namespace AlvorKit;

internal static class Vector4DotNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemLeft,
        System.Numerics.Vector4[] systemRight,
        float[] ScalarOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                ScalarOutput[i] = System.Numerics.Vector4.Dot(systemLeft[i], systemRight[i]);
        }
    }
}
