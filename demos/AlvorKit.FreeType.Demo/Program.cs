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

    using var demo = new FreeTypeDemo(new FtBackend(), fontPath, outputRoot);
    demo.ReportErrorString();
    demo.InitializeLibrary();
    demo.ReportLibraryVersion();

    demo.OpenNewFace();
    demo.ReportFaceBasics();
    demo.ReportReferenceCounting();
    demo.ReportOptionalAttachments();
    demo.ReportOptionalBitmapStrike();
    demo.ReportExtraHeaderApis();

    demo.OpenMemoryFace();
    demo.OpenFaceWithOpenArgs();

    var exported = new[]
    {
        demo.ExportNewFaceGlyph(),
        demo.ExportMemoryFaceGlyph(),
        demo.ExportOpenFaceGlyph(),
        demo.ExportSizingApis(),
        demo.ExportRenderModes(),
        demo.ExportLoadFlags(),
        demo.ExportTransform(),
        demo.ExportKerning(),
        demo.ExportCharmapGrid(),
        demo.ExportNamesAndComposite(),
        demo.ExportOutline(),
        demo.ExportFixedPointAndVector(),
        demo.ExportOutlineGeometry(),
        demo.ExportGlyphBitmapObjects(),
    };

    demo.ReportVariationSelectors();
    PrintExportedPngs(exported);

    return 0;

    // Prints the PNG paths created by the demo run.
    static void PrintExportedPngs(IEnumerable<string> exported)
    {
        Console.WriteLine();
        Console.WriteLine("Exported PNGs:");
        foreach (var path in exported)
            Console.WriteLine($"  {path}");
    }
}
