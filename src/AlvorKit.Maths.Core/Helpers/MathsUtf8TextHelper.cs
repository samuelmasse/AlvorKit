namespace AlvorKit.Maths;

/// <summary>Provides ASCII whitespace helpers for generated UTF-8 maths parsing.</summary>
internal static class MathsUtf8TextHelper
{
    /// <summary>Trims ASCII whitespace from both ends of a UTF-8 span.</summary>
    public static ReadOnlySpan<byte> TrimAsciiWhitespace(ReadOnlySpan<byte> source) =>
        TrimEndAsciiWhitespace(TrimStartAsciiWhitespace(source));

    /// <summary>Trims ASCII whitespace from the start of a UTF-8 span.</summary>
    public static ReadOnlySpan<byte> TrimStartAsciiWhitespace(ReadOnlySpan<byte> source)
    {
        while (!source.IsEmpty && IsAsciiWhitespace(source[0]))
            source = source[1..];

        return source;
    }

    /// <summary>Trims ASCII whitespace from the end of a UTF-8 span.</summary>
    public static ReadOnlySpan<byte> TrimEndAsciiWhitespace(ReadOnlySpan<byte> source)
    {
        while (!source.IsEmpty && IsAsciiWhitespace(source[^1]))
            source = source[..^1];

        return source;
    }

    /// <summary>Returns whether a byte is ASCII whitespace.</summary>
    public static bool IsAsciiWhitespace(byte value) => value is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n';
}
