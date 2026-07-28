namespace AlvorKit.Text;

/// <summary>Appends typed values without mutable process-wide formatter registration.</summary>
internal static class TextValueFormatter
{
    /// <summary>Appends one value, using direct text copies or a cached constrained span formatter.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Append<T>(TextBuffer buffer, in T value, ReadOnlySpan<char> format)
    {
        if (!typeof(T).IsValueType && value is null)
            return;

        if (typeof(T) == typeof(string))
        {
            buffer.Append(Unsafe.As<T, string?>(ref Unsafe.AsRef(in value)));
            return;
        }

        if (typeof(T) == typeof(StringBuilder))
        {
            var builder = Unsafe.As<T, StringBuilder?>(ref Unsafe.AsRef(in value));
            buffer.Append(builder!);
            return;
        }

        if (typeof(T) == typeof(ReadOnlyMemory<char>))
        {
            var memory = Unsafe.As<T, ReadOnlyMemory<char>>(ref Unsafe.AsRef(in value));
            buffer.Append(memory.Span);
            return;
        }

        var formatter = TextTryFormatCache<T>.Formatter;
        if (formatter != null)
        {
            AppendSpanFormatted(buffer, in value, format, formatter);
            return;
        }

        AppendFallback(buffer, value, format);
    }

    /// <summary>Retries a span formatter with retained growth until its destination is large enough.</summary>
    private static void AppendSpanFormatted<T>(
        TextBuffer buffer,
        in T value,
        ReadOnlySpan<char> format,
        TextTryFormat<T> formatter)
    {
        int minimumLength = 32;
        while (true)
        {
            var destination = buffer.GetWritableSpan(minimumLength);
            if (formatter(in value, destination, out int written, format, null))
            {
                buffer.Advance(written);
                return;
            }

            minimumLength = checked(Math.Max(32, destination.Length * 2));
        }
    }

    /// <summary>Uses allocating framework formatting only for values without a span-formatting contract.</summary>
    private static void AppendFallback<T>(TextBuffer buffer, T value, ReadOnlySpan<char> format)
    {
        if (value is null)
            return;

        if (value is IFormattable formattable)
        {
            buffer.Append(formattable.ToString(format.IsEmpty ? null : format.ToString(), null));
            return;
        }

        buffer.Append(value.ToString());
    }
}
