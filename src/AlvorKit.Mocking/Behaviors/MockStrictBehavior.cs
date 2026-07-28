namespace AlvorKit.Mocking;

/// <summary>Selects an explicit strict failure for one matching setup.</summary>
internal sealed class MockStrictBehavior : MockConfiguredBehavior
{
    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.Strict,
            null,
            [],
            null);
}
