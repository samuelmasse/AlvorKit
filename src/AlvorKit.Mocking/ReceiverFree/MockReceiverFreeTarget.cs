namespace AlvorKit.Mocking;

/// <summary>Provides a synthetic receiver for one session-owned interception site.</summary>
internal sealed class MockReceiverFreeTarget(Mocked mocked)
{
    /// <summary>Gets the mock-compatible state used by exact dispatch.</summary>
    internal Mocked Mocked { get; } = mocked;
}
