namespace AlvorKit;

internal static class FloatValueSemanticsVec3EqualsNumericsVec3Equals
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector3[] systemLeft3,
        System.Numerics.Vector3[] systemRight3,
        int[] output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = systemLeft3[i].Equals(systemRight3[i]) ? 1 : 0;
        }
    }
}
