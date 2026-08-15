namespace AlvorKit;

/// <summary>Tracks intercepted operations beneath weak module ownership.</summary>
internal static class MockInterceptionMethodRegistry
{
    private static readonly ConditionalWeakTable<Module, Registry> Registries =
        [];

    /// <summary>Returns whether one exact method is owned by interception.</summary>
    internal static bool Contains(MethodInfo method) =>
        Registries.TryGetValue(
            MockCollectibleReferenceOwner.Select(method),
            out Registry? registry) &&
        registry.Contains(MockMethodIdentity.Create(method));

    /// <summary>Marks one exact method as intercepted.</summary>
    internal static void Add(MethodInfo method) =>
        Registries.GetValue(
            MockCollectibleReferenceOwner.Select(method),
            static _ => new Registry())
        .Add(MockMethodIdentity.Create(method));

    private sealed class Registry
    {
        private readonly HashSet<MockMethodIdentity> methods = [];

        internal bool Contains(MockMethodIdentity method)
        {
            lock (methods)
                return methods.Contains(method);
        }

        internal void Add(MockMethodIdentity method)
        {
            lock (methods)
                methods.Add(method);
        }
    }
}
