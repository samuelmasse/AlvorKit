namespace AlvorKit.ECS;

// Signature internment is separated from the transition implementation without changing the closed generic state.
internal static partial class EntArchGraph<A>
{
    /// <summary>Finds a canonical arch by its sorted fields and precomputed hash.</summary>
    private static int FindBySignature(ReadOnlySpan<int> fieldIds, ulong signatureHash) =>
        EntArchSignatureIndex.Find(signatureArchIds, signatureHash, fieldIds, packedFieldIds, signatureEnds);

    /// <summary>Creates and publishes a canonical arch while computing its signature hash.</summary>
    private static int CreateFromSignature(ReadOnlySpan<int> fieldIds) =>
        CreateFromSignature(fieldIds, EntArchSignatureIndex.Hash(fieldIds));

    /// <summary>Creates and publishes a canonical arch with a known signature hash.</summary>
    private static int CreateFromSignature(ReadOnlySpan<int> fieldIds, ulong signatureHash)
    {
        int archId = AllocateArchId();
        int start = packedFieldCount;
        int nextPackedFieldCount = start + fieldIds.Length;

        if (packedFieldIds.Length < nextPackedFieldCount)
            Array.Resize(ref packedFieldIds, (int)BitOperations.RoundUpToPowerOf2((uint)nextPackedFieldCount));
        fieldIds.CopyTo(packedFieldIds.AsSpan(start, fieldIds.Length));
        signatureEnds[archId] = nextPackedFieldCount;
        packedFieldCount = nextPackedFieldCount;

        EnsureSignatureIndexCapacity(nextArchId - FirstArchId);
        EntArchSignatureIndex.Insert(signatureArchIds, signatureHash, archId);

        if (fieldIds.Length == 1)
        {
            int fieldId = fieldIds[0];
            Volatile.Write(ref singletonArchIds[fieldId], archId);
            singletonArchCount++;
        }

        Volatile.Write(ref publishedArchEnd, nextArchId);
        return archId;
    }

    /// <summary>Reserves the next arch ID and grows parallel arch directories when required.</summary>
    private static int AllocateArchId()
    {
        if (nextArchId == archCapacity)
            EnsureCapacity(fieldCapacity, archCapacity * 2);

        return nextArchId++;
    }

    /// <summary>Grows and rehashes the signature index before it exceeds its load limit.</summary>
    private static void EnsureSignatureIndexCapacity(int requiredCount)
    {
        // Grow before the next insertion would exceed a 75% load.
        int maxCount = signatureArchIds.Length - (signatureArchIds.Length >> 2);
        if (requiredCount <= maxCount)
            return;

        var previousArchIds = signatureArchIds;
        int nextCapacity = previousArchIds.Length == 0
            ? InitialSignatureIndexCapacity
            : previousArchIds.Length * 2;
        signatureArchIds = new int[nextCapacity];

        foreach (int archId in previousArchIds)
        {
            if (archId != NoArchId)
                EntArchSignatureIndex.Insert(signatureArchIds, EntArchSignatureIndex.Hash(FieldIds(archId)), archId);
        }
    }

    /// <summary>Gets the group-global structural scratch prefix for one locked signature operation.</summary>
    private static Span<int> SignatureScratch(int requiredLength)
    {
        if (signatureScratch.Length < requiredLength)
            Array.Resize(ref signatureScratch, (int)BitOperations.RoundUpToPowerOf2((uint)requiredLength));

        return signatureScratch.AsSpan(0, requiredLength);
    }

    /// <summary>Grows field and arch directories to their requested exact capacities.</summary>
    private static void EnsureCapacity(int requiredFieldCapacity, int requiredArchCapacity)
    {
        if (requiredFieldCapacity != fieldCapacity)
        {
            Array.Resize(ref columnOps, requiredFieldCapacity);
            Array.Resize(ref singletonArchIds, requiredFieldCapacity);
            fieldCapacity = requiredFieldCapacity;
        }

        if (requiredArchCapacity != archCapacity)
        {
            Array.Resize(ref signatureEnds, requiredArchCapacity);
            Array.Resize(ref edgeHeads, requiredArchCapacity);
            archCapacity = requiredArchCapacity;
        }
    }

    /// <summary>Inserts one field into a sorted signature.</summary>
    private static void InsertFieldId(ReadOnlySpan<int> srcFieldIds, int fieldId, Span<int> dstFieldIds)
    {
        int insertionIndex = 0;
        while (insertionIndex < srcFieldIds.Length && srcFieldIds[insertionIndex] < fieldId)
            insertionIndex++;

        srcFieldIds[..insertionIndex].CopyTo(dstFieldIds);
        dstFieldIds[insertionIndex] = fieldId;
        srcFieldIds[insertionIndex..].CopyTo(dstFieldIds[(insertionIndex + 1)..]);
    }
}
