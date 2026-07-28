namespace AlvorKit.Mocking;

/// <summary>Identifies an optionally observed asynchronous outcome.</summary>
internal enum MockInvocationAsyncCompletionKind
{
    /// <summary>The returned operation completed successfully.</summary>
    Succeeded,

    /// <summary>The returned operation faulted.</summary>
    Faulted,

    /// <summary>The returned operation was canceled.</summary>
    Canceled
}
