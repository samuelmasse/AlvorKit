namespace AlvorKit;

internal static class SystemTypeParityQuaternionAddAlvor
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Quat[] alvorQuatLeft, Quat[] alvorQuatRight, Quat[] alvorQuatOutput, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                alvorQuatOutput[i] = alvorQuatLeft[i] + alvorQuatRight[i];
        }
    }
}
