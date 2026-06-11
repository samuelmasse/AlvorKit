namespace AlvorKit.FreeType;

/// <summary>
/// Maps the public prefix of FT_FaceRec, up to the glyph slot — the struct is
/// only ever read through the face handle, never allocated managed-side.
/// FT_Long/FT_Pos are C longs (CLong), which keeps the layout correct on both
/// win-x64 (4 bytes) and unix (8 bytes). Offsets verified against MSVC for
/// FreeType 2.14.3.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtFaceRec
{
    public CLong NumFaces;
    public CLong FaceIndex;
    public CLong FaceFlags;
    public CLong StyleFlags;
    public CLong NumGlyphs;
    public nint FamilyName;
    public nint StyleName;
    public int NumFixedSizes;
    public nint AvailableSizes;
    public int NumCharmaps;
    public nint Charmaps;
    public FtGeneric Generic;
    public FtBBox Bbox;
    public ushort UnitsPerEM;
    public short Ascender;
    public short Descender;
    public short Height;
    public short MaxAdvanceWidth;
    public short MaxAdvanceHeight;
    public short UnderlinePosition;
    public short UnderlineThickness;
    public nint Glyph;
}

/// <summary>Maps FT_Generic.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtGeneric
{
    public nint Data;
    public nint Finalizer;
}

/// <summary>Maps FT_BBox.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FtBBox
{
    public CLong XMin;
    public CLong YMin;
    public CLong XMax;
    public CLong YMax;
}
