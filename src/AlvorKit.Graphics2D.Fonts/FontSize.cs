namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Loads and caches glyphs for one pixel size of a font face.</summary>
public sealed unsafe class FontSize
{
    /// <summary>The FreeType driver used to load glyphs.</summary>
    private readonly FontDriver driver;

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

    /// <summary>Creates a font-size cache and captures size metrics.</summary>
    internal FontSize(FontDriver driver, FontFace face, FontAtlasList atlases, int size)
    {
        this.driver = driver;
        this.face = face;
        this.atlases = atlases;
        this.size = size;

        driver.SetPixelSizes(face.Pointer, 0, (uint)size);
        var ftMetrics = face.Pointer->Size->Metrics;
        metrics = new FontSizeMetrics(
            FontFreeTypeValue.Pixel26Dot6(ftMetrics.Ascender),
            FontFreeTypeValue.Pixel26Dot6(ftMetrics.Descender),
            FontFreeTypeValue.Pixel26Dot6(ftMetrics.Height));
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

        driver.SetPixelSizes(face.Pointer, 0, (uint)size);
        var glyphIndex = driver.GetCharIndex(face.Pointer, (uint)character.Value);
        driver.LoadGlyph(face.Pointer, glyphIndex, (int)FtLoadFlags.Render);

        var slot = CreateGlyphSlot(character, *face.Pointer->Glyph);
        slots.Add(character, slot);
        return slot;
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
            new Vector2(width, height),
            new Vector2(glyphSlot.BitmapLeft, glyphSlot.BitmapTop),
            FontFreeTypeValue.Pixel26Dot6(glyphSlot.Advance.X));
        return atlases.FindFitting(glyph).Add(glyph, pixels);
    }

    /// <summary>Reads a FreeType bitmap into bottom-up RGBA coverage pixels.</summary>
    private static (byte Red, byte Green, byte Blue, byte Alpha)[] ReadPixels(FtBitmap bitmap, int width, int height)
    {
        var pixels = new (byte Red, byte Green, byte Blue, byte Alpha)[width * height];
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
