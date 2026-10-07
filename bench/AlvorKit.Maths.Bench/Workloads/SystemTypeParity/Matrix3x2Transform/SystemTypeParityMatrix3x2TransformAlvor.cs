namespace AlvorKit;

internal static class SystemTypeParityMatrix3x2TransformAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Mat3x2[] alvorMat3Left, Vec2[] alvorVec2, Vec2[] alvorVec2Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorVec2Output[i] = Mat3x2.TransformPoint(alvorMat3Left[i], alvorVec2[i]);
        }
    }
}
