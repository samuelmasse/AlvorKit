namespace AlvorKit;

internal static class Vector4ValueSemanticsNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemLeft,
        System.Numerics.Vector4[] systemRight,
        int[] IntOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                IntOutput[i] = systemLeft[i].Equals(systemRight[i]) ? 0 : systemLeft[i].GetHashCode();
        }
    }
}
