namespace AlvorKit.Mocking;

/// <summary>Caches per-type reflection state used by the mocking runtime.</summary>
internal static class Types
{
    /// <summary>Type cache entries keyed by target type.</summary>
    private static readonly ConcurrentDictionary<Type, TypeCache> types = [];

    /// <summary>Serializes first-time creation of a type cache.</summary>
    private static readonly Lock typeLock = new();

    /// <summary>Returns the cached metadata for one target type.</summary>
    internal static TypeCache Get(Type type)
    {
        if (types.TryGetValue(type, out var val))
            return val;

        lock (typeLock)
        {
            if (types.TryGetValue(type, out val))
                return val;

            var cache = new TypeCache(type);
            types.TryAdd(type, cache);
            return cache;
        }
    }
}
