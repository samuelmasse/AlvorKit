namespace AlvorKit.Mocking;

/// <summary>Invokes one ordinary callback using an invocation-local call context.</summary>
internal sealed class MockCallbackBehavior : MockConfiguredBehavior
{
    private static readonly object?[] NoReferenceValues = [];
    private readonly Func<MockCall, object?> callback;

    /// <summary>Creates ordinary callback behavior.</summary>
    internal MockCallbackBehavior(Func<MockCall, object?> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        this.callback = callback;
    }

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.Callback,
            null,
            NoReferenceValues,
            callback);
}
