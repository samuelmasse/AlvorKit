namespace AlvorKit.Logging;

/// <summary>Buffers entries written by one producer until the collector advances them.</summary>
internal sealed class LogBuffer
{
    /// <summary>Bounds the number of entries accepted before producers rotate buffers.</summary>
    private const int EntriesMax = 4096;
    /// <summary>Bounds the number of characters accepted before producers rotate buffers.</summary>
    private const int CharsMax = 65536;

    /// <summary>Stores buffered entry metadata and character slices.</summary>
    private LogBufferEntry[] entries = new LogBufferEntry[4];
    /// <summary>Owns formatted characters referenced by buffered entries.</summary>
    private char[] chars = new char[16];

    /// <summary>Tracks characters reserved by the producer.</summary>
    private int charWritten;
    /// <summary>Publishes the producer's completed entry count.</summary>
    private int written;
    /// <summary>Tracks entries consumed by the collector.</summary>
    private int read;

    /// <summary>Gets the remaining bounded character capacity.</summary>
    public int CharCapacity => CharsMax - charWritten;
    /// <summary>Gets the remaining bounded entry capacity.</summary>
    public int Capacity => EntriesMax - Volatile.Read(ref written);
    /// <summary>Gets whether the collector has consumed every published entry.</summary>
    public bool Synced => Volatile.Read(ref written) == Volatile.Read(ref read);

    /// <summary>Copies one formatted entry into the producer buffer.</summary>
    public void Write(LogEntry entry, ReadOnlySpan<char> text)
    {
        if (charWritten + text.Length >= chars.Length)
            Array.Resize(ref chars, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)(charWritten + text.Length)));

        var dst = new Memory<char>(chars, charWritten, text.Length);
        text.CopyTo(dst.Span);

        if (written >= entries.Length)
            Array.Resize(ref entries, entries.Length * 2);

        var entryIndex = written;
        entries[entryIndex] = new(entry, dst);

        charWritten += text.Length;
        Volatile.Write(ref written, entryIndex + 1);
    }

    /// <summary>Gets entries published since the last collector advance.</summary>
    public ReadOnlySpan<LogBufferEntry> Read()
    {
        int w = Volatile.Read(ref written);
        int start = Volatile.Read(ref read);
        return new ReadOnlySpan<LogBufferEntry>(entries, start, w - start);
    }

    /// <summary>Marks <paramref name="count"/> published entries as consumed.</summary>
    public void Advance(int count)
    {
        Volatile.Write(ref read, Volatile.Read(ref read) + count);
    }

    /// <summary>Resets a fully consumed buffer for producer reuse.</summary>
    public void Clear()
    {
        charWritten = 0;
        Volatile.Write(ref written, 0);
        Volatile.Write(ref read, 0);
    }
}
