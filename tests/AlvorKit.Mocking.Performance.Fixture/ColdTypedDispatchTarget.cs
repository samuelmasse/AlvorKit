namespace AlvorKit;

/// <summary>Provides distinct closed concrete types for cache-cold exact wrapper samples.</summary>
public sealed class ColdTypedDispatchTarget<TTag>
{
    /// <summary>Returns the live span length.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Invoke(Span<int> values) => values.Length;
}
