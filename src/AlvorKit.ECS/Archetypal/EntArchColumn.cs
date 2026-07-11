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
        if (archId == EntArchGraph<A>.NoArchId || Values.Length <= allocId)
            return null;

        var valuesByArch = Values[allocId];
        if (valuesByArch == null || valuesByArch.Length <= archId)
            return null;

        return valuesByArch[archId];
    }
}
