namespace AlvorKit;

internal static class SystemTypeParityQuaternionTransformNumerics
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(
        System.Numerics.Quaternion[] systemQuatLeft,
        System.Numerics.Vector3[] systemVec3,
        System.Numerics.Vector3[] systemVec3Output,
        int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                systemVec3Output[i] = System.Numerics.Vector3.Transform(systemVec3[i], systemQuatLeft[i]);
        }
    }
}
