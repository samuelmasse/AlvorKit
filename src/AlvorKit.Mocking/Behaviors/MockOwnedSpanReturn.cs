namespace AlvorKit.Mocking;

/// <summary>Owns one copied span setup buffer for the lifetime of its configured factory.</summary>
internal sealed class MockOwnedSpanReturn<T>
{
    private readonly T[] values;

    /// <summary>Copies setup input once into storage owned only by this configured behavior.</summary>
    internal MockOwnedSpanReturn(scoped ReadOnlySpan<T> source)
    {
        values = new T[source.Length];
        source.CopyTo(values);
    }

    /// <summary>Returns a fresh mutable span view over the current owned storage.</summary>
    internal Span<T> Mutable() => values;

    /// <summary>Returns a fresh read-only span view over the current owned storage.</summary>
    internal ReadOnlySpan<T> ReadOnly() => values;
}
