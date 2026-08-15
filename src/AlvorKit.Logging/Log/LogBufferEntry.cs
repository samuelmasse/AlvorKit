namespace AlvorKit;

/// <summary>Pairs entry metadata with its buffered formatted characters.</summary>
internal readonly record struct LogBufferEntry(LogEntry Entry, ReadOnlyMemory<char> Chars);
