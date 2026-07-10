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
}
