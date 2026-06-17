namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Tests the font layer against the generated FreeType backend and checked-in Inter font.</summary>
[TestClass]
public sealed class FontFreeTypeTest
{
    /// <summary>The real FreeType backend can load glyphs from file-backed and native-copied memory-backed fonts.</summary>
    [TestMethod]
    public void Font_WithInterFont_LoadsGlyphFromFileAndCopiedMemory()
    {
        var (_, gl) = FontsTestHarness.CreateLayer();
        using var batch = new SpriteBatch(gl);
        using var context = new FontContext(gl, batch);
        var fontBytes = File.ReadAllBytes(InterFontPath());

        using var fileFont = new Font(context, InterFontPath());
        using var memoryFont = new Font(context, fontBytes.AsSpan());
        Array.Clear(fontBytes);

        var fileGlyph = fileFont.Size(16).GlyphSlot(new Rune('A')).Glyph;
        var memoryGlyph = memoryFont.Size(16).GlyphSlot(new Rune('A')).Glyph;

        Assert.IsTrue(fileGlyph.Box.X > 0f);
        Assert.IsTrue(fileGlyph.Box.Y > 0f);
        Assert.IsTrue(memoryGlyph.Box.X > 0f);
        Assert.IsTrue(memoryGlyph.Box.Y > 0f);
    }

    /// <summary>FreeType failures are mapped to <see cref="FontException"/> with method and error context.</summary>
    [TestMethod]
    public void Font_WhenFreeTypeCallFails_ThrowsFontException()
    {
        var (_, gl) = FontsTestHarness.CreateLayer();
        using var batch = new SpriteBatch(gl);
        using var context = new FontContext(gl, batch);

        var exception = Assert.ThrowsException<FontException>(() => new Font(context, MissingFontPath()));

        StringAssert.Contains(exception.Message, nameof(Ft.NewFace));
        StringAssert.Contains(exception.Message, "failed with error");
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
