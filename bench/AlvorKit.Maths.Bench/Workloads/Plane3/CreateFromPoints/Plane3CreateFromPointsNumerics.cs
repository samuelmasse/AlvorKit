namespace AlvorKit;

internal static class Plane3CreateFromPointsNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Plane[] systemOutput,
        System.Numerics.Vector3[] systemPoint0,
        System.Numerics.Vector3[] systemPoint1,
        System.Numerics.Vector3[] systemPoint2,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Plane.CreateFromVertices(systemPoint0[i], systemPoint1[i], systemPoint2[i]);
        }
    }
}
