namespace AlvorKit.Mocking;

/// <summary>Identifies the retained representation of an invocation return.</summary>
internal enum MockInvocationReturnKind
{
    /// <summary>The intercepted member returns no value.</summary>
    Void,

    /// <summary>An ordinary return was retained shallowly.</summary>
    Shallow,

    /// <summary>A borrowed or backend-unobservable return was not retained.</summary>
    Unavailable
}
