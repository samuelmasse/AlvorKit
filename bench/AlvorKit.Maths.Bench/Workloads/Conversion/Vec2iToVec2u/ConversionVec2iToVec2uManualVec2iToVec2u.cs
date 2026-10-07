namespace AlvorKit;

internal static class ConversionVec2iToVec2uManualVec2iToVec2u
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec2i[] ints, Vec2u[] uintOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
            {
                var v = ints[i];
                uintOutput[i] = new((uint)v.X, (uint)v.Y);
            }
        }
    }
}
