namespace AlvorKit;

internal static class ConversionVec3ToVec3dManualVec3ToVec3d
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] floats, Vec3d[] doubleOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
            {
                var v = floats[i];
                doubleOutput[i] = new(v.X, v.Y, v.Z);
            }
        }
    }
}
