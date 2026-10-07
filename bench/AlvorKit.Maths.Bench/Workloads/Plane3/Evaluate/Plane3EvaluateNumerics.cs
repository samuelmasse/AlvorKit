namespace AlvorKit;

internal static class Plane3EvaluateNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Plane[] systemPlanes,
        System.Numerics.Vector3[] systemVectors,
        float[] scalarOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                scalarOutput[i] = System.Numerics.Plane.DotCoordinate(systemPlanes[i], systemVectors[i]);
        }
    }
}
