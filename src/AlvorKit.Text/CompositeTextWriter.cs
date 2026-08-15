namespace AlvorKit;

/// <summary>Parses standard composite-format items directly into reusable storage.</summary>
internal static class CompositeTextWriter
{
    /// <summary>Appends a one-argument composite format through its direct typed hot path.</summary>
    public static void Append<T>(TextBuffer buffer, string format, in T argument)
    {
        var chars = format.AsSpan();
        int literalStart = 0;
        int index = 0;

        while (index < chars.Length)
        {
            char current = chars[index];
            if (current == '{')
            {
                if (index + 1 < chars.Length && chars[index + 1] == '{')
                {
                    AppendEscaped(buffer, chars, ref index, ref literalStart, '{');
                    continue;
                }

                buffer.Append(chars[literalStart..index]);
                AppendSingleItem(buffer, chars, ref index, in argument);
                literalStart = index;
                continue;
            }

            if (current == '}')
            {
                if (index + 1 < chars.Length && chars[index + 1] == '}')
                {
                    AppendEscaped(buffer, chars, ref index, ref literalStart, '}');
                    continue;
                }

                throw CompositeTextFormatException.Create();
            }

            index++;
        }

        buffer.Append(chars[literalStart..]);
    }

    /// <summary>Appends a composite format using a strongly typed argument pack.</summary>
    public static void AppendMany<TArguments>(
        TextBuffer buffer,
        string format,
        in TArguments arguments)
        where TArguments : struct, ITextArguments
    {
        var chars = format.AsSpan();
        int literalStart = 0;
        int index = 0;

        while (index < chars.Length)
        {
            char current = chars[index];
            if (current == '{')
            {
                if (index + 1 < chars.Length && chars[index + 1] == '{')
                {
                    AppendEscaped(buffer, chars, ref index, ref literalStart, '{');
                    continue;
                }

                buffer.Append(chars[literalStart..index]);
                AppendItem(buffer, chars, ref index, in arguments);
                literalStart = index;
                continue;
            }

            if (current == '}')
            {
                if (index + 1 < chars.Length && chars[index + 1] == '}')
                {
                    AppendEscaped(buffer, chars, ref index, ref literalStart, '}');
                    continue;
                }

                throw CompositeTextFormatException.Create();
            }

            index++;
        }

        buffer.Append(chars[literalStart..]);
    }

    /// <summary>Appends an escaped brace and advances the literal scan.</summary>
    private static void AppendEscaped(
        TextBuffer buffer,
        ReadOnlySpan<char> chars,
        ref int index,
        ref int literalStart,
        char brace)
    {
        buffer.Append(chars[literalStart..index]);
        buffer.Append(brace);
        index += 2;
        literalStart = index;
    }

    /// <summary>Parses and appends one format item beginning at an opening brace.</summary>
    private static void AppendItem<TArguments>(
        TextBuffer buffer,
        ReadOnlySpan<char> chars,
        ref int position,
        in TArguments arguments)
        where TArguments : struct, ITextArguments
    {
        ParseItem(chars, ref position, out int argumentIndex, out int alignment, out int formatStart, out int formatLength);

        int valueStart = buffer.Length;
        arguments.Append(buffer, argumentIndex, chars.Slice(formatStart, formatLength));
        if (alignment != 0)
            buffer.Align(valueStart, alignment);
    }

    /// <summary>Parses and appends one item through the direct one-argument path.</summary>
    private static void AppendSingleItem<T>(
        TextBuffer buffer,
        ReadOnlySpan<char> chars,
        ref int position,
        in T argument)
    {
        ParseItem(chars, ref position, out int argumentIndex, out int alignment, out int formatStart, out int formatLength);
        if (argumentIndex != 0)
            throw CompositeTextFormatException.Create();

        int valueStart = buffer.Length;
        buffer.Append(in argument, chars.Slice(formatStart, formatLength));
        if (alignment != 0)
            buffer.Align(valueStart, alignment);
    }

    /// <summary>Parses one format item and advances beyond its closing brace.</summary>
    private static void ParseItem(
        ReadOnlySpan<char> chars,
        ref int position,
        out int argumentIndex,
        out int alignment,
        out int formatStart,
        out int formatLength)
    {
        position++;
        argumentIndex = ParseUnsigned(chars, ref position);
        SkipSpaces(chars, ref position);

        alignment = 0;
        if (Read(chars, position) == ',')
            alignment = ParseAlignment(chars, ref position);

        formatStart = position;
        formatLength = 0;
        if (Read(chars, position) == ':')
        {
            formatStart = ++position;
            while (position < chars.Length && chars[position] != '}')
                position++;
            if (position >= chars.Length)
                throw CompositeTextFormatException.Create();

            formatLength = position - formatStart;
        }
        else if (Read(chars, position) != '}')
        {
            throw CompositeTextFormatException.Create();
        }

        position++;
    }

    /// <summary>Parses a positive argument index.</summary>
    private static int ParseUnsigned(ReadOnlySpan<char> chars, ref int position)
    {
        if (!IsDigit(Read(chars, position)))
            throw CompositeTextFormatException.Create();

        int value = 0;
        do
        {
            int digit = chars[position++] - '0';
            if (value > (int.MaxValue - digit) / 10)
                throw CompositeTextFormatException.Create();
            value = value * 10 + digit;
        }
        while (IsDigit(Read(chars, position)));

        return value;
    }

    /// <summary>Parses a signed alignment component after its comma.</summary>
    private static int ParseAlignment(ReadOnlySpan<char> chars, ref int position)
    {
        position++;
        SkipSpaces(chars, ref position);
        bool negative = Read(chars, position) == '-';
        if (negative || Read(chars, position) == '+')
            position++;

        int width = ParseUnsigned(chars, ref position);
        SkipSpaces(chars, ref position);
        return negative ? -width : width;
    }

    /// <summary>Advances over ASCII spaces accepted by composite alignment syntax.</summary>
    private static void SkipSpaces(ReadOnlySpan<char> chars, ref int position)
    {
        while (Read(chars, position) == ' ')
            position++;
    }

    /// <summary>Reads one character or a null sentinel beyond the source span.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char Read(ReadOnlySpan<char> chars, int position) =>
        (uint)position < (uint)chars.Length ? chars[position] : '\0';

    /// <summary>Gets whether a character is an ASCII decimal digit.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigit(char value) => (uint)(value - '0') <= 9;
}
