namespace AlvorKit.Interception.CallerProof.Test;

internal static class LateMetadataRelocationTarget
{
    private static readonly int privateStorage = 7;

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static int Caller() => 3;

    private static ref readonly int PrivateValue() => ref privateStorage;

    private static T PrivateIdentity<T>(T value) => value;
}
