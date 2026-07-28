namespace AlvorKit.Interception.CallerProof.Test;

internal static class LateMetadataTarget
{
    private static nint replacementPointer;

    internal static nint ReplacementPointer
    {
        set => Volatile.Write(ref replacementPointer, value);
    }

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static int Caller()
    {
        var original = 2;
        return original;
    }

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe int CalliTemplate() =>
        ((delegate* unmanaged[Cdecl]<int>)Volatile.Read(
            ref replacementPointer))();

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    internal static int Replacement(int value) => value + 68;
}
