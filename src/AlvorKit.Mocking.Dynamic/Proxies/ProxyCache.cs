namespace AlvorKit.Mocking;

/// <summary>
/// Owns proxy metadata and one collectible emitter beneath a weak source-module
/// boundary.
/// </summary>
internal sealed class ProxyCache
{
    private readonly Lock cacheLock = new();
    private readonly Dictionary<Type, Type> proxies = [];
    private readonly AssemblyBuilder generatedAssembly;
    private readonly ModuleBuilder generatedModule;

    /// <summary>Creates one independently collectible proxy emitter.</summary>
    internal ProxyCache()
    {
        const string assemblyName = "AlvorKit.Mocking.Proxies";
        string moduleName = $"{assemblyName}.{Guid.NewGuid():N}";
        generatedAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName(assemblyName),
            AssemblyBuilderAccess.RunAndCollect);
        generatedModule =
            generatedAssembly.DefineDynamicModule(moduleName);
    }

    /// <summary>Returns one emitted proxy type for a source type.</summary>
    internal Type Get(Type type)
    {
        lock (cacheLock)
        {
            if (proxies.TryGetValue(type, out Type? proxy))
                return proxy;

            proxy = ProxyTypeBuilder.CreateType(
                generatedModule,
                type);
            proxies.Add(type, proxy);
            return proxy;
        }
    }
}
