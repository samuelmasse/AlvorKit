namespace AlvorKit;

internal static class Plane3TransformMatrixAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Plane3[] alvorPlanes, Plane3[] alvorOutput, Mat4[] alvorTransforms, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorOutput[i] = Plane3.Transform(alvorPlanes[i], alvorTransforms[i]);
        }
    }
}
