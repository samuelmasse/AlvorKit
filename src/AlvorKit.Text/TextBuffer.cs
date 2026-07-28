namespace AlvorKit.Text;

/// <summary>Owns reusable UTF-16 storage for allocation-free steady-state formatting.</summary>
internal sealed class TextBuffer
{
    /// <summary>Stores the formatted characters.</summary>
    private char[] chars;
    /// <summary>Tracks the number of populated characters.</summary>
    private int length;

    /// <summary>Creates a reusable buffer with the requested initial capacity.</summary>
    public TextBuffer(int initialCapacity = 256)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCapacity);
        chars = new char[initialCapacity];
    }

    /// <summary>Gets the number of populated characters.</summary>
    public int Length => length;
    /// <summary>Gets the populated characters.</summary>
    public ReadOnlySpan<char> Span => chars.AsSpan(0, length);

    /// <summary>Clears the contents while retaining allocated storage.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => length = 0;

    /// <summary>Appends one character.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value)
    {
        EnsureFreeCapacity(1);
        chars[length++] = value;
    }

    /// <summary>Appends a string when it is non-null.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? value)
    {
        if (value != null)
            Append(value.AsSpan());
    }

    /// <summary>Appends a substring.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string value, int startIndex, int count) =>
        Append(value.AsSpan(startIndex, count));

    /// <summary>Appends a character span.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> value)
    {
        EnsureFreeCapacity(value.Length);
        value.CopyTo(chars.AsSpan(length));
        length += value.Length;
    }

    /// <summary>Appends the contents of a string builder.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(StringBuilder value)
    {
        EnsureFreeCapacity(value.Length);
        value.CopyTo(0, chars.AsSpan(length), value.Length);
        length += value.Length;
    }

    /// <summary>Appends a typed value using its span formatter when available.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(in T value) => TextValueFormatter.Append(this, in value, default);

    /// <summary>Appends a typed value using its span formatter and format specifier.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(in T value, ReadOnlySpan<char> format) =>
        TextValueFormatter.Append(this, in value, format);

    /// <summary>Gets writable storage with at least <paramref name="minimumLength"/> free characters.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> GetWritableSpan(int minimumLength)
    {
        EnsureFreeCapacity(minimumLength);
        return chars.AsSpan(length);
    }

    /// <summary>Commits characters written directly into the writable span.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count) => length += count;

    /// <summary>Applies composite-format alignment to the value starting at <paramref name="start"/>.</summary>
    public void Align(int start, int alignment)
    {
        int valueLength = length - start;
        int width = Math.Abs(alignment);
        int padding = width - valueLength;
        if (padding <= 0)
            return;

        EnsureFreeCapacity(padding);
        if (alignment > 0)
        {
            chars.AsSpan(start, valueLength).CopyTo(chars.AsSpan(start + padding));
            chars.AsSpan(start, padding).Fill(' ');
            length += padding;
            return;
        }

        chars.AsSpan(length, padding).Fill(' ');
        length += padding;
    }

    /// <summary>Ensures the requested free capacity, retaining the expanded array for later writes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureFreeCapacity(int required)
    {
        if (required <= chars.Length - length)
            return;

        int requiredCapacity = checked(length + required);
        int expandedCapacity = Math.Max(requiredCapacity, Math.Max(32, chars.Length * 2));
        Array.Resize(ref chars, expandedCapacity);
    }
}
