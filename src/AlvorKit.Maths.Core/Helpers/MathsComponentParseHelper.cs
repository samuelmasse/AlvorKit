namespace AlvorKit.Maths;

/// <summary>Provides shared comma-separated component readers for generated maths types.</summary>
internal static class MathsComponentParseHelper
{
    /// <summary>Attempts to read and parse the next comma-separated value from a character span.</summary>
    public static bool TryReadNextComponent<TValue>(
        ref ReadOnlySpan<char> source,
        IFormatProvider? formatProvider,
        [MaybeNullWhen(false)] out TValue value)
        where TValue : ISpanParsable<TValue>
    {
        value = default;
        var separator = MathsTextSeparatorHelper.FindFlatSeparator(source);
        if (separator < 0 || !MathsParseHelper.TryParseComponent(source[..separator].Trim(), formatProvider, out value))
            return false;

        source = source[(separator + 1)..].TrimStart();
        return true;
    }

    /// <summary>Attempts to read and parse the next comma-separated value from a UTF-8 span.</summary>
    public static bool TryReadNextComponent<TValue>(
        ref ReadOnlySpan<byte> source,
        IFormatProvider? formatProvider,
        [MaybeNullWhen(false)] out TValue value)
        where TValue : IUtf8SpanParsable<TValue>
    {
        value = default;
        var separator = MathsTextSeparatorHelper.FindFlatSeparator(source);
        if (separator < 0 ||
            !MathsParseHelper.TryParseComponent(MathsUtf8TextHelper.TrimAsciiWhitespace(source[..separator]), formatProvider, out value))
        {
            return false;
        }

        source = MathsUtf8TextHelper.TrimStartAsciiWhitespace(source[(separator + 1)..]);
        return true;
    }

    /// <summary>Attempts to read and parse the next comma-separated Boolean value from a character span.</summary>
    public static bool TryReadNextComponent(ref ReadOnlySpan<char> source, IFormatProvider? formatProvider, out bool value)
    {
        value = default;
        var separator = MathsTextSeparatorHelper.FindFlatSeparator(source);
        if (separator < 0 || !MathsParseHelper.TryParseComponent(source[..separator].Trim(), formatProvider, out value))
            return false;

        source = source[(separator + 1)..].TrimStart();
        return true;
    }

    /// <summary>Attempts to read and parse the next comma-separated Boolean value from a UTF-8 span.</summary>
    public static bool TryReadNextComponent(ref ReadOnlySpan<byte> source, IFormatProvider? formatProvider, out bool value)
    {
        value = default;
        var separator = MathsTextSeparatorHelper.FindFlatSeparator(source);
        if (separator < 0 ||
            !MathsParseHelper.TryParseComponent(MathsUtf8TextHelper.TrimAsciiWhitespace(source[..separator]), formatProvider, out value))
        {
            return false;
        }

        source = MathsUtf8TextHelper.TrimStartAsciiWhitespace(source[(separator + 1)..]);
        return true;
    }

    /// <summary>Attempts to read and parse the next top-level parenthesized value from a character span.</summary>
    public static bool TryReadNextTopLevelComponent<TValue>(
        ref ReadOnlySpan<char> source,
        IFormatProvider? formatProvider,
        [MaybeNullWhen(false)] out TValue value)
        where TValue : ISpanParsable<TValue>
    {
        value = default;
        var separator = MathsTextSeparatorHelper.FindTopLevelSeparator(source);
        if (separator < 0 || !MathsParseHelper.TryParseComponent(source[..separator].Trim(), formatProvider, out value))
            return false;

        source = source[(separator + 1)..].TrimStart();
        return true;
    }

    /// <summary>Attempts to read and parse the next top-level parenthesized value from a UTF-8 span.</summary>
    public static bool TryReadNextTopLevelComponent<TValue>(
        ref ReadOnlySpan<byte> source,
        IFormatProvider? formatProvider,
        [MaybeNullWhen(false)] out TValue value)
        where TValue : IUtf8SpanParsable<TValue>
    {
        value = default;
        var separator = MathsTextSeparatorHelper.FindTopLevelSeparator(source);
        if (separator < 0 ||
            !MathsParseHelper.TryParseComponent(MathsUtf8TextHelper.TrimAsciiWhitespace(source[..separator]), formatProvider, out value))
        {
            return false;
        }

        source = MathsUtf8TextHelper.TrimStartAsciiWhitespace(source[(separator + 1)..]);
        return true;
    }
}
