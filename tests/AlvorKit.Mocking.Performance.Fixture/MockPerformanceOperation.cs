namespace AlvorKit;

/// <summary>Owns one prepared benchmark body and its optional cleanup.</summary>
internal sealed class MockPerformanceOperation(
    Action<int> run,
    Action? cleanup = null)
    : IDisposable
{
    /// <summary>Executes the requested number of measured operations.</summary>
    internal void Run(int operations) => run(operations);

    /// <inheritdoc />
    public void Dispose() => cleanup?.Invoke();
}
