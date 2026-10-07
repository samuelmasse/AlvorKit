namespace AlvorKit;

internal static class ConversionVec3dToVec3CastVec3dToVec3
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3d[] doubles, Vec3[] floatOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                floatOutput[i] = (Vec3)doubles[i];
        }
    }
}
