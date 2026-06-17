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

    /// <summary>The FreeType driver used to load glyphs.</summary>
    private readonly FontDriver driver;

    /// <summary>Creates a font from a file path.</summary>
    public Font(FontContext context, string file) : this(context, new FontOptions { File = file }) { }

    /// <summary>Creates a font from managed bytes that remain pinned while the font is alive.</summary>
    public Font(FontContext context, byte[] data) : this(context, new FontOptions { Data = data }) { }

    /// <summary>Creates a font from explicit options.</summary>
    public Font(FontContext context, FontOptions options)
    {
        driver = context.Driver;
        face = new FontFace(driver, context.Library, options);
        atlases = new FontAtlasList(context.GL, context.Batch, context.Buffer);
    }

    /// <summary>Gets the current atlas textures used by this font.</summary>
    public ReadOnlySpan<Texture> Textures => atlases.Textures;

    /// <summary>Gets or creates a font-size cache for the requested pixel height.</summary>
    public FontSize Size(int size)
    {
        if (sizes.TryGetValue(size, out var cached))
            return cached;

        var created = new FontSize(driver, face, atlases, size);
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
