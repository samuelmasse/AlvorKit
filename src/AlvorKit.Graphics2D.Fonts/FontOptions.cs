namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Describes the face source and index used when opening a font.</summary>
public class FontOptions
{
    /// <summary>Gets the filesystem path for a file-backed font face.</summary>
    public string? File { get; init; }

    /// <summary>Gets caller-owned bytes to copy into native memory for a memory-backed font face.</summary>
    public ReadOnlyMemory<byte>? Data { get; init; }

    /// <summary>Gets the face index inside a font collection.</summary>
    public int Index { get; init; }
}
