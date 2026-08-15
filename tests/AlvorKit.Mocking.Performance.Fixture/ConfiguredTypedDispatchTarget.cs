namespace AlvorKit;

/// <summary>Provides one span-only signature shared by configured typed measurements.</summary>
public sealed class ConfiguredTypedDispatchTarget
{
    /// <summary>Returns the live span length when no configured behavior intercepts the call.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public int Invoke(Span<int> values) => values.Length;
}
