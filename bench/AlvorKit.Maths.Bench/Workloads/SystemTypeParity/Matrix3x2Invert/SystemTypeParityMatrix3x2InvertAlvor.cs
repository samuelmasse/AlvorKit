namespace AlvorKit;

internal static class SystemTypeParityMatrix3x2InvertAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Mat3x2[] alvorMat3Left, Mat3x2[] alvorMat3Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                Mat3x2.TryInvert(alvorMat3Left[i], out alvorMat3Output[i]);
        }
    }
}
