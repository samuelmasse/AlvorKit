namespace AlvorKit.Text;

/// <summary>Optimizes the common one-argument, one-item composite format shape.</summary>
internal static class SingleCompositeTextWriter
{
    /// <summary>Appends through the simple-item fast path or the complete parser when required.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Append<T>(TextBuffer buffer, string format, in T argument)
    {
        if (!TryAppendSimple(buffer, format.AsSpan(), in argument))
            CompositeTextWriter.Append(buffer, format, in argument);
    }

    /// <summary>Attempts to append one unaligned item without invoking the complete composite parser.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryAppendSimple<T>(
        TextBuffer buffer,
        ReadOnlySpan<char> chars,
        in T argument)
    {
        int opening = chars.IndexOf('{');
        if (opening < 0)
        {
            if (chars.Contains('}'))
                return false;

            buffer.Append(chars);
            return true;
        }

        if (chars[..opening].Contains('}') || opening + 2 >= chars.Length || chars[opening + 1] != '0')
            return false;

        int position = opening + 2;
        int formatStart = position;
        int formatLength = 0;
        if (chars[position] == ':')
        {
            formatStart = ++position;
            int relativeClosing = chars[position..].IndexOf('}');
            if (relativeClosing < 0)
                return false;

            formatLength = relativeClosing;
            position += relativeClosing;
        }
        else if (chars[position] != '}')
        {
            return false;
        }

        var suffix = chars[(position + 1)..];
        if (suffix.ContainsAny('{', '}'))
            return false;

        if (opening > 0)
            buffer.Append(chars[..opening]);
        buffer.Append(in argument, chars.Slice(formatStart, formatLength));
        if (!suffix.IsEmpty)
            buffer.Append(suffix);
        return true;
    }
}
