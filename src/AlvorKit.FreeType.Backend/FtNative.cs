[assembly: DisableRuntimeMarshalling]

namespace AlvorKit.FreeType;

/// <summary>Raw imports from the FreeType shared library (AlvorKit.FreeType.Native).</summary>
internal static partial class FtNative
{
    private const string Lib = "freetype";

    [LibraryImport(Lib, EntryPoint = "FT_Init_FreeType")]
    public static partial int InitFreeType(out nint library);

    [LibraryImport(Lib, EntryPoint = "FT_Done_FreeType")]
    public static partial int DoneFreeType(nint library);

    [LibraryImport(Lib, EntryPoint = "FT_New_Face", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int NewFace(nint library, string path, CLong faceIndex, out nint face);

    [LibraryImport(Lib, EntryPoint = "FT_Done_Face")]
    public static partial int DoneFace(nint face);

    [LibraryImport(Lib, EntryPoint = "FT_Set_Pixel_Sizes")]
    public static partial int SetPixelSizes(nint face, uint width, uint height);

    [LibraryImport(Lib, EntryPoint = "FT_Load_Char")]
    public static partial int LoadChar(nint face, CULong charCode, int loadFlags);
}
