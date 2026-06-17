namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns a FreeType face and its glyph atlases for all requested sizes.</summary>
public sealed class Font : IDisposable
{
    /// <summary>The FreeType face lifetime owner.</summary>
    private readonly FontFace face;

    /// <summary>The atlas collection shared by all sizes of this font.</summary>
    private readonly FontAtlasList atlases;

    /// <summary>The cached font sizes by pixel height.</summary>
    private readonly Dictionary<int, FontSize> sizes = [];

    /// <summary>The FreeType binding used to load glyphs.</summary>
    private readonly Ft ft;

    /// <summary>Creates a font from a file path.</summary>
    public Font(FontContext context, string file) : this(context, new FontOptions { File = file }) { }

    /// <summary>Creates a font from caller-owned bytes copied into native memory for the font lifetime.</summary>
    public Font(FontContext context, ReadOnlySpan<byte> data) : this(context, new FontFace(context.FreeType, context.Library, data, 0)) { }

    /// <summary>Creates a font from explicit options.</summary>
    public Font(FontContext context, FontOptions options) : this(context, new FontFace(context.FreeType, context.Library, options)) { }

    /// <summary>Creates a font around an opened face.</summary>
    private Font(FontContext context, FontFace face)
    {
        ft = context.FreeType;
        this.face = face;
        atlases = new FontAtlasList(context.GL, context.Batch, context.Buffer);
    }

    /// <summary>Gets the current atlas textures used by this font.</summary>
    public ReadOnlySpan<Texture> Textures => atlases.Textures;

    /// <summary>Gets or creates a font-size cache for the requested pixel height.</summary>
    public FontSize Size(int size)
    {
        if (sizes.TryGetValue(size, out var cached))
            return cached;

        var created = new FontSize(ft, face, atlases, size);
        sizes.Add(size, created);
        return created;
    }

    /// <summary>Repack atlases that became full after glyph insertion attempts.</summary>
    public void Pack() => atlases.Pack();

    /// <summary>Repack every atlas with pending, unpacked placement.</summary>
    public void ForcePack() => atlases.ForcePack();

    /// <summary>Deletes atlas textures and releases the FreeType face.</summary>
    public void Dispose()
    {
        atlases.Dispose();
        face.Dispose();
    }
}
