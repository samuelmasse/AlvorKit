namespace AlvorKit.ECS;

internal static class EntArchColumn<T, N, A>
{
    internal static readonly int FieldId;
    internal static T[][][] Values;

    static EntArchColumn()
    {
        Values = [];
        FieldId = EntArchGraph<A>.RegisterField(
            new EntArchColumnOps<T, N, A>(),
            Unsafe.SizeOf<T>(),
            EntArchStorageClass<T, A>.Id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T[]? ValuesAt(int allocId, int archId)
    {
        var valuesByAlloc = Values;
        if ((uint)allocId >= (uint)valuesByAlloc.Length)
            return null;

        var valuesByArch = valuesByAlloc[allocId];
        if (valuesByArch == null || (uint)archId >= (uint)valuesByArch.Length)
            return null;

        return valuesByArch[archId];
    }
}
