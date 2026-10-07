namespace AlvorKit;

internal static class SystemTypeParityMatrix3x2TransformNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Matrix3x2[] systemMat3Left,
        System.Numerics.Vector2[] systemVec2,
        System.Numerics.Vector2[] systemVec2Output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemVec2Output[i] = System.Numerics.Vector2.Transform(systemVec2[i], systemMat3Left[i]);
        }
    }
}
