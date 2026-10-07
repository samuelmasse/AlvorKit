namespace AlvorKit;

internal static class Plane3EqualsAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Plane3[] alvorPlanes, Plane3[] alvorOtherPlanes, int[] intOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                intOutput[i] = alvorPlanes[i].Equals(alvorOtherPlanes[i]) ? 1 : 0;
        }
    }
}
