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

    /// <summary>Walks the smaller of cached matching archs and alloc-local active row sets.</summary>
    public ref struct Enumerator
    {
        private readonly int allocId;
        // Nonnegative scans matching arch IDs; bitwise-complemented active counts scan active row sets.
        private readonly int candidateCount;
        private int candidateIndex;
        // MoveNext retains this so Current does not repeat the alloc/arch directory lookup.
        private int rowSetId;
        private EntMut[] ents;
        private int count;

        /// <summary>Gets the current arch's aligned spans.</summary>
        public readonly EntArchChunk<A> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(
                rowSetId,
                ents,
                count);
        }

        internal Enumerator(int allocId)
        {
            this.allocId = allocId;
            int matchingArchCount = EntArchQueryCache<A, TSelect>.CaptureCount(allocId, out bool matchBitsReady);
            int activeRowSetCount = EntArchRows<A>.ActiveCountAt(allocId);
            if (activeRowSetCount < matchingArchCount)
            {
                if (activeRowSetCount != 0 && !matchBitsReady)
                    EntArchQueryCache<A, TSelect>.EnsureMatchBits();
                candidateCount = ~activeRowSetCount;
            }
            else candidateCount = matchingArchCount;
            candidateIndex = 0;
            rowSetId = EntArchRows<A>.NoRowSetId;
            ents = null!;
            count = 0;
        }

        /// <summary>Advances to the next nonempty matching arch.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (candidateCount >= 0)
            {
                while ((uint)candidateIndex < (uint)candidateCount)
                {
                    int archId = EntArchQueryCache<A, TSelect>.ArchIdAt(candidateIndex++);
                    rowSetId = EntArchRows<A>.RowSetIdAt(allocId, archId);
                    if (EntArchRows<A>.TryGetActive(rowSetId, out ents, out count))
                        return true;
                }

                return false;
            }

            int activeRowSetCount = ~candidateCount;
            while ((uint)candidateIndex < (uint)activeRowSetCount)
            {
                rowSetId = EntArchRows<A>.ActiveRowSetIdAt(allocId, candidateIndex++);
                if (EntArchQueryCache<A, TSelect>.MatchesArch(EntArchRows<A>.ArchIdAt(rowSetId)))
                {
                    EntArchRows<A>.GetActive(rowSetId, out ents, out count);
                    return true;
                }
            }

            return false;
        }
    }
}
