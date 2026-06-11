namespace AlvorKit.FreeType;

/// <summary>
/// Maps the prefix of FT_GlyphSlotRec, up to the rendered bitmap placement —
/// only ever read through FtFaceRec.Glyph. Offsets verified against MSVC for
/// FreeType 2.14.3.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtGlyphSlotRec
{
    public nint Library;
    public nint Face;
    public nint Next;
    public uint GlyphIndex;
    public FtGeneric Generic;
    public FtGlyphMetrics Metrics;
    public CLong LinearHoriAdvance;
    public CLong LinearVertAdvance;
    public FtVector Advance;
    public uint Format;
    public FtBitmap Bitmap;
    public int BitmapLeft;
    public int BitmapTop;
}

/// <summary>Maps FT_Glyph_Metrics (all FT_Pos).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtGlyphMetrics
{
    public CLong Width;
    public CLong Height;
    public CLong HoriBearingX;
    public CLong HoriBearingY;
    public CLong HoriAdvance;
    public CLong VertBearingX;
    public CLong VertBearingY;
    public CLong VertAdvance;
}

/// <summary>Maps FT_Vector.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtVector
{
    public CLong X;
    public CLong Y;
}

/// <summary>Maps FT_Bitmap.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtBitmap
{
    public uint Rows;
    public uint Width;
    public int Pitch;
    public nint Buffer;
    public ushort NumGrays;
    public byte PixelMode;
    public byte PaletteMode;
    public nint Palette;
}
