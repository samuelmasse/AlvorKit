namespace AlvorKit.ECS;

/// <summary>Exposes aligned Ent and component spans for one active alloc-local arch.</summary>
public readonly ref struct EntArchChunk<A>
{
    private readonly int rowSetId;
    private readonly EntMut[] ents;
    private readonly int count;

    /// <summary>Gets the Ents aligned with every component span returned by this chunk.</summary>
    public ReadOnlySpan<EntMut> Ents
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ents.AsSpan(0, count);
    }

    internal EntArchChunk(int rowSetId, EntMut[] ents, int count)
    {
        this.rowSetId = rowSetId;
        this.ents = ents;
        this.count = count;
    }

    /// <summary>Gets an aligned mutable span, or an empty span when this arch lacks the component.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> Get<T, N>()
    {
        var values = EntArchColumn<T, N, A>.ValuesAt(rowSetId);
        return values == null ? default : values.AsSpan(0, count);
    }
}
