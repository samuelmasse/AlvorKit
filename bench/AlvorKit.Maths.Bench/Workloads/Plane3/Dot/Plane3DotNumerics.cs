namespace AlvorKit;

internal static class Plane3DotNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Plane[] systemPlanes,
        System.Numerics.Vector4[] systemCoefficients,
        float[] scalarOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                scalarOutput[i] = System.Numerics.Plane.Dot(systemPlanes[i], systemCoefficients[i]);
        }
    }
}
