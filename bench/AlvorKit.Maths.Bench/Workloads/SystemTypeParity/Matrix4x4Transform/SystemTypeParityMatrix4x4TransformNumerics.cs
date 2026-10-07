namespace AlvorKit;

internal static class SystemTypeParityMatrix4x4TransformNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Matrix4x4[] systemMat4Left,
        System.Numerics.Vector4[] systemVec4,
        System.Numerics.Vector4[] systemVec4Output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemVec4Output[i] = System.Numerics.Vector4.Transform(systemVec4[i], systemMat4Left[i]);
        }
    }
}
