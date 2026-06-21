namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Tests glyph loading, caching, atlas upload, rollover, and repacking behavior.</summary>
[TestClass]
public sealed class FontSizeTest
{
    /// <summary>An empty glyph still creates a cached atlas slot without uploading pixels.</summary>
    [TestMethod]
    public void GlyphSlot_EmptyGlyph_CachesSlotWithoutUpload()
    {
        var (backend, driver, batch, context) = FontsTestHarness.CreateContext();
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);

            var first = fontSize.GlyphSlot(new Rune('A'));
            var second = fontSize.GlyphSlot(new Rune('A'));

            Assert.AreSame(first, second);
            Assert.AreEqual(Vec2u.Zero, first.Position);
            Assert.AreEqual(Vec2u.Zero, first.Glyph.Box);
            Assert.AreSame(font.Textures[0], first.Texture);
            Assert.AreEqual(1, driver.LoadGlyphCount);
            Assert.AreEqual(0, backend.TexSubImage2DCalls);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Different runes and different font sizes receive independent glyph slots.</summary>
    [TestMethod]
    public void GlyphSlot_DifferentRuneAndSize_ReturnsDistinctSlots()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 1, 1);
        driver.SetGlyph(new Rune('B'), 1, 1);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var small = font.Size(12);
            var large = font.Size(24);

            var smallA = small.GlyphSlot(new Rune('A'));
            var smallB = small.GlyphSlot(new Rune('B'));
            var largeA = large.GlyphSlot(new Rune('A'));

            Assert.AreNotSame(smallA, smallB);
            Assert.AreNotSame(smallA, largeA);
            Assert.AreEqual(new Vec2u(0u, 0u), smallA.Position);
            Assert.AreEqual(new Vec2u(2u, 0u), smallB.Position);
            Assert.AreEqual(new Vec2u(4u, 0u), largeA.Position);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Rendered glyph bytes are converted to bottom-up RGBA coverage and uploaded to the atlas.</summary>
    [TestMethod]
    public void GlyphSlot_RenderedGlyph_UploadsBottomUpRgbaPixels()
    {
        var (backend, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 2, 2, 1, 3, 6.5f, [1, 2, 3, 4]);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var slot = font.Size(12).GlyphSlot(new Rune('A'));

            Assert.AreEqual(new Vec2u(2u, 2u), slot.Glyph.Box);
            Assert.AreEqual(new Vec2i(1, 3), slot.Glyph.Bearing);
            Assert.AreEqual(6.5f, slot.Glyph.Advance);
            Assert.AreEqual((0, 2046, 2, 2), backend.LastTexSubImage);
            CollectionAssert.AreEqual(
                new byte[]
                {
                    255, 255, 255, 3,
                    255, 255, 255, 4,
                    255, 255, 255, 1,
                    255, 255, 255, 2
                },
                backend.LastTexSubImageBytes);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>A glyph that no longer fits the first atlas starts a second atlas texture.</summary>
    [TestMethod]
    public void GlyphSlot_WhenAtlasCannotFitGlyph_StartsSecondAtlas()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 1024, 1024);
        driver.SetGlyph(new Rune('B'), 1024, 1024);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);

            var first = fontSize.GlyphSlot(new Rune('A'));
            var second = fontSize.GlyphSlot(new Rune('B'));

            Assert.AreEqual(2, font.Textures.Length);
            Assert.AreSame(font.Textures[0], first.Texture);
            Assert.AreSame(font.Textures[1], second.Texture);
            Assert.AreEqual(Vec2u.Zero, second.Position);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>ForcePack reorders pending glyphs by height and renders the repacked atlas through the sprite batch.</summary>
    [TestMethod]
    public void ForcePack_AfterAscendingGlyphs_ReordersSlots()
    {
        var (backend, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 1, 1);
        driver.SetGlyph(new Rune('B'), 2, 2);
        driver.SetGlyph(new Rune('C'), 3, 3);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);
            var first = fontSize.GlyphSlot(new Rune('A'));
            var second = fontSize.GlyphSlot(new Rune('B'));
            var third = fontSize.GlyphSlot(new Rune('C'));

            font.ForcePack();

            Assert.AreEqual(new Vec2u(7u, 0u), first.Position);
            Assert.AreEqual(new Vec2u(4u, 0u), second.Position);
            Assert.AreEqual(new Vec2u(0u, 0u), third.Position);
            Assert.AreEqual(1, backend.FramebufferTextureCalls);
            Assert.AreEqual(1, backend.DrawElementsCalls);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Pack skips unpacked atlases until a failed fit marks one as full.</summary>
    [TestMethod]
    public void Pack_OnlyReordersAfterAtlasWasMarkedFull()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 1, 1);
        driver.SetGlyph(new Rune('B'), 2, 2);
        driver.SetGlyph(new Rune('C'), 2044, 2047);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);
            var first = fontSize.GlyphSlot(new Rune('A'));
            var second = fontSize.GlyphSlot(new Rune('B'));

            font.Pack();

            Assert.AreEqual(new Vec2u(0u, 0u), first.Position);
            Assert.AreEqual(new Vec2u(2u, 0u), second.Position);

            _ = fontSize.GlyphSlot(new Rune('C'));
            font.Pack();

            Assert.AreEqual(2, font.Textures.Length);
            Assert.AreEqual(new Vec2u(3u, 0u), first.Position);
            Assert.AreEqual(new Vec2u(0u, 0u), second.Position);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>ForcePack keeps the current layout when the height-sorted layout still cannot fit.</summary>
    [TestMethod]
    public void ForcePack_WhenSortedLayoutCannotFit_KeepsCurrentPositions()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 2047, 1);
        driver.SetGlyph(new Rune('B'), 1, 2045);
        driver.SetGlyph(new Rune('C'), 1, 1);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);
            var first = fontSize.GlyphSlot(new Rune('A'));
            var second = fontSize.GlyphSlot(new Rune('B'));
            var third = fontSize.GlyphSlot(new Rune('C'));

            font.ForcePack();

            Assert.AreEqual(new Vec2u(0u, 0u), first.Position);
            Assert.AreEqual(new Vec2u(0u, 2u), second.Position);
            Assert.AreEqual(new Vec2u(2u, 2u), third.Position);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }
}
