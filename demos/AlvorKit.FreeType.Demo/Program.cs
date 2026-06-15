using AlvorKit.FreeType;
using AlvorKit.FreeType.Demo;

const string fallbackFontUrl = "https://github.com/google/fonts/raw/main/ofl/inter/Inter%5Bopsz,wght%5D.ttf";

var outputRoot = Path.GetFullPath(Path.Combine("out", "freetype-demo"));
Directory.CreateDirectory(outputRoot);

var fontPath = DemoFont.Resolve(args, outputRoot, fallbackFontUrl);
Console.WriteLine("AlvorKit.FreeType.Demo - ordered tour of the generated freetype.h binding");
Console.WriteLine($"Font: {fontPath}");
Console.WriteLine($"Output: {outputRoot}");
Console.WriteLine();

Ft ft = new FtBackend();
var exported = new List<string>();

// FT_Error_String -> Ft.ErrorString: show how FT_Error values can be decoded when the native build includes error strings.
FreeTypeTour.ReportErrorString(ft);

// FT_Init_FreeType -> Ft.InitFreeType: create one library handle that owns all faces and child resources in this run.
using var library = FreeTypeHandles.InitLibrary(ft);

// FT_Library_Version -> Ft.LibraryVersion: query the dynamically loaded FreeType version rather than trusting compile-time macros.
FreeTypeTour.ReportLibraryVersion(ft, library.Handle);

// FT_New_Face -> Ft.NewFace: open the main face from a filesystem path.
using var pathFace = FreeTypeHandles.NewFace(ft, library.Handle, fontPath);
FreeTypeTour.ReportFaceBasics(ft, pathFace.Handle, fontPath);
FreeTypeTour.ReportReferenceCounting(ft, pathFace.Handle);
FreeTypeTour.ReportOptionalAttachments(ft, pathFace.Handle, fontPath);
FreeTypeTour.ReportOptionalBitmapStrike(ft, pathFace.Handle);

// FT_New_Memory_Face -> Ft.NewMemoryFace: pin the same font bytes and open a face from managed memory.
using var pinnedFont = new PinnedBytes(File.ReadAllBytes(fontPath));
using var memoryFace = FreeTypeHandles.NewMemoryFace(ft, library.Handle, pinnedFont);

// FT_Open_Face -> Ft.OpenFace: use an explicit FT_Open_Args structure instead of the convenience FT_New_Face wrapper.
using var openFace = FreeTypeHandles.OpenPathFace(ft, library.Handle, fontPath);

exported.Add(FreeTypeTour.ExportNewFaceGlyph(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportMemoryFaceGlyph(ft, memoryFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportOpenFaceGlyph(ft, openFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportSizingApis(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportRenderModes(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportLoadFlags(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportTransform(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportKerning(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportCharmapGrid(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportNamesAndComposite(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportOutline(ft, pathFace.Handle, outputRoot));
exported.Add(FreeTypeTour.ExportFixedPointAndVector(ft, pathFace.Handle, outputRoot));

FreeTypeTour.ReportVariationSelectors(ft, pathFace.Handle);

Console.WriteLine();
Console.WriteLine("Exported PNGs:");
foreach (var path in exported)
    Console.WriteLine($"  {path}");

return 0;
