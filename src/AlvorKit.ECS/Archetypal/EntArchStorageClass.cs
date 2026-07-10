namespace AlvorKit.ECS;

internal static class EntArchStorageClass<T, A>
{
    internal static readonly int Id = RuntimeHelpers.IsReferenceOrContainsReferences<T>()
        ? EntArchGraph<A>.RegisterStorageClass()
        : EntArchField.ByteStorageClassId;
}
