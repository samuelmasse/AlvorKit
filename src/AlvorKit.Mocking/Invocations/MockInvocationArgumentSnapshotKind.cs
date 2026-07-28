namespace AlvorKit.Mocking;

/// <summary>Identifies the retained representation of an invocation argument.</summary>
internal enum MockInvocationArgumentSnapshotKind
{
    /// <summary>The ordinary argument object was retained without cloning.</summary>
    Shallow,

    /// <summary>A configured projector produced the retained heap-safe value.</summary>
    Projected,

    /// <summary>History contains metadata instead of an argument value.</summary>
    Unavailable
}
