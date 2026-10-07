namespace AlvorKit;

internal static class SystemTypeParityQuaternionTransformAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Quat[] alvorQuatLeft, Vec3[] alvorVec3, Vec3[] alvorVec3Output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorVec3Output[i] = Quat.TransformVector(alvorQuatLeft[i], alvorVec3[i]);
        }
    }
}
