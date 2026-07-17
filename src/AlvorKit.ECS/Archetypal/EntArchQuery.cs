namespace AlvorKit.ECS;

/// <summary>Builds an alloc-scoped archetypal span query.</summary>
public readonly struct EntArchQuery<A>
{
    private readonly int allocId;

    internal EntArchQuery(int allocId) => this.allocId = allocId;

    /// <summary>Requires component <typeparamref name="N"/> with value type <typeparamref name="T"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntArchQuery<A, EntArchSelect<T, N, A>> With<T, N>() => new(allocId);
}

/// <summary>Enumerates active alloc-local archs matching a compile-time component selection.</summary>
public readonly struct EntArchQuery<A, TSelect>
    where TSelect : struct, IEntArchSelect<A>
{
    private readonly int allocId;

    internal EntArchQuery(int allocId) => this.allocId = allocId;

    /// <summary>Adds another required component to this query.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntArchQuery<A, EntArchSelect<T, N, A, TSelect>> With<T, N>() => new(allocId);

    /// <summary>Creates an allocation-free chunk enumerator.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(allocId);

    /// <summary>Walks cached matching arch IDs and yields the alloc-local active ones.</summary>
    public ref struct Enumerator
    {
        private readonly int allocId;
        private readonly int matchingArchCount;
        private int matchIndex;
        private EntMut[] ents;
        private int count;

        /// <summary>Gets the current arch's aligned spans.</summary>
        public readonly EntArchChunk<A> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(
                allocId,
                EntArchQueryCache<A, TSelect>.ArchIdAt(matchIndex - 1),
                ents,
                count);
        }

        internal Enumerator(int allocId)
        {
            this.allocId = allocId;
            matchingArchCount = EntArchQueryCache<A, TSelect>.CaptureCount(allocId);
            matchIndex = 0;
            ents = null!;
            count = 0;
        }

        /// <summary>Advances to the next nonempty matching arch.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while ((uint)matchIndex < (uint)matchingArchCount)
            {
                int archId = EntArchQueryCache<A, TSelect>.ArchIdAt(matchIndex++);
                if (EntArchRows<A>.TryGetActive(allocId, archId, out ents, out count))
                    return true;
            }

            return false;
        }
    }
}
