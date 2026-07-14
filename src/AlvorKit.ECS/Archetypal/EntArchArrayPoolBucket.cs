namespace AlvorKit.ECS;

/// <summary>Stores returned arrays for one exact capacity and drops inactive storage after Gen2 aging.</summary>
internal sealed class EntArchArrayPoolBucket<T>
{
    private T[]?[] cached = [];
    private int count;
    private int lastUseGen2Collection;

    // The bucket is private to the pool, so every mutation can lock it without allocating another lock object.
    /// <summary>Returns one cached array, or null when the bucket is empty.</summary>
    internal T[]? Rent(int gen2Collection)
    {
        lock (this)
        {
            lastUseGen2Collection = gen2Collection;
            if (count == 0)
                return null;

            int index = --count;
            T[] values = cached[index]!;
            cached[index] = null;
            return values;
        }
    }

    /// <summary>Pushes an array onto the bucket's synchronized cache.</summary>
    internal void Return(T[] values, int gen2Collection)
    {
        lock (this)
        {
            lastUseGen2Collection = gen2Collection;
            if (count == cached.Length)
                Array.Resize(ref cached, count == 0 ? 1 : count * 2);

            cached[count++] = values;
        }
    }

    /// <summary>Drops cached arrays when the bucket has not been used since the supplied Gen2 collection.</summary>
    internal void TrimIfInactive(int activeSinceGen2Collection)
    {
        lock (this)
        {
            if (lastUseGen2Collection >= activeSinceGen2Collection)
                return;

            cached = [];
            count = 0;
        }
    }
}
