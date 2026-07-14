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

/// <summary>Enumerates active alloc-local arches matching a compile-time component selection.</summary>
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

    /// <summary>Walks active arches and yields the ones containing every selected component.</summary>
    public ref struct Enumerator
    {
        private readonly int allocId;
        private readonly int archLimit;
        private int archId;
        private EntMut[] ents;
        private int count;

        /// <summary>Gets the current arch's aligned spans.</summary>
        public readonly EntArchChunk<A> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(allocId, archId, ents, count);
        }

        internal Enumerator(int allocId)
        {
            this.allocId = allocId;
            archLimit = EntArchRows<A>.ArchCapacityAt(allocId);
            archId = EntArchGraph<A>.NoArchId;
            ents = null!;
            count = 0;
        }

        /// <summary>Advances to the next nonempty matching arch.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (++archId < archLimit)
            {
                if (EntArchRows<A>.TryGetActive(allocId, archId, out ents, out count) &&
                    TSelect.Matches(allocId, archId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
