namespace AlvorKit;

internal static class SystemTypeParityMatrix4x4InvertAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Mat4[] alvorMat4Left, Mat4[] alvorMat4Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                Mat4.TryInvert(alvorMat4Left[i], out alvorMat4Output[i]);
        }
    }
}
