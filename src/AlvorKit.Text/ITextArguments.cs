namespace AlvorKit.Text;

/// <summary>Provides indexed, strongly typed arguments to the composite-format parser.</summary>
internal interface ITextArguments
{
    /// <summary>Appends the indexed argument with the requested format specifier.</summary>
    void Append(TextBuffer buffer, int index, ReadOnlySpan<char> format);
}
