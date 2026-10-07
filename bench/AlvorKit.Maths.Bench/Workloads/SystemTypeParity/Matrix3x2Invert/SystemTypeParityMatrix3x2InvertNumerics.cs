namespace AlvorKit;

internal static class SystemTypeParityMatrix3x2InvertNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Matrix3x2[] systemMat3Left,
        System.Numerics.Matrix3x2[] systemMat3Output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                System.Numerics.Matrix3x2.Invert(systemMat3Left[i], out systemMat3Output[i]);
        }
    }
}
