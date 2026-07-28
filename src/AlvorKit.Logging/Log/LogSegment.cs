namespace AlvorKit.Logging;

/// <summary>Stores an ordered, reusable batch of collected log entries.</summary>
internal sealed class LogSegment
{
    /// <summary>Stores entry metadata and character slices.</summary>
    private LogBufferEntry[] entries = new LogBufferEntry[4096];
    /// <summary>Owns the formatted characters referenced by entries.</summary>
    private char[] chars = new char[65536];
    /// <summary>Tracks the next free entry slot.</summary>
    private int entryIndex;
    /// <summary>Tracks the next free character slot.</summary>
    private int charIndex;
    /// <summary>Tracks whether consumers may advance beyond this segment.</summary>
    private bool closed;

    /// <summary>Gets whether no more entries will be appended before reuse.</summary>
    public bool Closed => closed;
    /// <summary>Gets the number of entries that fit before growth is required.</summary>
    public int Capacity => entries.Length - entryIndex;
    /// <summary>Gets the number of characters that fit before growth is required.</summary>
    public int CharCapacity => chars.Length - charIndex;
    /// <summary>Gets every entry currently stored in this segment.</summary>
    public ReadOnlySpan<LogBufferEntry> Entries => new(entries, 0, entryIndex);

    /// <summary>Copies one collected entry into the segment's owned storage.</summary>
    public void Add(LogBufferEntry entry)
    {
        if (entryIndex == entries.Length)
            Array.Resize(ref entries, entries.Length * 2);
        if (charIndex + entry.Chars.Length > chars.Length)
        {
            var required = (uint)(charIndex + entry.Chars.Length);
            Array.Resize(ref chars, (int)BitOperations.RoundUpToPowerOf2(required));
        }

        var dst = new Memory<char>(chars, charIndex, entry.Chars.Length);
        entry.Chars.CopyTo(dst);

        entries[entryIndex] = new(entry.Entry, dst);

        charIndex += entry.Chars.Length;
        entryIndex++;
    }

    /// <summary>Clears the segment for reuse without releasing its arrays.</summary>
    public void Reset()
    {
        entryIndex = 0;
        charIndex = 0;
        closed = false;
    }

    /// <summary>Marks the segment complete for consumers.</summary>
    public void Close() => closed = true;
}
