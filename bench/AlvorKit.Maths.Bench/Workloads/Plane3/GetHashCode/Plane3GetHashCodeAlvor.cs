namespace AlvorKit;

internal static class Plane3GetHashCodeAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Plane3[] alvorPlanes, int[] intOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                intOutput[i] = alvorPlanes[i].GetHashCode();
        }
    }
}
