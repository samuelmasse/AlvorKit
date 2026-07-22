namespace AlvorKit.ECS;

/// <summary>Stores the direct row-set column directory used by point access for one exact field.</summary>
internal static class EntArchColumn<T, N, A>
{
    /// <summary>Maps row-set IDs to their typed component arrays without registering the field.</summary>
    internal static T[][] Values = [];

    /// <summary>Gets the registered field ID, initializing the cold column operations on first structural use.</summary>
    internal static int FieldId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => EntArchColumnOps<T, N, A>.FieldId;
    }

    /// <summary>Returns the typed column for a row set, or null when the field is absent.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T[]? ValuesAt(int rowSetId)
    {
        var valuesByRowSet = Values;
        if ((uint)rowSetId >= (uint)valuesByRowSet.Length)
            return null;

        return valuesByRowSet[rowSetId];
    }
}
