namespace AlvorKit;

/// <summary>Stores the synchronous return or exact thrown exception of an invocation.</summary>
internal sealed class MockInvocationCompletion
{
    private MockInvocationCompletion(
        MockInvocationCompletionKind kind,
        MockInvocationExecutionSource source,
        MockInvocationReturn? returned,
        Exception? exception,
        MockInvocationFailureStage? failureStage)
    {
        Kind = kind;
        Source = source;
        Return = returned;
        Exception = exception;
        FailureStage = failureStage;
    }

    /// <summary>Gets the shared pending completion value.</summary>
    internal static MockInvocationCompletion Pending { get; } =
        new(MockInvocationCompletionKind.Pending, MockInvocationExecutionSource.Unselected, null, null, null);

    /// <summary>Gets the synchronous completion kind.</summary>
    internal MockInvocationCompletionKind Kind { get; }

    /// <summary>Gets the path that executed the invocation.</summary>
    internal MockInvocationExecutionSource Source { get; }

    /// <summary>Gets the retained normal-return metadata.</summary>
    internal MockInvocationReturn? Return { get; }

    /// <summary>Gets the original thrown exception instance.</summary>
    internal Exception? Exception { get; }

    /// <summary>Gets the dispatch stage that threw.</summary>
    internal MockInvocationFailureStage? FailureStage { get; }

    /// <summary>Creates a normal completion.</summary>
    internal static MockInvocationCompletion Returned(
        MockInvocationExecutionSource source,
        MockInvocationReturn returned)
    {
        ValidateSelectedSource(source);
        ArgumentNullException.ThrowIfNull(returned);

        return new(MockInvocationCompletionKind.Returned, source, returned, null, null);
    }

    /// <summary>Creates a throwing completion with exact exception identity.</summary>
    internal static MockInvocationCompletion Threw(
        MockInvocationExecutionSource source,
        Exception exception,
        MockInvocationFailureStage failureStage)
    {
        ValidateSelectedSource(source);
        ArgumentNullException.ThrowIfNull(exception);

        return new(MockInvocationCompletionKind.Threw, source, null, exception, failureStage);
    }

    private static void ValidateSelectedSource(MockInvocationExecutionSource source)
    {
        if (source == MockInvocationExecutionSource.Unselected)
            throw new ArgumentException("A completed invocation requires an execution source.", nameof(source));
    }
}
