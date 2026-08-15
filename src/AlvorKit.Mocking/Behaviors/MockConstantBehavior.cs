namespace AlvorKit;

/// <summary>Returns one configured constant and reference-writeback set.</summary>
internal sealed class MockConstantBehavior : MockConfiguredBehavior
{
    private readonly object? value;
    private readonly object?[] referenceValues;

    /// <summary>Creates a constant behavior with immutable setup-owned values.</summary>
    internal MockConstantBehavior(object? value, object?[] referenceValues)
    {
        this.value = value;
        this.referenceValues = [.. referenceValues];
    }

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(MockBehaviorExecutionKind.Return, value, referenceValues, null);
}
