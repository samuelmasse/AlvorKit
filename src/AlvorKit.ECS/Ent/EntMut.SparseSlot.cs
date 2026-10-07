namespace AlvorKit;

public readonly partial record struct EntMut
{
    /// <summary>Returns existing storage without creating a page. Validate liveness before reading values or committing.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref (int Generation, T? Value) FindSparseSlot<T, N>()
    {
        if (!FetchPage<T, N>(out var page))
            return ref Unsafe.NullRef<(int, T?)>();

        return ref page[SubIndex];
    }

    /// <summary>Resolves storage once for an observed read/commit operation. The caller must validate liveness.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref (int Generation, T? Value) RequireSparseSlot<T, N>()
    {
        if (!FetchPage<T, N>(out var page))
            page = RequireSparsePage<T, N>();

        return ref page[SubIndex];
    }

    /// <summary>Creates a missing sparse page under its storage lock, outside the inlined write path.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private (int Generation, T? Value)[] RequireSparsePage<T, N>()
    {
        lock (EntStorage<T, N>.Lock)
        {
            if (!FetchPage<T, N>(out var page))
                page = CreatePage<T, N>();

            return page;
        }
    }
}
