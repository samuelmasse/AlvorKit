namespace AlvorKit.Text;

/// <summary>Formats a typed value directly into caller-owned UTF-16 storage.</summary>
/// <typeparam name="T">The formatted value type.</typeparam>
internal delegate bool TextTryFormat<T>(
    in T value,
    Span<char> destination,
    out int charsWritten,
    ReadOnlySpan<char> format,
    IFormatProvider? provider);

/// <summary>Caches the immutable span-formatting capability for one closed value type.</summary>
/// <typeparam name="T">The formatted value type.</typeparam>
internal static class TextTryFormatCache<T>
{
    /// <summary>Gets the constrained formatter, or <see langword="null"/> when the type does not support span formatting.</summary>
    public static readonly TextTryFormat<T>? Formatter = Create();

    /// <summary>Creates a constrained generic delegate once for this closed value type.</summary>
    private static TextTryFormat<T>? Create()
    {
        if (!typeof(ISpanFormattable).IsAssignableFrom(typeof(T)))
            return null;

        var method = typeof(TextTryFormatCache<T>).GetMethod(
            nameof(Constrained),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return method.MakeGenericMethod(typeof(T)).CreateDelegate<TextTryFormat<T>>();
    }

    /// <summary>Invokes <see cref="ISpanFormattable.TryFormat"/> without boxing value types.</summary>
    private static bool Constrained<TValue>(
        in TValue value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
        where TValue : ISpanFormattable =>
        value.TryFormat(destination, out charsWritten, format, provider);
}
