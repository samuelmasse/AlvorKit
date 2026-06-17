namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns all atlas textures for one font and chooses a fitting atlas for new glyphs.</summary>
internal sealed class FontAtlasList : IDisposable
{
    /// <summary>The strict OpenGL layer used for new atlases.</summary>
    private readonly GlLayer gl;

    /// <summary>The sprite batch used when repacking atlas textures.</summary>
    private readonly SpriteBatch batch;

    /// <summary>The framebuffer and scratch texture used while repacking atlases.</summary>
    private readonly FontBuffer buffer;

    /// <summary>The atlas list.</summary>
    private readonly List<FontAtlas> atlases;

    /// <summary>The reusable texture view buffer.</summary>
    private Texture[] textures = [];

    /// <summary>Creates the first empty atlas.</summary>
    internal FontAtlasList(GlLayer gl, SpriteBatch batch, FontBuffer buffer)
    {
        this.gl = gl;
        this.batch = batch;
        this.buffer = buffer;
        atlases = [new FontAtlas(gl, batch, buffer)];
    }

    /// <summary>Gets the current atlas textures.</summary>
    public ReadOnlySpan<Texture> Textures
    {
        get
        {
            if (textures.Length < atlases.Count)
                Array.Resize(ref textures, atlases.Count * 2);

            for (var i = 0; i < atlases.Count; i++)
                textures[i] = atlases[i].Tablet.Texture;

            return new ReadOnlySpan<Texture>(textures, 0, atlases.Count);
        }
    }

    /// <summary>Repack atlases that were marked full by a failed fitting attempt.</summary>
    internal void Pack()
    {
        foreach (var atlas in atlases)
            if (atlas.Full && !atlas.Packed)
                atlas.Pack();
    }

    /// <summary>Repack every atlas with pending, unpacked placement.</summary>
    internal void ForcePack()
    {
        foreach (var atlas in atlases)
            if (!atlas.Packed)
                atlas.Pack();
    }

    /// <summary>Finds an atlas that fits the glyph, creating a new atlas when needed.</summary>
    internal FontAtlas FindFitting(FontGlyph glyph)
    {
        foreach (var atlas in atlases)
        {
            if (atlas.Fits(glyph))
                return atlas;

            atlas.Full = true;
        }

        var newAtlas = new FontAtlas(gl, batch, buffer);
        atlases.Add(newAtlas);
        return newAtlas;
    }

    /// <summary>Deletes all atlas textures.</summary>
    public void Dispose()
    {
        foreach (var atlas in atlases)
            atlas.Dispose();
    }
}
