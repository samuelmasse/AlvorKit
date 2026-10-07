namespace AlvorKit;

internal static class Plane3CreateFromPointsAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        Plane3[] alvorOutput,
        Vec3[] alvorPoint0,
        Vec3[] alvorPoint1,
        Vec3[] alvorPoint2,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Plane3.CreateFromPoints(alvorPoint0[i], alvorPoint1[i], alvorPoint2[i]);
        }
    }
}
