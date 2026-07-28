namespace AlvorKit.Mocking.Test;

internal sealed class ReentrantDiagnosticValue(Action onFormatting)
{
    internal int FormattingCount { get; private set; }

    /// <inheritdoc />
    public override string ToString()
    {
        FormattingCount++;
        onFormatting();
        return "unsafe diagnostic value";
    }
}
