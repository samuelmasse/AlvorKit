namespace AlvorKit;

/// <summary>Stores an optional asynchronous event on an existing invocation.</summary>
internal sealed record MockInvocationAsyncCompletion
{
    /// <summary>Creates an asynchronous completion.</summary>
    internal MockInvocationAsyncCompletion(
        MockInvocationAsyncCompletionKind kind,
        Exception? exception = null)
    {
        if (kind == MockInvocationAsyncCompletionKind.Faulted && exception is null)
            throw new ArgumentNullException(nameof(exception));
        if (kind != MockInvocationAsyncCompletionKind.Faulted && exception is not null)
            throw new ArgumentException("Only a faulted completion carries an exception.", nameof(exception));

        Kind = kind;
        Exception = exception;
    }

    /// <summary>Gets the asynchronous outcome kind.</summary>
    internal MockInvocationAsyncCompletionKind Kind { get; }

    /// <summary>Gets the exact asynchronous failure instance.</summary>
    internal Exception? Exception { get; }
}
