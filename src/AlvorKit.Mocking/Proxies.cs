namespace AlvorKit.Mocking;

/// <summary>Caches generated proxy types for mockable interfaces and inheritable classes.</summary>
internal static class Proxies
{
    /// <summary>Generated proxy types keyed by target type.</summary>
    private static readonly ConcurrentDictionary<Type, Type> proxies = [];

    /// <summary>Serializes first-time proxy generation for a target type.</summary>
    private static readonly Lock proxyLock = new();

    /// <summary>Returns the generated proxy type for a target type.</summary>
    internal static Type Get(Type type)
    {
        if (proxies.TryGetValue(type, out var val))
            return val;

        lock (proxyLock)
        {
            if (proxies.TryGetValue(type, out val))
                return val;

            var proxyType = ProxyTypeBuilder.CreateType(type);
            proxies.TryAdd(type, proxyType);
            return proxyType;
        }
    }
}
