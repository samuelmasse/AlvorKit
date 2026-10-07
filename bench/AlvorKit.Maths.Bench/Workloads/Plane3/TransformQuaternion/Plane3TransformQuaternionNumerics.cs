namespace AlvorKit;

internal static class Plane3TransformQuaternionNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Plane[] systemPlanes,
        System.Numerics.Plane[] systemOutput,
        System.Numerics.Quaternion[] systemRotations,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Plane.Transform(systemPlanes[i], systemRotations[i]);
        }
    }
}
