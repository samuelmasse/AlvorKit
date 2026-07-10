namespace AlvorKit.ECS;

internal readonly record struct EntArchField(
    int ByteWidth,
    int StorageClassId)
{
    internal const int ByteStorageClassId = 0;

    internal bool ContainsReferences => StorageClassId != ByteStorageClassId;
}
