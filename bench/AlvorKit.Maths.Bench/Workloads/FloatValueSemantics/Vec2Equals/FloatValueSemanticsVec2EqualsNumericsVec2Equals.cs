namespace AlvorKit;

internal static class FloatValueSemanticsVec2EqualsNumericsVec2Equals
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector2[] systemLeft2,
        System.Numerics.Vector2[] systemRight2,
        int[] output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = systemLeft2[i].Equals(systemRight2[i]) ? 1 : 0;
        }
    }
}
