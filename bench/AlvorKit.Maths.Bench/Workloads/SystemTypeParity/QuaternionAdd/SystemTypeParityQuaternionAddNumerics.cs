namespace AlvorKit;

internal static class SystemTypeParityQuaternionAddNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Quaternion[] systemQuatLeft,
        System.Numerics.Quaternion[] systemQuatRight,
        System.Numerics.Quaternion[] systemQuatOutput,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemQuatOutput[i] = systemQuatLeft[i] + systemQuatRight[i];
        }
    }
}
