namespace AlvorKit.Mocking;

/// <summary>Retains one stable exact ref-result delegate, never the referenced value.</summary>
internal sealed class MockTypedRefReturnFactoryBehavior(
    Delegate factory) : MockConfiguredBehavior
{
    private static readonly object?[] NoReferenceValues = [];
    private readonly Delegate factory = factory;

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.TypedRefReturnFactory,
            null,
            NoReferenceValues,
            factory);
}
