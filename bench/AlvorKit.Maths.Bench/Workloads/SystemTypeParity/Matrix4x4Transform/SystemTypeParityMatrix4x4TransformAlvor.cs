namespace AlvorKit;

internal static class SystemTypeParityMatrix4x4TransformAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Mat4[] alvorMat4Left, Vec4[] alvorVec4, Vec4[] alvorVec4Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorVec4Output[i] = alvorMat4Left[i] * alvorVec4[i];
        }
    }
}
