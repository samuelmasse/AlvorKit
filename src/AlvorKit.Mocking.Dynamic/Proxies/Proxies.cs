namespace AlvorKit.Mocking;

/// <summary>Caches generated proxy types for mockable interfaces and inheritable classes.</summary>
internal static class Proxies
{
    /// <summary>Per-source-module caches held beneath an ephemeron boundary.</summary>
    private static readonly ConditionalWeakTable<Module, ProxyCache> caches = [];

    /// <summary>Returns the generated proxy type for a target type.</summary>
    internal static Type Get(Type type) =>
        caches.GetValue(
            type.Module,
            static _ => new ProxyCache())
        .Get(type);
}
