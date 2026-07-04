namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Deterministic FreeType binding used to feed fake face and glyph slot state to font tests.</summary>
internal sealed unsafe class FontsTestFt : FtNoop, IDisposable
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

    /// <summary>Configured kerning adjustments keyed by paired glyph indices.</summary>
    private readonly Dictionary<ulong, float> kernings = [];

    /// <summary>The unmanaged coverage buffer used by the current glyph slot.</summary>
    private nint glyphBuffer;

    /// <summary>Creates a fake face with active size and glyph pointers.</summary>
    public FontsTestFt()
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

    /// <summary>Gets the number of kerning lookup calls.</summary>
    public int GetKerningCount { get; private set; }

    /// <summary>Gets the most recent file path passed to <see cref="NewFace(nint, nint, CLong, out FtFaceRec*)"/>.</summary>
    public string? LastFile { get; private set; }

    /// <summary>Gets the most recent memory font bytes passed to <see cref="NewMemoryFace"/>.</summary>
    public byte[] LastMemoryBytes { get; private set; } = [];

    /// <summary>Gets the most recent face index passed to a face creation method.</summary>
    public nint LastFaceIndex { get; private set; }

    /// <summary>Gets or sets the FreeType error returned from memory face creation.</summary>
    public int NewMemoryFaceError { get; set; }

    /// <inheritdoc/>
    public override int InitFreeType(out nint alibrary)
    {
        InitFreeTypeCount++;
        alibrary = Library;
        return 0;
    }

    /// <inheritdoc/>
    public override int DoneFreeType(nint library)
    {
        DoneFreeTypeCount++;
        return 0;
    }

    /// <inheritdoc/>
    public override int NewFace(nint library, nint filepathname, CLong faceIndex, out FtFaceRec* aface)
    {
        NewFaceCount++;
        LastFile = Marshal.PtrToStringUTF8(filepathname);
        LastFaceIndex = faceIndex.Value;
        aface = face;
        return 0;
    }

    /// <inheritdoc/>
    public override int NewMemoryFace(nint library, nint fileBase, CLong fileSize, CLong faceIndex, out FtFaceRec* aface)
    {
        NewMemoryFaceCount++;
        LastMemoryBytes = ReadBytes(fileBase, checked((int)fileSize.Value));
        LastFaceIndex = faceIndex.Value;
        aface = NewMemoryFaceError == 0 ? face : null;
        return NewMemoryFaceError;
    }

    /// <inheritdoc/>
    public override int DoneFace(FtFaceRec* face)
    {
        DoneFaceCount++;
        return 0;
    }

    /// <inheritdoc/>
    public override int SetPixelSizes(FtFaceRec* face, uint pixelWidth, uint pixelHeight)
    {
        SetPixelSizesCount++;
        return 0;
    }

    /// <inheritdoc/>
    public override uint GetCharIndex(FtFaceRec* face, CULong charCode) => checked((uint)charCode.Value.ToUInt64());

    /// <inheritdoc/>
    public override int LoadGlyph(FtFaceRec* face, uint glyphIndex, int loadFlags)
    {
        LoadGlyphCount++;
        ApplyGlyph(glyphs.GetValueOrDefault(glyphIndex));
        return 0;
    }

    /// <inheritdoc/>
    public override int GetKerning(FtFaceRec* face, uint leftGlyph, uint rightGlyph, uint kernMode, out FtVector akerning)
    {
        GetKerningCount++;
        var key = KerningKey(leftGlyph, rightGlyph);
        akerning = new FtVector { X = Pixel(kernings.GetValueOrDefault(key)), Y = default };
        return 0;
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

    /// <summary>Configures a horizontal kerning adjustment between two Unicode scalars.</summary>
    internal void SetKerning(Rune left, Rune right, float x)
    {
        kernings[KerningKey((uint)left.Value, (uint)right.Value)] = x;
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

    /// <summary>Copies unmanaged font bytes into a managed assertion buffer.</summary>
    private static byte[] ReadBytes(nint source, int byteCount)
    {
        if (source == 0 || byteCount == 0)
            return [];

        var bytes = new byte[byteCount];
        Marshal.Copy(source, bytes, 0, bytes.Length);
        return bytes;
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
    private static CLong Pixel(float value) => new((nint)MathF.Round(value * FontFreeType.PixelOne));

    /// <summary>Creates a stable key for one glyph-index pair.</summary>
    private static ulong KerningKey(uint leftGlyph, uint rightGlyph) => ((ulong)leftGlyph << 32) | rightGlyph;

    /// <summary>One configured glyph slot payload.</summary>
    private sealed record GlyphData(int Width, int Height, int Pitch, int BearingX, int BearingY, float Advance, byte[] Alpha)
    {
        /// <summary>Gets an empty glyph payload.</summary>
        internal static GlyphData Empty { get; } = new(0, 0, 0, 0, 0, 0f, []);
    }
}
