namespace AlvorKit;

internal static class SystemTypeParityMatrix4x4TransposeNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Matrix4x4[] systemMat4Left,
        System.Numerics.Matrix4x4[] systemMat4Output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemMat4Output[i] = System.Numerics.Matrix4x4.Transpose(systemMat4Left[i]);
        }
    }
}
