namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Deterministic FreeType driver used to feed fake face and glyph slot state to font tests.</summary>
internal sealed unsafe class FontsTestDriver : FontDriver, IDisposable
{
    /// <summary>The fake FreeType library handle.</summary>
    private static readonly nint Library = 0xFEED;

    /// <summary>The fake face record returned by new-face calls.</summary>
    private readonly FtFaceRec* face = (FtFaceRec*)NativeMemory.AllocZeroed((nuint)sizeof(FtFaceRec));

    /// <summary>The active fake size record.</summary>
    private readonly FtSizeRec* size = (FtSizeRec*)NativeMemory.AllocZeroed((nuint)sizeof(FtSizeRec));

    /// <summary>The active fake glyph slot.</summary>
    private readonly FtGlyphSlotRec* glyph = (FtGlyphSlotRec*)NativeMemory.AllocZeroed((nuint)sizeof(FtGlyphSlotRec));

    /// <summary>Configured glyph data keyed by Unicode scalar.</summary>
    private readonly Dictionary<uint, GlyphData> glyphs = [];

    /// <summary>The unmanaged coverage buffer used by the current glyph slot.</summary>
    private nint glyphBuffer;

    /// <summary>Creates a fake face with active size and glyph pointers.</summary>
    public FontsTestDriver()
    {
        face->Size = size;
        face->Glyph = glyph;
    }

    /// <summary>Gets the number of library initialization calls.</summary>
    public int InitFreeTypeCount { get; private set; }

    /// <summary>Gets the number of library disposal calls.</summary>
    public int DoneFreeTypeCount { get; private set; }

    /// <summary>Gets the number of file face creation calls.</summary>
    public int NewFaceCount { get; private set; }

    /// <summary>Gets the number of memory face creation calls.</summary>
    public int NewMemoryFaceCount { get; private set; }

    /// <summary>Gets the number of face disposal calls.</summary>
    public int DoneFaceCount { get; private set; }

    /// <summary>Gets the number of size selection calls.</summary>
    public int SetPixelSizesCount { get; private set; }

    /// <summary>Gets the number of glyph load calls.</summary>
    public int LoadGlyphCount { get; private set; }

    /// <summary>Gets the most recent file path passed to <see cref="NewFace"/>.</summary>
    public string? LastFile { get; private set; }

    /// <summary>Gets the most recent memory font length passed to <see cref="NewMemoryFace"/>.</summary>
    public int LastMemoryLength { get; private set; }

    /// <summary>Gets the most recent face index passed to a face creation method.</summary>
    public nint LastFaceIndex { get; private set; }

    /// <inheritdoc/>
    internal override nint InitFreeType()
    {
        InitFreeTypeCount++;
        return Library;
    }

    /// <inheritdoc/>
    internal override void DoneFreeType(nint library) => DoneFreeTypeCount++;

    /// <inheritdoc/>
    internal override FtFaceRec* NewFace(nint library, string path, nint faceIndex)
    {
        NewFaceCount++;
        LastFile = path;
        LastFaceIndex = faceIndex;
        return face;
    }

    /// <inheritdoc/>
    internal override FtFaceRec* NewMemoryFace(nint library, nint data, int length, nint faceIndex)
    {
        NewMemoryFaceCount++;
        LastMemoryLength = length;
        LastFaceIndex = faceIndex;
        return face;
    }

    /// <inheritdoc/>
    internal override void DoneFace(FtFaceRec* face) => DoneFaceCount++;

    /// <inheritdoc/>
    internal override void SetPixelSizes(FtFaceRec* face, uint pixelWidth, uint pixelHeight) => SetPixelSizesCount++;

    /// <inheritdoc/>
    internal override uint GetCharIndex(FtFaceRec* face, uint charCode) => charCode;

    /// <inheritdoc/>
    internal override void LoadGlyph(FtFaceRec* face, uint glyphIndex, int loadFlags)
    {
        LoadGlyphCount++;
        ApplyGlyph(glyphs.GetValueOrDefault(glyphIndex));
    }

    /// <summary>Configures the size metrics reported when a <see cref="FontSize"/> is created.</summary>
    internal void SetMetrics(float ascender, float descender, float height)
    {
        size->Metrics.Ascender = Pixel(ascender);
        size->Metrics.Descender = Pixel(descender);
        size->Metrics.Height = Pixel(height);
    }

    /// <summary>Configures a glyph bitmap and metrics for a Unicode scalar.</summary>
    internal void SetGlyph(Rune character, int width, int height, int bearingX, int bearingY, float advance, ReadOnlySpan<byte> alpha)
    {
        glyphs[(uint)character.Value] = new GlyphData(width, height, width, bearingX, bearingY, advance, alpha.ToArray());
    }

    /// <summary>Configures a glyph bitmap with default metrics for a Unicode scalar.</summary>
    internal void SetGlyph(Rune character, int width, int height) => SetGlyph(character, width, height, 0, 0, 0f, []);

    /// <summary>Releases fake native face, size, glyph, and bitmap memory.</summary>
    public void Dispose()
    {
        if (glyphBuffer != 0)
            NativeMemory.Free((void*)glyphBuffer);

        NativeMemory.Free(glyph);
        NativeMemory.Free(size);
        NativeMemory.Free(face);
    }

    /// <summary>Writes configured glyph data into the fake glyph slot.</summary>
    private void ApplyGlyph(GlyphData? data)
    {
        data ??= GlyphData.Empty;
        if (glyphBuffer != 0)
            NativeMemory.Free((void*)glyphBuffer);

        var byteCount = data.Pitch * data.Height;
        glyphBuffer = byteCount > 0 ? (nint)NativeMemory.AllocZeroed((nuint)byteCount) : 0;
        if (byteCount > 0)
            Marshal.Copy(data.Alpha, 0, glyphBuffer, Math.Min(data.Alpha.Length, byteCount));

        glyph->Bitmap.Width = (uint)data.Width;
        glyph->Bitmap.Rows = (uint)data.Height;
        glyph->Bitmap.Pitch = data.Pitch;
        glyph->Bitmap.Buffer = glyphBuffer;
        glyph->BitmapLeft = data.BearingX;
        glyph->BitmapTop = data.BearingY;
        glyph->Advance.X = Pixel(data.Advance);
    }

    /// <summary>Converts a pixel value into FreeType 26.6 fixed-point storage.</summary>
    private static CLong Pixel(float value) => new((nint)MathF.Round(value * FontFreeTypeValue.PixelOne));

    /// <summary>One configured glyph slot payload.</summary>
    private sealed record GlyphData(int Width, int Height, int Pitch, int BearingX, int BearingY, float Advance, byte[] Alpha)
    {
        /// <summary>Gets an empty glyph payload.</summary>
        internal static GlyphData Empty { get; } = new(0, 0, 0, 0, 0, 0f, []);
    }
}
