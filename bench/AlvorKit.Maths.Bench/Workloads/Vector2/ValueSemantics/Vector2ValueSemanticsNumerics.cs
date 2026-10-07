namespace AlvorKit;

internal static class Vector2ValueSemanticsNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector2[] systemLeft,
        System.Numerics.Vector2[] systemRight,
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
