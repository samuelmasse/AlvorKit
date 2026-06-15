namespace AlvorKit.FreeType.Demo;

/// <summary>Owns the FreeType binding tour state, native handles, and generated PNG output path.</summary>
internal sealed unsafe partial class FreeTypeDemo(Ft ft, string fontPath, string outputRoot) : IDisposable
{
    /// <summary>The initialized FreeType library handle.</summary>
    private nint library;

    /// <summary>The face opened with FT_New_Face.</summary>
    private FtFaceRec* pathFace;

    /// <summary>The face opened with FT_New_Memory_Face.</summary>
    private FtFaceRec* memoryFace;

    /// <summary>The face opened with FT_Open_Face.</summary>
    private FtFaceRec* openFace;

    /// <summary>The pinned managed font bytes backing the memory face.</summary>
    private GCHandle pinnedFont;

    /// <summary>Tracks whether native resources have already been released.</summary>
    private bool disposed;

    /// <summary>Initializes the FreeType library used by all later demo steps.</summary>
    public void InitializeLibrary()
    {
        Require("FT_Init_FreeType", ft.InitFreeType(out library));
    }

    /// <summary>Opens the demo font through the generated FT_New_Face overload.</summary>
    public void OpenNewFace()
    {
        Require("FT_New_Face", ft.NewFace(library, fontPath, FreeTypeValues.Long(0), out pathFace));
    }

    /// <summary>Opens the demo font from pinned managed bytes through FT_New_Memory_Face.</summary>
    public void OpenMemoryFace()
    {
        var fontBytes = File.ReadAllBytes(fontPath);
        pinnedFont = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
        Require(
            "FT_New_Memory_Face",
            ft.NewMemoryFace(
                library,
                pinnedFont.AddrOfPinnedObject(),
                FreeTypeValues.Long(fontBytes.Length),
                FreeTypeValues.Long(0),
                out memoryFace));
    }

    /// <summary>Opens the demo font through FT_Open_Face and the generated FtOpenArgs struct.</summary>
    public void OpenFaceWithOpenArgs()
    {
        openFace = OpenPathFace();
    }

    /// <summary>Releases FreeType resources allocated by the walkthrough.</summary>
    public void Dispose()
    {
        if (disposed)
            return;

        ReleaseFreeTypeResources();
        if (pinnedFont.IsAllocated)
            pinnedFont.Free();

        disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>Opens a face through FT_Open_Face using the generated FtOpenArgs struct.</summary>
    private FtFaceRec* OpenPathFace()
    {
        FtFaceRec* face;
        var pathBytes = Encoding.UTF8.GetBytes(fontPath + '\0');
        fixed (byte* path = pathBytes)
        {
            var openArgs = new FtOpenArgs
            {
                Flags = (uint)FtOpenFlags.Pathname,
                Pathname = (nint)path,
            };
            Require("FT_Open_Face", ft.OpenFace(library, &openArgs, FreeTypeValues.Long(0), out face));
        }

        return face;
    }

    /// <summary>Releases every FreeType resource allocated by the walkthrough.</summary>
    private void ReleaseFreeTypeResources()
    {
        if (openFace != null)
            _ = ft.DoneFace(openFace);
        if (memoryFace != null)
            _ = ft.DoneFace(memoryFace);
        if (pathFace != null)
            _ = ft.DoneFace(pathFace);
        if (library != 0)
            _ = ft.DoneFreeType(library);
    }

    /// <summary>Throws when a required FreeType call returns an error.</summary>
    private void Require(string cName, int error)
    {
        FreeTypeStatus.Require(ft, cName, error);
    }

    /// <summary>Writes a console message for optional FreeType calls.</summary>
    private void ReportOptional(string cName, int error)
    {
        FreeTypeStatus.ReportOptional(ft, cName, error);
    }
}
