namespace AlvorKit;

internal static class SystemTypeParityMatrix3x2AddNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Matrix3x2[] systemMat3Left,
        System.Numerics.Matrix3x2[] systemMat3Right,
        System.Numerics.Matrix3x2[] systemMat3Output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemMat3Output[i] = systemMat3Left[i] + systemMat3Right[i];
        }
    }
}
