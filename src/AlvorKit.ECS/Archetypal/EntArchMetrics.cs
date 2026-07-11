namespace AlvorKit.ECS;

// Value-only snapshot of retained archetypal storage. Logical payload bytes include allocated array capacity,
// but exclude managed object headers, alignment, runtime type data, JIT data, and GC fragmentation. Catalog
// used/slack bytes split arrays at their current ID high-water marks, including reserved ID slots below them.
internal struct EntArchMetrics
{
    internal int RegisteredFieldCount;
    internal int FieldCapacity;
    internal int MaterializedArchCount;
    internal int ArchCapacity;
    internal int SignatureMembershipCount;
    internal int SignatureMembershipCapacity;
    internal int FieldLayoutCount;
    internal int FieldLayoutCapacity;
    internal int SignatureIndexCount;
    internal int SignatureIndexCapacity;
    internal int SignatureScratchCapacity;
    internal int SingletonArchCount;
    internal int SingletonDirectoryCapacity;
    internal long DirectedStructuralEdgeCount;
    internal long TransitionCellCapacity;
    internal int StoredTransitionEdgeCount;
    internal int TransitionEdgeCapacity;
    internal int EdgeHeadCapacity;

    internal int AllocDirectoryCount;
    internal int AllocDirectoryCapacity;
    internal long ArchDirectorySlotCapacity;
    internal long RetainedStateCount;
    internal long ActiveStateCount;
    internal long ActiveRowCount;
    internal long RowCapacity;
    internal long RowSlack;

    internal long ComponentBufferCount;
    internal long ComponentCapacity;

    internal long CatalogLogicalPayloadBytes;
    internal long CatalogUsedLogicalPayloadBytes;
    internal long CatalogSlackLogicalPayloadBytes;
    internal long RowLogicalPayloadBytes;
    internal long EntUsedLogicalPayloadBytes;
    internal long EntSlackLogicalPayloadBytes;
    internal long ColumnLogicalPayloadBytes;
    internal long ComponentLogicalPayloadBytes;
    internal long ComponentUsedLogicalPayloadBytes;
    internal long ComponentSlackLogicalPayloadBytes;
    internal long EstimatedManagedBytes;
    internal long CatalogManagedObjectCount;
    internal long StorageManagedObjectCount;
    internal long OwnedManagedObjectCount;

    internal readonly long TotalLogicalRetainedBytes =>
        CatalogLogicalPayloadBytes +
        RowLogicalPayloadBytes +
        ColumnLogicalPayloadBytes +
        ComponentLogicalPayloadBytes;

    internal void AddCatalogArray<T>(T[] array, long usedLength)
    {
        int elementSize = Unsafe.SizeOf<T>();
        long usedBytes = usedLength * elementSize;
        long slackBytes = (array.LongLength - usedLength) * elementSize;

        CatalogUsedLogicalPayloadBytes += usedBytes;
        CatalogSlackLogicalPayloadBytes += slackBytes;
        AddArray(ref CatalogLogicalPayloadBytes, array.LongLength, elementSize, true);
    }

    internal void AddRowArray<T>(T[] array) =>
        AddArray(ref RowLogicalPayloadBytes, array.LongLength, Unsafe.SizeOf<T>(), false);

    internal void AddEntArray(EntMut[] ents, int count)
    {
        int elementSize = Unsafe.SizeOf<EntMut>();
        EntUsedLogicalPayloadBytes += (long)count * elementSize;
        EntSlackLogicalPayloadBytes += (ents.LongLength - count) * elementSize;
        AddArray(ref RowLogicalPayloadBytes, ents.LongLength, elementSize, false);
    }

    internal void AddColumnArray<T>(T[] array) =>
        AddArray(ref ColumnLogicalPayloadBytes, array.LongLength, Unsafe.SizeOf<T>(), false);

    internal void AddComponentArray<T>(T[] array, int count)
    {
        int elementSize = Unsafe.SizeOf<T>();
        ComponentUsedLogicalPayloadBytes += (long)count * elementSize;
        ComponentSlackLogicalPayloadBytes += (array.LongLength - count) * elementSize;
        AddArray(ref ComponentLogicalPayloadBytes, array.LongLength, elementSize, false);
    }

    internal void AddCatalogObjects(long count)
    {
        CatalogManagedObjectCount += count;
        OwnedManagedObjectCount += count;
        EstimatedManagedBytes += count * EstimatedObjectBytes;
    }

    private static int EstimatedObjectBytes => 3 * IntPtr.Size;

    private void AddArray(ref long categoryBytes, long length, int elementSize, bool catalog)
    {
        // Collection expressions use runtime-shared empty arrays; they are not owned by this arch group.
        if (length == 0)
            return;

        long payloadBytes = length * elementSize;
        categoryBytes += payloadBytes;
        if (catalog)
            CatalogManagedObjectCount++;
        else StorageManagedObjectCount++;
        OwnedManagedObjectCount++;
        EstimatedManagedBytes += EstimatedObjectBytes + AlignToPointer(payloadBytes);
    }

    private static long AlignToPointer(long bytes)
    {
        int alignment = IntPtr.Size;
        return (bytes + alignment - 1) & -alignment;
    }
}
