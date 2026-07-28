namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>Provides an original-call control that bypasses all mocking instrumentation.</summary>
public sealed class DirectDispatchTarget
{
    /// <summary>Returns the supplied value plus one.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Invoke(int value) => value + 1;
}
