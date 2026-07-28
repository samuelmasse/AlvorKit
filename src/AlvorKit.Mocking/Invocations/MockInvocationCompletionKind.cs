namespace AlvorKit.Mocking;

/// <summary>Identifies the synchronous completion state of an invocation.</summary>
internal enum MockInvocationCompletionKind
{
    /// <summary>The invocation has entered but not completed.</summary>
    Pending,

    /// <summary>The invocation returned normally.</summary>
    Returned,

    /// <summary>The invocation threw an exception.</summary>
    Threw
}
