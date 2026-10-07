namespace AlvorKit;

internal static class Plane3NormalizeAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Plane3[] alvorPlanes, Plane3[] alvorOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Plane3.Normalize(alvorPlanes[i]);
        }
    }
}
