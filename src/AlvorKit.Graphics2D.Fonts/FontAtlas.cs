namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Packs glyph bitmaps into one atlas texture and can repack them through a sprite batch.</summary>
internal sealed partial class FontAtlas : IDisposable
{
    /// <summary>The strict OpenGL layer used for atlas uploads and repacking.</summary>
    private readonly GlLayer gl;

    /// <summary>The sprite batch used to copy glyph rectangles during repacking.</summary>
    private readonly SpriteBatch batch;

    /// <summary>The framebuffer and scratch tablet used while repacking.</summary>
    private readonly FontBuffer buffer;

    /// <summary>The glyph slots currently assigned to this atlas.</summary>
    private readonly List<FontGlyphSlot> slots = [];

    /// <summary>The atlas tablet holding current glyph pixels.</summary>
    private FontTablet tablet;

    /// <summary>The skyline height used for each atlas x coordinate.</summary>
    private int[] used;

    /// <summary>The next preferred x coordinate for placement.</summary>
    private int cursor;

    /// <summary>Whether this atlas has already been repacked since the last glyph add.</summary>
    private bool packed;

    /// <summary>Whether a previous fitting attempt failed for this atlas.</summary>
    private bool full;

    /// <summary>Creates an empty atlas.</summary>
    internal FontAtlas(GlLayer gl, SpriteBatch batch, FontBuffer buffer)
    {
        this.gl = gl;
        this.batch = batch;
        this.buffer = buffer;
        tablet = new FontTablet(gl);
        used = new int[tablet.Size];
    }

    /// <summary>Gets the atlas tablet holding current glyph pixels.</summary>
    internal FontTablet Tablet => tablet;

    /// <summary>Gets whether this atlas has already been repacked since the last glyph add.</summary>
    internal bool Packed => packed;

    /// <summary>Gets a mutable reference tracking whether a previous fitting attempt failed.</summary>
    internal ref bool Full => ref full;

    /// <summary>Checks whether the glyph fits in the atlas using the current skyline.</summary>
    internal bool Fits(FontGlyph glyph)
    {
        var (_, y) = NextSlot(glyph);
        return y + checked((int)glyph.Box.Y) <= tablet.Size;
    }

    /// <summary>Adds one rendered glyph bitmap to the atlas.</summary>
    internal FontGlyphSlot Add(FontGlyph glyph, ReadOnlySpan<Vec4u8> pixels)
    {
        var (x, y) = NextSlot(glyph);
        Upload(glyph, pixels, x, y);
        Advance(glyph, x, y);
        packed = false;

        var slot = new FontGlyphSlot(glyph, tablet.Texture, (checked((uint)x), checked((uint)y)));
        slots.Add(slot);
        return slot;
    }

    /// <summary>Deletes the current atlas tablet.</summary>
    public void Dispose() => tablet.Dispose();
}
