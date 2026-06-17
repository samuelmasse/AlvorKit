namespace AlvorKit.Mocking;

/// <summary>Reflection lookup caches for one mocked target type.</summary>
internal class TypeCache(Type type)
{
    /// <summary>Gets the target type represented by this cache.</summary>
    internal Type Type => type;

    /// <summary>Event lookup cache keyed by add and remove accessors.</summary>
    internal ConcurrentDictionary<MethodInfo, EventInfo?> Events { get; } = [];

    /// <summary>Logical argument order cache keyed by method.</summary>
    internal ConcurrentDictionary<MethodInfo, int[]> ParameterIndices { get; } = [];

    /// <summary>Reference argument index cache keyed by method.</summary>
    internal ConcurrentDictionary<MethodInfo, int[]> RefParameterIndices { get; } = [];

    /// <summary>Output argument index cache keyed by method.</summary>
    internal ConcurrentDictionary<MethodInfo, int[]> OutParameterIndices { get; } = [];
}
