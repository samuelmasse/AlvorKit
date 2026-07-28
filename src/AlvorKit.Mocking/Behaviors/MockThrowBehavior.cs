namespace AlvorKit.Mocking;

/// <summary>Returns a claim that throws one configured exception instance.</summary>
internal sealed class MockThrowBehavior : MockConfiguredBehavior
{
    private static readonly object?[] NoReferenceValues = [];
    private readonly Exception exception;

    /// <summary>Creates configured exception behavior.</summary>
    internal MockThrowBehavior(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        this.exception = exception;
    }

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim() =>
        new(
            MockBehaviorExecutionKind.Throw,
            exception,
            NoReferenceValues,
            null);
}
