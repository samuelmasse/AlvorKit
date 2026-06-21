namespace AlvorKit.Maths;

/// <summary>Provides shared parsing helpers for generated maths types.</summary>
internal static class MathsParseHelper
{
    /// <summary>Attempts to read a parenthesized character body.</summary>
    public static bool TryReadParenthesizedBody(ReadOnlySpan<char> source, out ReadOnlySpan<char> body)
    {
        source = source.Trim();
        if (source.Length < 2 || source[0] != '(' || source[^1] != ')')
        {
            body = default;
            return false;
        }

        body = source[1..^1].Trim();
        return true;
    }

    /// <summary>Attempts to read a parenthesized UTF-8 body.</summary>
    public static bool TryReadParenthesizedBody(ReadOnlySpan<byte> source, out ReadOnlySpan<byte> body)
    {
        source = MathsUtf8TextHelper.TrimAsciiWhitespace(source);
        if (source.Length < 2 || source[0] != (byte)'(' || source[^1] != (byte)')')
        {
            body = default;
            return false;
        }

        body = MathsUtf8TextHelper.TrimAsciiWhitespace(source[1..^1]);
        return true;
    }

    /// <summary>Attempts to parse one scalar or vector value from a character span.</summary>
    public static bool TryParseComponent<TValue>(
        ReadOnlySpan<char> source,
        IFormatProvider? formatProvider,
        [MaybeNullWhen(false)] out TValue value)
        where TValue : ISpanParsable<TValue> =>
        TValue.TryParse(source, formatProvider, out value);

    /// <summary>Attempts to parse one scalar or vector value from a UTF-8 span.</summary>
    public static bool TryParseComponent<TValue>(
        ReadOnlySpan<byte> source,
        IFormatProvider? formatProvider,
        [MaybeNullWhen(false)] out TValue value)
        where TValue : IUtf8SpanParsable<TValue> =>
        TValue.TryParse(source, formatProvider, out value);

    /// <summary>Attempts to parse a Boolean value from a character span.</summary>
    public static bool TryParseComponent(ReadOnlySpan<char> source, IFormatProvider? formatProvider, out bool value)
    {
        _ = formatProvider;
        if (source.Equals("True".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (source.Equals("False".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>Attempts to parse a Boolean value from a UTF-8 span.</summary>
    public static bool TryParseComponent(ReadOnlySpan<byte> source, IFormatProvider? formatProvider, out bool value)
    {
        _ = formatProvider;
        if (source.SequenceEqual("True"u8))
        {
            value = true;
            return true;
        }

        if (source.SequenceEqual("False"u8))
        {
            value = false;
            return true;
        }

        value = default;
        return false;
    }
}
