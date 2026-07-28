namespace AlvorKit.Mocking;

/// <summary>Identifies which selected path executed an invocation.</summary>
internal enum MockInvocationExecutionSource
{
    /// <summary>No behavior has completed yet.</summary>
    Unselected,

    /// <summary>A configured behavior executed.</summary>
    Configured,

    /// <summary>An explicit loose fallback executed.</summary>
    LooseFallback,

    /// <summary>A strict fallback rejected the invocation.</summary>
    StrictFailure,

    /// <summary>An instance partial mock called its original implementation.</summary>
    PartialPassthrough,

    /// <summary>A receiver-free operation preserved its original implementation.</summary>
    ReceiverFreeOriginal,

    /// <summary>A library-managed event accessor executed.</summary>
    EventAccessor
}
