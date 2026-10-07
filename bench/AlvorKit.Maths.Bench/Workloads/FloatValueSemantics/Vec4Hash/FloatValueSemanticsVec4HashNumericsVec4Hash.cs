namespace AlvorKit;

internal static class FloatValueSemanticsVec4HashNumericsVec4Hash
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(System.Numerics.Vector4[] systemLeft4, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = systemLeft4[i].GetHashCode();
        }
    }
}
