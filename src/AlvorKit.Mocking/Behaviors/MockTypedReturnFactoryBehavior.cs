namespace AlvorKit;

/// <summary>Retains one exact zero-argument delegate, never its produced value.</summary>
internal sealed class MockTypedReturnFactoryBehavior : MockConfiguredBehavior
{
    private static readonly object?[] NoReferenceValues = [];
    private readonly Delegate factory;

    /// <summary>Creates a typed factory behavior without invoking the delegate.</summary>
    internal MockTypedReturnFactoryBehavior(Delegate factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        this.factory = factory;
    }

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.TypedReturnFactory,
            null,
            NoReferenceValues,
            factory);
}
