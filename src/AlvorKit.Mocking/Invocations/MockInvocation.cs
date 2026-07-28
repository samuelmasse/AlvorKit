namespace AlvorKit.Mocking;

/// <summary>Provides one consistent heap-safe invocation snapshot.</summary>
internal sealed class MockInvocation
{
    private readonly MockInvocationArgument[] arguments;
    private readonly MockInvocationArgumentSnapshot[]
        selectionArguments;

    /// <summary>Creates an immutable invocation snapshot.</summary>
    internal MockInvocation(
        MockInvocationIdentity identity,
        MockInvocationCoordinate coordinate,
        MockHistoryEpoch epoch,
        MockInvocationArgument[] arguments,
        MockInvocationArgumentSnapshot[] selectionArguments,
        MockInvocationCompletion completion,
        MockInvocationAsyncCompletion? asyncCompletion,
        bool isVerified)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(selectionArguments);
        Identity = identity;
        Coordinate = coordinate;
        Epoch = epoch;
        this.arguments = arguments;
        this.selectionArguments = selectionArguments;
        Completion = completion;
        AsyncCompletion = asyncCompletion;
        IsVerified = isVerified;
    }

    /// <summary>Gets the target, operation, and backend identity.</summary>
    internal MockInvocationIdentity Identity { get; }

    /// <summary>Gets the logical timeline coordinate assigned at entry.</summary>
    internal MockInvocationCoordinate Coordinate { get; }

    /// <summary>Gets the history epoch entered by the invocation.</summary>
    internal MockHistoryEpoch Epoch { get; }

    /// <summary>Gets retained arguments in declared parameter order.</summary>
    internal ReadOnlySpan<MockInvocationArgument> Arguments => arguments;

    /// <summary>
    /// Gets immutable original entry snapshots used only for later matching.
    /// </summary>
    internal ReadOnlySpan<MockInvocationArgumentSnapshot>
        SelectionArguments => selectionArguments;

    /// <summary>Gets the synchronous completion snapshot.</summary>
    internal MockInvocationCompletion Completion { get; }

    /// <summary>Gets the optional asynchronous completion event.</summary>
    internal MockInvocationAsyncCompletion? AsyncCompletion { get; }

    /// <summary>Gets whether successful verification marked this invocation.</summary>
    internal bool IsVerified { get; }
}
