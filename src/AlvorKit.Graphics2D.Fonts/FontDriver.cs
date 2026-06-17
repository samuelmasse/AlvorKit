namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Small test seam over the FreeType calls required by the font atlas.</summary>
internal abstract unsafe class FontDriver
{
    /// <summary>Initializes and returns a FreeType library handle.</summary>
    internal abstract nint InitFreeType();

    /// <summary>Releases a FreeType library handle.</summary>
    internal abstract void DoneFreeType(nint library);

    /// <summary>Opens a face from a filesystem path.</summary>
    internal abstract FtFaceRec* NewFace(nint library, string path, nint faceIndex);

    /// <summary>Opens a face from caller-owned bytes that remain pinned for the face lifetime.</summary>
    internal abstract FtFaceRec* NewMemoryFace(nint library, nint data, int length, nint faceIndex);

    /// <summary>Releases a FreeType face handle.</summary>
    internal abstract void DoneFace(FtFaceRec* face);

    /// <summary>Selects the active pixel size for the face.</summary>
    internal abstract void SetPixelSizes(FtFaceRec* face, uint pixelWidth, uint pixelHeight);

    /// <summary>Maps a Unicode scalar to a glyph index in the active face charmap.</summary>
    internal abstract uint GetCharIndex(FtFaceRec* face, uint charCode);

    /// <summary>Loads a glyph into the face's current glyph slot.</summary>
    internal abstract void LoadGlyph(FtFaceRec* face, uint glyphIndex, int loadFlags);
}
