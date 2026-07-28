namespace AlvorKit.Logging;

/// <summary>Writes published log segments to a text destination.</summary>
/// <param name="logStream">The ordered segment source.</param>
/// <param name="output">The destination writer.</param>
internal sealed class LogConsole(LogStream logStream, TextWriter output)
{
    /// <summary>Caches the platform newline without allocating per entry.</summary>
    private readonly char[] newline = Environment.NewLine.ToCharArray();
    /// <summary>Batches characters before writing to the destination.</summary>
    private char[] buffer = new char[4096];

    /// <summary>Tracks whether ANSI severity colors are enabled.</summary>
    private bool useColor = ReferenceEquals(output, Console.Out);
    /// <summary>Tracks the next free character in the output buffer.</summary>
    private int bufferIndex;
    /// <summary>Tracks the next segment to consume.</summary>
    private long segmentIndex;
    /// <summary>Tracks the next entry within the current segment.</summary>
    private int segmentInnerIndex;
    /// <summary>Counts segments overwritten before this consumer observed them.</summary>
    private long segmentDroppedCount;
    /// <summary>Counts entries written by this consumer.</summary>
    private long logCount;

    /// <summary>Gets or sets whether entries use ANSI severity colors.</summary>
    public bool UseColor
    {
        get => useColor;
        set => useColor = value;
    }

    /// <summary>Gets the index of the segment currently being consumed.</summary>
    public long SegmentIndex => segmentIndex;
    /// <summary>Gets the number of segments overwritten before consumption.</summary>
    public long SegmentDroppedCount => segmentDroppedCount;
    /// <summary>Gets the number of entries written to the destination.</summary>
    public long LogCount => logCount;

    /// <summary>Writes every currently available segment entry.</summary>
    public void Print()
    {
        long nextIndex = logStream.SegmentIndex;
        long oldestIndex = Math.Max(0, nextIndex - logStream.Segments.Length + 1);

        if (segmentIndex < oldestIndex)
        {
            segmentDroppedCount += oldestIndex - segmentIndex;
            segmentIndex = oldestIndex;
            segmentInnerIndex = 0;
        }

        while (segmentIndex <= nextIndex)
        {
            var segment = logStream.Segments[(int)(segmentIndex % logStream.Segments.Length)];
            var closed = segment.Closed;
            var entries = segment.Entries[segmentInnerIndex..];

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var color = entry.Entry.Level switch
                {
                    LogLevel.Fatal => "\x1b[1;37;41m",
                    LogLevel.Error => "\x1b[1;91;49m",
                    LogLevel.Warn => "\x1b[1;93;49m",
                    LogLevel.Info => "\x1b[1;37;49m",
                    LogLevel.Debug => "\x1b[0;37;49m",
                    LogLevel.Trace => "\x1b[0;90;49m",
                    _ => ""
                };

                WriteEntry(entry.Chars.Span, color);
            }

            logCount += entries.Length;
            segmentInnerIndex += entries.Length;

            if (!closed)
                break;

            segmentIndex++;
            segmentInnerIndex = 0;
        }

        if (bufferIndex > 0)
        {
            output.Write(buffer.AsSpan()[..bufferIndex]);
            bufferIndex = 0;
        }
    }

    /// <summary>Splits and writes one possibly multiline entry.</summary>
    private void WriteEntry(ReadOnlySpan<char> text, ReadOnlySpan<char> color)
    {
        int start = 0;
        while (start < text.Length)
        {
            int offset = text[start..].IndexOf('\n');
            int end = offset < 0 ? text.Length : start + offset;
            var line = text[start..end];
            if (!line.IsEmpty && line[^1] == '\r')
                line = line[..^1];

            WriteLine(line, color);
            if (offset < 0)
                return;

            start = end + 1;
        }

        if (text.IsEmpty)
            WriteLine([], color);
    }

    /// <summary>Writes one line with optional ANSI color framing.</summary>
    private void WriteLine(ReadOnlySpan<char> line, ReadOnlySpan<char> color)
    {
        if (!line.IsEmpty)
        {
            if (useColor && !color.IsEmpty)
                Write(color);
            Write(line);
            if (useColor && !color.IsEmpty)
                Write("\x1b[0m");
        }

        Write(newline);
    }

    /// <summary>Appends characters to the reusable output buffer.</summary>
    private void Write(ReadOnlySpan<char> text)
    {
        if (bufferIndex + text.Length >= buffer.Length)
        {
            var required = (uint)(bufferIndex + text.Length);
            Array.Resize(ref buffer, (int)BitOperations.RoundUpToPowerOf2(required));
        }

        text.CopyTo(new(buffer, bufferIndex, text.Length));
        bufferIndex += text.Length;
    }
}
