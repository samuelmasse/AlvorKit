namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Tests font construction, size caching, metrics, and empty packing paths.</summary>
[TestClass]
public sealed class FontTest
{
    /// <summary>File, memory, and option constructors open faces and expose the initial atlas texture.</summary>
    [TestMethod]
    public void Constructors_OpenFacesAndExposeInitialTexture()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        try
        {
            using var fileFont = new Font(context, "Inter.ttf");
            using var memoryFont = new Font(context, [1, 2, 3]);
            using var defaultOptionsFont = new Font(context, new FontOptions());
            using var optionsFont = new Font(context, new FontOptions { Data = [4, 5], Index = 2 });

            Assert.AreEqual(1, fileFont.Textures.Length);
            Assert.AreEqual(1, memoryFont.Textures.Length);
            Assert.AreEqual(1, defaultOptionsFont.Textures.Length);
            Assert.AreEqual(1, optionsFont.Textures.Length);
            Assert.AreEqual(1, driver.NewFaceCount);
            Assert.AreEqual(3, driver.NewMemoryFaceCount);
            Assert.AreEqual("Inter.ttf", driver.LastFile);
            Assert.AreEqual(2, driver.LastMemoryLength);
            Assert.AreEqual((nint)2, driver.LastFaceIndex);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }

        Assert.AreEqual(4, driver.DoneFaceCount);
    }

    /// <summary>Repeated size requests share one cache entry while distinct pixel heights get distinct entries.</summary>
    [TestMethod]
    public void Size_CachesByPixelHeightAndCapturesMetrics()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.SetMetrics(18f, -5f, 24f);
        try
        {
            using var font = new Font(context, "Inter.ttf");

            var size = font.Size(32);
            var same = font.Size(32);
            var other = font.Size(48);

            Assert.AreSame(size, same);
            Assert.AreNotSame(size, other);
            Assert.AreEqual(32, size.Size);
            Assert.AreEqual(18f, size.Metrics.Ascender);
            Assert.AreEqual(-5f, size.Metrics.Descender);
            Assert.AreEqual(24f, size.Metrics.Height);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }

    /// <summary>Empty pack operations are no-ops and do not submit sprite draws.</summary>
    [TestMethod]
    public void PackAndForcePack_WhenEmpty_DoNotDraw()
    {
        var (backend, driver, batch, context) = FontsTestHarness.CreateContext();
        try
        {
            using var font = new Font(context, "Inter.ttf");

            font.Pack();
            font.ForcePack();

            Assert.AreEqual(0, backend.DrawElementsCalls);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }
    }
}
