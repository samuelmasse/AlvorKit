namespace AlvorKit;

/// <summary>Selects the preserved original implementation.</summary>
internal sealed class MockPassthroughBehavior : MockConfiguredBehavior
{
    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.Passthrough,
            null,
            [],
            null);
}
