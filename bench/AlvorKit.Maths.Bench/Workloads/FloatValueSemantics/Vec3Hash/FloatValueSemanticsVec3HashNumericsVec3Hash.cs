namespace AlvorKit;

internal static class FloatValueSemanticsVec3HashNumericsVec3Hash
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(System.Numerics.Vector3[] systemLeft3, int[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = systemLeft3[i].GetHashCode();
        }
    }
}
