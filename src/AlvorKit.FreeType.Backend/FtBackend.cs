namespace AlvorKit.FreeType;

/// <summary>Implements <see cref="Ft"/> against the FreeType shared library.</summary>
public class FtBackend : Ft
{
    public override int InitFreeType(out nint library) => FtNative.InitFreeType(out library);

    public override int DoneFreeType(nint library) => FtNative.DoneFreeType(library);

    public override int NewFace(nint library, string path, CLong faceIndex, out nint face) => FtNative.NewFace(library, path, faceIndex, out face);

    public override int DoneFace(nint face) => FtNative.DoneFace(face);

    public override int SetPixelSizes(nint face, uint width, uint height) => FtNative.SetPixelSizes(face, width, height);

    public override int LoadChar(nint face, CULong charCode, int loadFlags) => FtNative.LoadChar(face, charCode, loadFlags);
}
