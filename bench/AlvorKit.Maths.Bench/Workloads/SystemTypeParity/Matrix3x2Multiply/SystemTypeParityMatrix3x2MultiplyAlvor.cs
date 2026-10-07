namespace AlvorKit;

internal static class SystemTypeParityMatrix3x2MultiplyAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Mat3x2[] alvorMat3Left, Mat3x2[] alvorMat3Right, Mat3x2[] alvorMat3Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorMat3Output[i] = alvorMat3Left[i] * alvorMat3Right[i];
        }
    }
}
