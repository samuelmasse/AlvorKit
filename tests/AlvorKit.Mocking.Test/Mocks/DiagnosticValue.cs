namespace AlvorKit.Mocking.Test;

internal sealed class DiagnosticValue
{
    /// <inheritdoc />
    public override string ToString() =>
        throw new InvalidOperationException("Formatting must not invoke user code.");
}
