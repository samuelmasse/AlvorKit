namespace AlvorKit;

internal static class SystemTypeParityMatrix4x4MultiplyAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Mat4[] alvorMat4Left, Mat4[] alvorMat4Right, Mat4[] alvorMat4Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorMat4Output[i] = alvorMat4Left[i] * alvorMat4Right[i];
        }
    }
}
