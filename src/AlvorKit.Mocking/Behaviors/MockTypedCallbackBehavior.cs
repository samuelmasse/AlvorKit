namespace AlvorKit.Mocking;

/// <summary>Retains one exact normalized callback for generated invocation.</summary>
internal sealed class MockTypedCallbackBehavior(
    Delegate callback) : MockConfiguredBehavior
{
    private static readonly object?[] NoReferenceValues = [];
    private readonly Delegate callback = callback;

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.TypedCallback,
            null,
            NoReferenceValues,
            callback);
}
