namespace AlvorKit;

internal static class Plane3EvaluateAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Plane3[] alvorPlanes, Vec3[] alvorVectors, float[] scalarOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                scalarOutput[i] = Plane3.Evaluate(alvorPlanes[i], alvorVectors[i]);
        }
    }
}
