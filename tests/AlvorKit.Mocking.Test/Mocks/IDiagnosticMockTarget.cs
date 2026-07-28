namespace AlvorKit.Mocking.Test;

public interface IDiagnosticMockTarget
{
    void Accept(object? value);

    void Values(
        string text,
        int[] values,
        object? diagnostic);
}
