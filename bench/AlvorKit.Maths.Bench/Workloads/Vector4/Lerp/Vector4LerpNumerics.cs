namespace AlvorKit;

internal static class Vector4LerpNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemLeft,
        System.Numerics.Vector4[] systemRight,
        System.Numerics.Vector4[] systemOutput,
        float[] Amount,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Vector4.Lerp(systemLeft[i], systemRight[i], Amount[i]);
        }
    }
}
