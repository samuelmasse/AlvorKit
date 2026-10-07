namespace AlvorKit;

internal static class Vector3DotNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector3[] systemLeft,
        System.Numerics.Vector3[] systemRight,
        float[] ScalarOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                ScalarOutput[i] = System.Numerics.Vector3.Dot(systemLeft[i], systemRight[i]);
        }
    }
}
