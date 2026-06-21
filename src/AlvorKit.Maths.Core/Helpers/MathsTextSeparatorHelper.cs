namespace AlvorKit.Maths;

/// <summary>Finds comma separators in generated maths text representations.</summary>
internal static class MathsTextSeparatorHelper
{
    /// <summary>Finds the next comma followed by whitespace.</summary>
    public static int FindFlatSeparator(ReadOnlySpan<char> source)
    {
        for (var index = 0; index < source.Length - 1; index++)
        {
            if (source[index] == ',' && char.IsWhiteSpace(source[index + 1]))
                return index;
        }

        return -1;
    }

    /// <summary>Finds the next UTF-8 comma followed by ASCII whitespace.</summary>
    public static int FindFlatSeparator(ReadOnlySpan<byte> source)
    {
        for (var index = 0; index < source.Length - 1; index++)
        {
            if (source[index] == (byte)',' && MathsUtf8TextHelper.IsAsciiWhitespace(source[index + 1]))
                return index;
        }

        return -1;
    }

    /// <summary>Finds the next top-level comma followed by whitespace.</summary>
    public static int FindTopLevelSeparator(ReadOnlySpan<char> source)
    {
        var depth = 0;
        for (var index = 0; index < source.Length - 1; index++)
        {
            if (source[index] == '(')
                depth++;
            else if (source[index] == ')')
                depth--;
            else if (depth == 0 && source[index] == ',' && char.IsWhiteSpace(source[index + 1]))
                return index;
        }

        return -1;
    }

    /// <summary>Finds the next top-level UTF-8 comma followed by ASCII whitespace.</summary>
    public static int FindTopLevelSeparator(ReadOnlySpan<byte> source)
    {
        var depth = 0;
        for (var index = 0; index < source.Length - 1; index++)
        {
            if (source[index] == (byte)'(')
                depth++;
            else if (source[index] == (byte)')')
                depth--;
            else if (depth == 0 && source[index] == (byte)',' && MathsUtf8TextHelper.IsAsciiWhitespace(source[index + 1]))
                return index;
        }

        return -1;
    }
}
