namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>Provides a concrete span-parameter method for warm typed dispatch measurements.</summary>
public sealed class TypedDispatchTarget
{
    /// <summary>Returns the ordinary value plus the live span length.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Invoke(int value, Span<int> values) => value + values.Length;
}
