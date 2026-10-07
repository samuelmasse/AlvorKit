namespace AlvorKit;

internal static class Vector3NormalizeNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector3[] systemLeft,
        System.Numerics.Vector3[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Vector3.Normalize(systemLeft[i]);
        }
    }
}
