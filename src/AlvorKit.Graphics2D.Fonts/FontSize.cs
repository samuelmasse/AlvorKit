namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Loads and caches glyphs for one pixel size of a font face.</summary>
public sealed unsafe class FontSize
{
    /// <summary>The FreeType binding used to load glyphs.</summary>
    private readonly Ft ft;

    /// <summary>The FreeType face whose size is selected before each glyph load.</summary>
    private readonly FontFace face;

    /// <summary>The atlas collection that stores rendered glyph bitmaps.</summary>
    private readonly FontAtlasList atlases;

    /// <summary>The requested pixel height.</summary>
    private readonly int size;

    /// <summary>The metrics captured after selecting this pixel size.</summary>
    private readonly FontSizeMetrics metrics;

    /// <summary>The cached glyph slots by Unicode scalar.</summary>
    private readonly Dictionary<Rune, FontGlyphSlot> slots = [];

    /// <summary>The cached horizontal kerning offset by paired FreeType glyph indices.</summary>
    private readonly Dictionary<ulong, float> kernings = [];

    /// <summary>Creates a font-size cache and captures size metrics.</summary>
    internal FontSize(Ft ft, FontFace face, FontAtlasList atlases, int size)
    {
        this.ft = ft;
        this.face = face;
        this.atlases = atlases;
        this.size = size;

        FontFreeType.Require(ft, nameof(Ft.SetPixelSizes), ft.SetPixelSizes(face.Pointer, 0, (uint)size));
        var ftMetrics = face.Pointer->Size->Metrics;
        metrics = new FontSizeMetrics(
            FontFreeType.Pixel26Dot6(ftMetrics.Ascender),
            FontFreeType.Pixel26Dot6(ftMetrics.Descender),
            FontFreeType.Pixel26Dot6(ftMetrics.Height));
    }

    /// <summary>Gets the requested pixel height.</summary>
    public int Size => size;

    /// <summary>Gets font-wide metrics for this pixel size.</summary>
    public FontSizeMetrics Metrics => metrics;

    /// <summary>Gets or creates an atlas slot for one Unicode scalar.</summary>
    public FontGlyphSlot GlyphSlot(Rune character)
    {
        if (slots.TryGetValue(character, out var cached))
            return cached;

        FontFreeType.Require(ft, nameof(Ft.SetPixelSizes), ft.SetPixelSizes(face.Pointer, 0, (uint)size));
        var glyphIndex = ft.GetCharIndex(face.Pointer, character);
        FontFreeType.Require(ft, nameof(Ft.LoadGlyph), ft.LoadGlyph(face.Pointer, glyphIndex, FtLoadFlags.Render));

        var slot = CreateGlyphSlot(character, *face.Pointer->Glyph);
        slot.GlyphIndex = glyphIndex;
        slots.Add(character, slot);
        return slot;
    }

    /// <summary>Gets the horizontal kerning adjustment between two glyph slots in pixels.</summary>
    internal float Kerning(FontGlyphSlot left, FontGlyphSlot right)
    {
        if (left.GlyphIndex == 0 || right.GlyphIndex == 0)
            return 0f;

        var key = ((ulong)left.GlyphIndex << 32) | right.GlyphIndex;
        if (kernings.TryGetValue(key, out var cached))
            return cached;

        FontFreeType.Require(
            ft,
            nameof(Ft.GetKerning),
            ft.GetKerning(face.Pointer, left.GlyphIndex, right.GlyphIndex, FtKerningMode.KerningDefault, out var kerning));
        var value = FontFreeType.Pixel26Dot6(kerning.X);
        kernings.Add(key, value);
        return value;
    }

    /// <summary>Creates a glyph and atlas slot from the current FreeType glyph slot.</summary>
    private FontGlyphSlot CreateGlyphSlot(Rune character, FtGlyphSlotRec glyphSlot)
    {
        var bitmap = glyphSlot.Bitmap;
        var width = checked((int)bitmap.Width);
        var height = checked((int)bitmap.Rows);
        var pixels = ReadPixels(bitmap, width, height);
        var glyph = new FontGlyph(
            character,
            size,
            (checked((uint)width), checked((uint)height)),
            (glyphSlot.BitmapLeft, glyphSlot.BitmapTop),
            FontFreeType.Pixel26Dot6(glyphSlot.Advance.X));
        return atlases.FindFitting(glyph).Add(glyph, pixels);
    }

    /// <summary>Reads a FreeType bitmap into bottom-up RGBA coverage pixels.</summary>
    private static Vec4u8[] ReadPixels(FtBitmap bitmap, int width, int height)
    {
        var pixels = new Vec4u8[width * height];
        if (bitmap.Buffer == 0)
            return pixels;

        for (var y = 0; y < height; y++)
        {
            var row = RowPointer(bitmap, y, height);
            for (var x = 0; x < width; x++)
            {
                var value = Marshal.ReadByte(row, x);
                pixels[((height - y - 1) * width) + x] = (byte.MaxValue, byte.MaxValue, byte.MaxValue, value);
            }
        }

        return pixels;
    }

    /// <summary>Returns a row pointer while respecting positive or negative FreeType pitch.</summary>
    private static nint RowPointer(FtBitmap bitmap, int y, int height)
    {
        var pitch = bitmap.Pitch;
        return pitch >= 0 ? bitmap.Buffer + y * pitch : bitmap.Buffer + (height - 1 - y) * -pitch;
    }
}
