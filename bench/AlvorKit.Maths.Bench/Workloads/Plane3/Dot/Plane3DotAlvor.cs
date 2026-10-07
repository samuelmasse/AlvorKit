namespace AlvorKit;

internal static class Plane3DotAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Plane3[] alvorPlanes, Vec4[] alvorCoefficients, float[] scalarOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                scalarOutput[i] = Plane3.Dot(alvorPlanes[i], alvorCoefficients[i]);
        }
    }
}
