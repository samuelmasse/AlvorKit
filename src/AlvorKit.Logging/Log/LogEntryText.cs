namespace AlvorKit.Logging;

/// <summary>Couples one formatted entry buffer to its owning producer thread.</summary>
internal readonly struct LogEntryText(LogThread thread, TextBuffer buffer)
{
    /// <summary>Gets the reusable text buffer for composite formatting.</summary>
    public TextBuffer Buffer => buffer;

    /// <summary>Appends one character.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value) => buffer.Append(value);

    /// <summary>Appends a string when it is non-null.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? value) => buffer.Append(value);

    /// <summary>Appends a substring.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string value, int startIndex, int count) =>
        buffer.Append(value, startIndex, count);

    /// <summary>Appends a character span.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> value) => buffer.Append(value);

    /// <summary>Appends a typed value through the shared span-formatting path.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(in T value) => buffer.Append(in value);

    /// <summary>Commits the completed entry directly to its owning producer thread.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Commit(LogEntry entry) => thread.Add(entry, buffer.Span);
}
