namespace AlvorKit.Mocking;

/// <summary>Reflection lookup caches for one mocked target type.</summary>
internal class TypeCache
{
    private readonly ConcurrentDictionary<MethodInfo, ParameterInfo[]> parameters = [];
    private readonly Type type;
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicEvents)]
    private readonly Type? eventType;

    /// <summary>Creates metadata caches without promising event reflection.</summary>
    internal TypeCache(Type type)
    {
        this.type = type;
    }

    /// <summary>Creates metadata caches for a full mock whose public events are rooted.</summary>
    internal TypeCache(
        [DynamicallyAccessedMembers(
            DynamicallyAccessedMemberTypes.PublicEvents)]
        Type type,
        bool preserveEvents)
    {
        this.type = type;
        eventType = preserveEvents
            ? type
            : null;
    }

    /// <summary>Gets the target type represented by this cache.</summary>
    internal Type Type => type;

    /// <summary>Gets the event-rooted target type for event accessor lookup.</summary>
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicEvents)]
    internal Type EventType =>
        eventType ??
        throw new MockException(
            $"Event lookup for '{type}' requires preserved public event " +
            "metadata.");

    /// <summary>Event lookup cache keyed by add and remove accessors.</summary>
    internal ConcurrentDictionary<MethodInfo, EventInfo?> Events { get; } = [];

    /// <summary>Logical argument order cache keyed by method.</summary>
    internal ConcurrentDictionary<MethodInfo, int[]> ParameterIndices { get; } = [];

    /// <summary>Reference argument index cache keyed by method.</summary>
    internal ConcurrentDictionary<MethodInfo, int[]> RefParameterIndices { get; } = [];

    /// <summary>Returns the stable reflected parameter array for one method.</summary>
    internal ParameterInfo[] GetParameters(MethodInfo method) =>
        parameters.GetOrAdd(
            method,
            static target => target.GetParameters());
}
