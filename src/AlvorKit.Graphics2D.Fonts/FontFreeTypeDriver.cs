namespace AlvorKit.Graphics2D.Fonts;

/// <summary>FreeType driver implemented with the generated AlvorKit FreeType binding.</summary>
internal sealed unsafe class FontFreeTypeDriver(Ft ft) : FontDriver
{
    /// <inheritdoc/>
    internal override nint InitFreeType()
    {
        Require(nameof(Ft.InitFreeType), ft.InitFreeType(out var library));
        return library;
    }

    /// <inheritdoc/>
    internal override void DoneFreeType(nint library) => Require(nameof(Ft.DoneFreeType), ft.DoneFreeType(library));

    /// <inheritdoc/>
    internal override FtFaceRec* NewFace(nint library, string path, nint faceIndex)
    {
        var pathPointer = Marshal.StringToCoTaskMemUTF8(path);
        try
        {
            Require(nameof(Ft.NewFace), ft.NewFace(library, pathPointer, new CLong(faceIndex), out var face));
            return face;
        }
        finally
        {
            Marshal.FreeCoTaskMem(pathPointer);
        }
    }

    /// <inheritdoc/>
    internal override FtFaceRec* NewMemoryFace(nint library, nint data, int length, nint faceIndex)
    {
        Require(nameof(Ft.NewMemoryFace), ft.NewMemoryFace(library, data, new CLong(length), new CLong(faceIndex), out var face));
        return face;
    }

    /// <inheritdoc/>
    internal override void DoneFace(FtFaceRec* face) => Require(nameof(Ft.DoneFace), ft.DoneFace(face));

    /// <inheritdoc/>
    internal override void SetPixelSizes(FtFaceRec* face, uint pixelWidth, uint pixelHeight) =>
        Require(nameof(Ft.SetPixelSizes), ft.SetPixelSizes(face, pixelWidth, pixelHeight));

    /// <inheritdoc/>
    internal override uint GetCharIndex(FtFaceRec* face, uint charCode) => ft.GetCharIndex(face, new CULong(charCode));

    /// <inheritdoc/>
    internal override void LoadGlyph(FtFaceRec* face, uint glyphIndex, int loadFlags) =>
        Require(nameof(Ft.LoadGlyph), ft.LoadGlyph(face, glyphIndex, loadFlags));

    /// <summary>Throws a font exception when a FreeType call reports an error.</summary>
    private void Require(string method, int error)
    {
        if (error == 0)
            return;

        ft.ErrorString(error, out var description);
        throw new FontException($"FreeType {method} failed with error {error}: {description ?? "unknown error"}");
    }
}
