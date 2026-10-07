namespace AlvorKit;

internal static class Plane3EqualsNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Plane[] systemPlanes,
        System.Numerics.Plane[] systemOtherPlanes,
        int[] intOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                intOutput[i] = systemPlanes[i].Equals(systemOtherPlanes[i]) ? 1 : 0;
        }
    }
}
