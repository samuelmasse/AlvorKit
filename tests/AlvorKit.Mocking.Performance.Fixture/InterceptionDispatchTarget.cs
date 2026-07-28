namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>
/// Provides one concrete method shared by mocked and unmocked interception
/// receivers.
/// </summary>
public sealed class InterceptionDispatchTarget
{
    /// <summary>Returns the supplied value plus one.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Invoke(int value) => value + 1;
}
