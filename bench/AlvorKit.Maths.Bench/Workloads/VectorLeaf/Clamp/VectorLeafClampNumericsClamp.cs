namespace AlvorKit;

internal static class VectorLeafClampNumericsClamp
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemInput,
        System.Numerics.Vector4[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var min = new System.Numerics.Vector4(0.25f);
            var max = new System.Numerics.Vector4(5f);

            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Vector4.Clamp(systemInput[i], min, max);
        }
    }
}
