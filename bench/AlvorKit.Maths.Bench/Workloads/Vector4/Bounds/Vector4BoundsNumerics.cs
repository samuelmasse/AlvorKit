namespace AlvorKit;

internal static class Vector4BoundsNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Vector4[] systemLeft,
        System.Numerics.Vector4[] systemRight,
        System.Numerics.Vector4[] systemOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
            {
                var distance = System.Numerics.Vector4.Abs(systemLeft[i] - systemRight[i]);

                systemOutput[i] = System.Numerics.Vector4.Clamp(distance, new(0.25f), new(5f));
            }
        }
    }
}
