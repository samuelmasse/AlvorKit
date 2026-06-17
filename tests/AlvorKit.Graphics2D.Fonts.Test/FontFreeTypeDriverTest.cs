namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Tests the generated FreeType binding adapter against the checked-in Inter font.</summary>
[TestClass]
public sealed unsafe class FontFreeTypeDriverTest
{
    /// <summary>The real driver can open file and memory faces, select a size, and load a rendered glyph.</summary>
    [TestMethod]
    public void Driver_WithInterFont_LoadsGlyphFromFileAndMemory()
    {
        var driver = new FontFreeTypeDriver(new FtBackend());
        var library = driver.InitFreeType();
        try
        {
            var fontBytes = File.ReadAllBytes(InterFontPath());
            using var pinned = fontBytes.AsMemory().Pin();
            var fileFace = driver.NewFace(library, InterFontPath(), 0);
            var memoryFace = driver.NewMemoryFace(library, (nint)pinned.Pointer, fontBytes.Length, 0);
            try
            {
                driver.SetPixelSizes(fileFace, 0, 16);
                var glyphIndex = driver.GetCharIndex(fileFace, 'A');
                driver.LoadGlyph(fileFace, glyphIndex, (int)FtLoadFlags.Render);

                Assert.AreNotEqual(0u, glyphIndex);
                Assert.IsTrue(fileFace->Glyph->Bitmap.Width > 0);
                Assert.IsTrue(memoryFace->NumGlyphs.Value.ToInt64() > 0);
            }
            finally
            {
                driver.DoneFace(memoryFace);
                driver.DoneFace(fileFace);
            }
        }
        finally
        {
            driver.DoneFreeType(library);
        }
    }

    /// <summary>FreeType failures are mapped to <see cref="FontException"/> with method and error context.</summary>
    [TestMethod]
    public void Driver_WhenFreeTypeCallFails_ThrowsFontException()
    {
        var driver = new FontFreeTypeDriver(new FtBackend());
        var library = driver.InitFreeType();
        try
        {
            var exception = Assert.ThrowsException<FontException>(() => driver.NewFace(library, MissingFontPath(), 0));

            StringAssert.Contains(exception.Message, nameof(Ft.NewFace));
            StringAssert.Contains(exception.Message, "failed with error");
        }
        finally
        {
            driver.DoneFreeType(library);
        }
    }

    /// <summary>Finds the repository font asset from the test output directory.</summary>
    private static string InterFontPath() => Path.Combine(RepoRoot(), "res", "fonts", "Inter.ttf");

    /// <summary>Builds a guaranteed-missing font path under the repository root.</summary>
    private static string MissingFontPath() => Path.Combine(RepoRoot(), "out", "missing-font-file.ttf");

    /// <summary>Finds the repository root by walking up to the solution file.</summary>
    private static string RepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find AlvorKit.slnx from the test output directory.");
    }
}
