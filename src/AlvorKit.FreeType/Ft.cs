namespace AlvorKit.FreeType;

/// <summary>
/// The FreeType API surface. All handles are opaque; functions return FT_Error
/// (0 = success). Read rendered glyphs by dereferencing the face handle as
/// <see cref="FtFaceRec"/>. Every method throws NotImplementedException until a
/// backend overrides it (e.g. FtBackend from AlvorKit.FreeType.Backend).
/// </summary>
public class Ft
{
    public const int LoadRender = 4;
    public const byte PixelModeGray = 2;

    public virtual int InitFreeType(out nint library) => throw new NotImplementedException();

    public virtual int DoneFreeType(nint library) => throw new NotImplementedException();

    public virtual int NewFace(nint library, string path, CLong faceIndex, out nint face) => throw new NotImplementedException();

    public virtual int DoneFace(nint face) => throw new NotImplementedException();

    public virtual int SetPixelSizes(nint face, uint width, uint height) => throw new NotImplementedException();

    public virtual int LoadChar(nint face, CULong charCode, int loadFlags) => throw new NotImplementedException();
}
