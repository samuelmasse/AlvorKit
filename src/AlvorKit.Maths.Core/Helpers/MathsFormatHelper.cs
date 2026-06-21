namespace AlvorKit.Maths;

/// <summary>Provides shared formatting helpers for generated maths types.</summary>
internal static class MathsFormatHelper
{
    /// <summary>Returns a formatted string using a stack-backed fast path caller and a growing fallback buffer.</summary>
    public static string ToStringSlow<TValue>(TValue value, string? format, IFormatProvider? formatProvider, int initialLength)
        where TValue : ISpanFormattable
    {
        var length = initialLength;
        while (true)
        {
            var destination = new char[length];
            if (value.TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider))
                return new string(destination, 0, charsWritten);

            length *= 2;
        }
    }

    /// <summary>Attempts to append literal text to a remaining character span.</summary>
    public static bool TryAppend(ReadOnlySpan<char> value, ref Span<char> destination, ref int charsWritten)
    {
        if (!value.TryCopyTo(destination))
            return false;

        destination = destination[value.Length..];
        charsWritten += value.Length;
        return true;
    }

    /// <summary>Attempts to append literal UTF-8 text to a remaining byte span.</summary>
    public static bool TryAppend(ReadOnlySpan<byte> value, ref Span<byte> destination, ref int bytesWritten)
    {
        if (!value.TryCopyTo(destination))
            return false;

        destination = destination[value.Length..];
        bytesWritten += value.Length;
        return true;
    }

    /// <summary>Attempts to append one formattable value to a remaining character span.</summary>
    public static bool TryAppendFormatted<TValue>(
        TValue value,
        ref Span<char> destination,
        ref int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider)
        where TValue : ISpanFormattable
    {
        if (!value.TryFormat(destination, out var componentCharsWritten, format, formatProvider))
            return false;

        destination = destination[componentCharsWritten..];
        charsWritten += componentCharsWritten;
        return true;
    }

    /// <summary>Attempts to append one formattable value to a remaining UTF-8 byte span.</summary>
    public static bool TryAppendFormatted<TValue>(
        TValue value,
        ref Span<byte> destination,
        ref int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider)
        where TValue : IUtf8SpanFormattable
    {
        if (!value.TryFormat(destination, out var componentBytesWritten, format, formatProvider))
            return false;

        destination = destination[componentBytesWritten..];
        bytesWritten += componentBytesWritten;
        return true;
    }

    /// <summary>Attempts to append a Boolean value to a remaining character span.</summary>
    public static bool TryAppendFormatted(
        bool value,
        ref Span<char> destination,
        ref int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider)
    {
        _ = format;
        _ = formatProvider;

        return TryAppend(value ? "True".AsSpan() : "False".AsSpan(), ref destination, ref charsWritten);
    }

    /// <summary>Attempts to append a Boolean value to a remaining UTF-8 byte span.</summary>
    public static bool TryAppendFormatted(
        bool value,
        ref Span<byte> destination,
        ref int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider)
    {
        _ = format;
        _ = formatProvider;

        return TryAppend(value ? "True"u8 : "False"u8, ref destination, ref bytesWritten);
    }

    /// <summary>Reports a failed formatting operation and satisfies the written-count contract.</summary>
    public static bool Fail(out int written)
    {
        written = 0;
        return false;
    }
}
