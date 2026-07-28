namespace AlvorKit.Mocking;

/// <summary>
/// Provides an immutable behavior description whose per-call state is claimed
/// before user code executes.
/// </summary>
internal abstract class MockConfiguredBehavior
{
    /// <summary>Claims the execution state for one matching invocation.</summary>
    internal abstract MockBehaviorExecution Claim();
}
