namespace AlvorKit;

internal static class Vector3AddNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector3[] systemLeft,
        System.Numerics.Vector3[] systemRight,
        System.Numerics.Vector3[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = systemLeft[i] + systemRight[i];
        }
    }
}
