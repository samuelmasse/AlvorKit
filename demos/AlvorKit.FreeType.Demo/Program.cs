using AlvorKit.FreeType;
using AlvorKit.FreeType.Demo;

unsafe
{
    var fontPath = Path.GetFullPath(Path.Combine("res", "fonts", "Inter.ttf"));
    if (!File.Exists(fontPath))
        throw new FileNotFoundException("Required demo font is missing.", fontPath);

    var outputRoot = Path.GetFullPath(Path.Combine("out", "freetype-demo"));
    Directory.CreateDirectory(outputRoot);

    Console.WriteLine("AlvorKit.FreeType.Demo - generated freetype.h binding tour");
    Console.WriteLine($"Font: {fontPath}");
    Console.WriteLine($"Output: {outputRoot}");
    Console.WriteLine();

    Ft ft = new FtBackend();
    nint library = 0;
    FtFaceRec* pathFace = null;
    FtFaceRec* memoryFace = null;
    FtFaceRec* openFace = null;
    GCHandle pinnedFont = default;

    try
    {
        FreeTypeTour.ReportErrorString(ft);
        FreeTypeStatus.Require(ft, "FT_Init_FreeType", ft.InitFreeType(out library));
        FreeTypeTour.ReportLibraryVersion(ft, library);

        FreeTypeStatus.Require(ft, "FT_New_Face", ft.NewFace(library, fontPath, FreeTypeValues.Long(0), out pathFace));
        FreeTypeTour.ReportFaceBasics(ft, pathFace, fontPath);
        FreeTypeTour.ReportReferenceCounting(ft, pathFace);
        FreeTypeTour.ReportOptionalAttachments(ft, pathFace, fontPath);
        FreeTypeTour.ReportOptionalBitmapStrike(ft, pathFace);
        FreeTypeTour.ReportExtraHeaderApis(ft, library, pathFace);

        var fontBytes = File.ReadAllBytes(fontPath);
        pinnedFont = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
        FreeTypeStatus.Require(
            ft,
            "FT_New_Memory_Face",
            ft.NewMemoryFace(library, pinnedFont.AddrOfPinnedObject(), FreeTypeValues.Long(fontBytes.Length), FreeTypeValues.Long(0), out memoryFace));

        openFace = OpenPathFace(ft, library, fontPath);

        var exported = new[]
        {
            FreeTypeTour.ExportNewFaceGlyph(ft, pathFace, outputRoot),
            FreeTypeTour.ExportMemoryFaceGlyph(ft, memoryFace, outputRoot),
            FreeTypeTour.ExportOpenFaceGlyph(ft, openFace, outputRoot),
            FreeTypeTour.ExportSizingApis(ft, pathFace, outputRoot),
            FreeTypeTour.ExportRenderModes(ft, pathFace, outputRoot),
            FreeTypeTour.ExportLoadFlags(ft, pathFace, outputRoot),
            FreeTypeTour.ExportTransform(ft, pathFace, outputRoot),
            FreeTypeTour.ExportKerning(ft, pathFace, outputRoot),
            FreeTypeTour.ExportCharmapGrid(ft, pathFace, outputRoot),
            FreeTypeTour.ExportNamesAndComposite(ft, pathFace, outputRoot),
            FreeTypeTour.ExportOutline(ft, pathFace, outputRoot),
            FreeTypeTour.ExportFixedPointAndVector(ft, pathFace, outputRoot),
            FreeTypeTour.ExportOutlineGeometry(ft, pathFace, outputRoot),
            FreeTypeTour.ExportGlyphBitmapObjects(ft, library, pathFace, outputRoot),
        };

        FreeTypeTour.ReportVariationSelectors(ft, pathFace);
        PrintExportedPngs(exported);
    }
    finally
    {
        ReleaseFreeTypeResources(ft, openFace, memoryFace, pathFace, library);
        if (pinnedFont.IsAllocated)
            pinnedFont.Free();
    }

    return 0;

    static unsafe FtFaceRec* OpenPathFace(Ft ft, nint library, string fontPath)
    {
        FtFaceRec* face;
        var pathBytes = Encoding.UTF8.GetBytes(fontPath + '\0');
        fixed (byte* path = pathBytes)
        {
            var openArgs = new FtOpenArgs { Flags = Ft.OpenPathname, Pathname = (nint)path };
            FreeTypeStatus.Require(ft, "FT_Open_Face", ft.OpenFace(library, &openArgs, FreeTypeValues.Long(0), out face));
        }

        return face;
    }

    static unsafe void ReleaseFreeTypeResources(Ft ft, FtFaceRec* openFace, FtFaceRec* memoryFace, FtFaceRec* pathFace, nint library)
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

    static void PrintExportedPngs(IEnumerable<string> exported)
    {
        Console.WriteLine();
        Console.WriteLine("Exported PNGs:");
        foreach (var path in exported)
            Console.WriteLine($"  {path}");
    }
}
