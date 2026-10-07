namespace AlvorKit;

/// <summary>Cold context ownership and slot reuse; ordinary writes use the typed plan table directly.</summary>
internal static class EntIdxContexts
{
    /// <summary>Serializes context registration and slot recycling.</summary>
    private static readonly Lock gate = new();
    /// <summary>Released slots available for a new context generation.</summary>
    private static readonly Stack<int> free = [];
    /// <summary>Cold ownership records with generations that reject stale identities.</summary>
    private static (EntIdxContext? Context, int Generation)[] slots = new (EntIdxContext?, int)[2];
    /// <summary>Next unused context slot; zero is reserved for default handles.</summary>
    private static int count = 1;

    /// <summary>Claims a fresh generation in a free or newly grown slot.</summary>
    internal static EntIdxContextKey Add(EntIdxContext context)
    {
        lock (gate)
        {
            int index = free.Count == 0 ? count++ : free.Pop();

            if (index >= slots.Length)
                Array.Resize(ref slots, slots.Length * 2);

            ref var slot = ref slots[index];
            slot.Context = context;
            slot.Generation++;
            return new(index, slot.Generation);
        }
    }

    /// <summary>Resolves an owned identity and rejects stale or disposed contexts.</summary>
    internal static EntIdxContext Get(EntIdxContextKey key)
    {
        var (Context, Generation) = slots[key.Index];

        if (Generation != key.Generation || Context == null)
            throw new ObjectDisposedException(nameof(EntIdxContext));

        return Context;
    }

    /// <summary>Clears the owning reference and makes the slot reusable.</summary>
    internal static void Remove(EntIdxContextKey key)
    {
        lock (gate)
        {
            slots[key.Index].Context = null;
            free.Push(key.Index);
        }
    }
}
