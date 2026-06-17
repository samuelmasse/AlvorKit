namespace AlvorKit.Mocking;

/// <summary>Mutable state owned by one mock or partially mocked instance.</summary>
internal class Mocked(bool partial, TypeCache type)
{
    /// <summary>Lazy defaults keyed by return method.</summary>
    private ConcurrentDictionary<MethodInfo, object?>? defaultValues;

    /// <summary>Configured return values keyed by return method.</summary>
    private ConcurrentDictionary<MethodInfo, List<(object?, object?[], object?[])>>? returnValues;

    /// <summary>Mocked event handlers keyed by event metadata.</summary>
    private ConcurrentDictionary<EventInfo, Delegate>? eventHandlers;

    /// <summary>Gets whether unmatched calls should continue to the original implementation.</summary>
    internal bool Partial => partial;

    /// <summary>Gets reflection metadata cached for this mock's target type.</summary>
    internal TypeCache Type => type;

    /// <summary>Gets the lazy default return value cache.</summary>
    internal ConcurrentDictionary<MethodInfo, object?> DefaultValues
    {
        get
        {
            defaultValues ??= [];
            return defaultValues;
        }
    }

    /// <summary>Gets the configured return values for this mock.</summary>
    internal ConcurrentDictionary<MethodInfo, List<(object?, object?[], object?[])>> ReturnValues
    {
        get
        {
            returnValues ??= [];
            return returnValues;
        }
    }

    /// <summary>Gets the mocked event handler table.</summary>
    internal ConcurrentDictionary<EventInfo, Delegate> EventHandlers
    {
        get
        {
            eventHandlers ??= [];
            return eventHandlers;
        }
    }

    /// <summary>Gets whether any return values have been configured.</summary>
    internal bool HasReturnValues => returnValues != null;

    /// <summary>Gets whether any event handlers have been attached.</summary>
    internal bool HasEventHandlers => eventHandlers != null;
}
