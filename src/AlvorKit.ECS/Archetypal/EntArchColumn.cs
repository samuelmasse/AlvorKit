namespace AlvorKit.ECS;

/// <summary>Stores the direct alloc/arch column directory used by point access for one exact field.</summary>
internal static class EntArchColumn<T, N, A>
{
    /// <summary>Maps alloc and arch IDs to their typed component arrays without registering the field.</summary>
    internal static T[][][] Values = [];

    /// <summary>Gets the registered field ID, initializing the cold column operations on first structural use.</summary>
    internal static int FieldId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => EntArchColumnOps<T, N, A>.FieldId;
    }

    /// <summary>Returns the typed column for an alloc and arch, or null when the field is absent.</summary>
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
