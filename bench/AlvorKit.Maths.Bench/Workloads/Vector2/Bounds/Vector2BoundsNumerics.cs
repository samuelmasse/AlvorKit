namespace AlvorKit;

internal static class Vector2BoundsNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector2[] systemLeft,
        System.Numerics.Vector2[] systemRight,
        System.Numerics.Vector2[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
            {
                var distance = System.Numerics.Vector2.Abs(systemLeft[i] - systemRight[i]);

                systemOutput[i] = System.Numerics.Vector2.Clamp(distance, new(0.25f), new(5f));
            }
        }
    }
}
