namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Tests text measurement and sprite-batch text writing helpers.</summary>
[TestClass]
public sealed class FontSpriteBatchExtensionsTest
{
    /// <summary>Measure sums advances and includes the last glyph box and bearing.</summary>
    [TestMethod]
    public void Measure_IncludesAdvanceBearingAndLastBox()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('a'), 0, 0, 0, 0, 10f, []);
        driver.SetGlyph(new Rune('b'), 4, 0, 2, 0, 5f, []);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);

            Assert.AreEqual(0f, batch.Writer.Measure(fontSize, ""));
            Assert.AreEqual(16f, batch.Writer.Measure(fontSize, "ab"));
            Assert.AreEqual(16f, batch.Writer.Measure(fontSize, "ab".AsSpan()));
            Assert.AreEqual(16f, batch.Writer.Measure(fontSize, "ab", 0f));
            Assert.AreEqual(16f, batch.Writer.Measure(fontSize, "ab".AsSpan(), 0f));
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Measure applies cached FreeType kerning between adjacent glyphs.</summary>
    [TestMethod]
    public void Measure_AppliesKerningBetweenGlyphs()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('a'), 4, 0, 0, 0, 10f, []);
        driver.SetGlyph(new Rune('b'), 4, 0, 0, 0, 5f, []);
        driver.SetKerning(new Rune('a'), new Rune('b'), -2f);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);

            Assert.AreEqual(12f, batch.Writer.Measure(fontSize, "ab"));
            Assert.AreEqual(12f, batch.Writer.Measure(fontSize, "ab".AsSpan()));
            Assert.AreEqual(1, driver.GetKerningCount);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Measure snaps the final glyph origin before adding the glyph box.</summary>
    [TestMethod]
    public void Measure_WithSnap_SnapsLastGlyphOrigin()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('a'), 2, 0, 0, 0, 7f, []);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);

            Assert.AreEqual(18f, batch.Writer.Measure(fontSize, "aaa", 8f));
            Assert.AreEqual(18f, batch.Writer.Measure(fontSize, "aaa".AsSpan(), 8f));
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Write emits sprite vertices at the measured glyph baseline position.</summary>
    [TestMethod]
    public void Write_EmitsPositionedTintedGlyphVertices()
    {
        var (backend, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetMetrics(10f, -2f, 12f);
        driver.SetGlyph(new Rune('A'), 4, 6, 1, 7, 8f, [255, 255, 255, 255, 255, 255, 255, 255]);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);
            Vec4 color = (0.1f, 0.2f, 0.3f, 0.4f);
            batch.Begin((100f, 100f));
            batch.Writer.Write(fontSize, "A", (20f, 30f), color);
            batch.End();

            Assert.AreEqual(1, backend.DrawElementsCalls);
            Assert.AreEqual(36, backend.LastVertexFloats.Length);
            Assert.AreEqual(-0.6f, backend.LastVertexFloats[0], 0.0001f);
            Assert.AreEqual(0.46f, backend.LastVertexFloats[1], 0.0001f);
            Assert.AreEqual(0.1f, backend.LastVertexFloats[2], 0.0001f);
            Assert.AreEqual(0.2f, backend.LastVertexFloats[3], 0.0001f);
            Assert.AreEqual(0.3f, backend.LastVertexFloats[4], 0.0001f);
            Assert.AreEqual(0.4f, backend.LastVertexFloats[5], 0.0001f);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>All Write overloads forward into the span-based implementation.</summary>
    [TestMethod]
    public void Write_Overloads_Succeed()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetGlyph(new Rune('A'), 1, 1);
        driver.SetGlyph(new Rune('B'), 1, 1);
        try
        {
            using var font = new Font(context, "Inter.ttf");
            var fontSize = font.Size(12);
            var text = "AB";
            Vec4 color = (1f, 0f, 0f, 1f);
            batch.Begin((100f, 100f));
            batch.Writer.Write(fontSize, text, Vec2.Zero);
            batch.Writer.Write(fontSize, text, Vec2.Zero, color);
            batch.Writer.Write(fontSize, text, Vec2.Zero, color, 2f);
            batch.Writer.Write(fontSize, text, Vec2.Zero, color, 2f, 1f);
            batch.Writer.Write(fontSize, text.AsSpan(), Vec2.Zero);
            batch.Writer.Write(fontSize, text.AsSpan(), Vec2.Zero, color);
            batch.Writer.Write(fontSize, text.AsSpan(), Vec2.Zero, color, 2f);
            batch.Writer.Write(fontSize, text.AsSpan(), Vec2.Zero, color, 2f, 1f);
            batch.End();
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }
}
