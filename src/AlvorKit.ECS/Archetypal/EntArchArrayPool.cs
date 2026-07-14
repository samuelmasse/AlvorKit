namespace AlvorKit.ECS;

/// <summary>Caches exact power-of-two arrays for one value type across all archetypal groups and allocs.</summary>
internal static class EntArchArrayPool<T>
{
    // Archetypal capacity starts at four, so bucket zero represents 1 << 2.
    private const int MinimumCapacityShift = 2;

    private static readonly EntArchArrayPoolBucket<T>?[] Buckets = new EntArchArrayPoolBucket<T>?[
        BitOperations.Log2((uint)Array.MaxLength) - MinimumCapacityShift + 1];
    private static readonly bool ClearOnReturn = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
    private static int lastGen2Collection = GC.CollectionCount(2);

    /// <summary>Rents an array whose length is exactly <paramref name="capacity"/>.</summary>
    internal static T[] Rent(int capacity)
    {
        int bucket = BucketIndex(capacity);
        int gen2Collection = GC.CollectionCount(2);
        T[]? values = Buckets[bucket]?.Rent(gen2Collection);
        TrimInactiveBuckets(gen2Collection);
        return values ?? new T[capacity];
    }

    /// <summary>Moves the live prefix to a larger pooled array and returns the previous array.</summary>
    internal static T[] Grow(T[] values, int capacity, int count)
    {
        T[] grown = Rent(capacity);
        values.AsSpan(0, count).CopyTo(grown);
        Return(values);
        return grown;
    }

    /// <summary>Moves the live prefix to a smaller pooled array and returns the previous array.</summary>
    internal static T[] Reduce(T[] values, int capacity, int count)
    {
        T[] reduced = Rent(capacity);
        values.AsSpan(0, count).CopyTo(reduced);
        Return(values);
        return reduced;
    }

    /// <summary>Returns an array, clearing it first only when its value type contains references.</summary>
    internal static void Return(T[] values)
    {
        int gen2Collection = GC.CollectionCount(2);
        int bucket = BucketIndex(values.Length);
        if (ClearOnReturn)
            Array.Clear(values);
        GetBucket(bucket).Return(values, gen2Collection);
        TrimInactiveBuckets(gen2Collection);
    }

    private static EntArchArrayPoolBucket<T> GetBucket(int bucket)
    {
        EntArchArrayPoolBucket<T>? current = Buckets[bucket];
        if (current != null)
            return current;

        var created = new EntArchArrayPoolBucket<T>();
        return Interlocked.CompareExchange(ref Buckets[bucket], created, null) ?? created;
    }

    private static int BucketIndex(int capacity) =>
        BitOperations.TrailingZeroCount((uint)capacity) - MinimumCapacityShift;

    private static void TrimInactiveBuckets(int gen2Collection)
    {
        int previous = lastGen2Collection;
        if (gen2Collection == previous ||
            Interlocked.CompareExchange(ref lastGen2Collection, gen2Collection, previous) != previous)
        {
            return;
        }

        for (int bucket = 0; bucket < Buckets.Length; bucket++)
            Buckets[bucket]?.TrimIfInactive(previous);
    }
}
