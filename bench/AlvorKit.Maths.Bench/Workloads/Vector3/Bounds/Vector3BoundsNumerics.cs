namespace AlvorKit;

internal static class Vector3BoundsNumerics
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
            {
                var distance = System.Numerics.Vector3.Abs(systemLeft[i] - systemRight[i]);

                systemOutput[i] = System.Numerics.Vector3.Clamp(distance, new(0.25f), new(5f));
            }
        }
    }
}
