namespace AlvorKit;

internal static class SystemTypeParityQuaternionNormalizeAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Quat[] alvorQuatLeft, Quat[] alvorQuatOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorQuatOutput[i] = Quat.Normalize(alvorQuatLeft[i]);
        }
    }
}
