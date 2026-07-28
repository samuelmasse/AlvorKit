namespace AlvorKit.Mocking;

/// <summary>Caches per-type reflection state used by the mocking runtime.</summary>
internal static class Types
{
    /// <summary>Type cache entries held beneath an ephemeron boundary.</summary>
    private static readonly ConditionalWeakTable<Type, TypeCache> types = [];
    private static readonly Lock gate = new();

    /// <summary>Returns the cached metadata for one target type.</summary>
    internal static TypeCache Get(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicEvents)]
        Type type)
    {
        lock (gate)
        {
            if (types.TryGetValue(type, out TypeCache? cached))
                return cached;

            var created = new TypeCache(
                type,
                preserveEvents: true);
            types.Add(type, created);
            return created;
        }
    }
}
