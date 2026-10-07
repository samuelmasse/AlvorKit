namespace AlvorKit;

internal static class Plane3TransformMatrixNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Plane[] systemPlanes,
        System.Numerics.Plane[] systemOutput,
        System.Numerics.Matrix4x4[] systemTransforms,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemOutput[i] = System.Numerics.Plane.Transform(systemPlanes[i], systemTransforms[i]);
        }
    }
}
