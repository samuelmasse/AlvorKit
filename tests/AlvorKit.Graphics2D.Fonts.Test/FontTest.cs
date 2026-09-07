namespace AlvorKit;

/// <summary>Tests font construction, size caching, metrics, and empty packing paths.</summary>
[TestClass]
public class FontTest
{
    /// <summary>Font disposal releases its face while its atlas remains alive until the GL layer is disposed.</summary>
    [TestMethod]
    public void Dispose_ReleasesFaceAndLeavesAtlasToLayer()
    {
        var (backend, driver, _, context) = FontsTestHarness.CreateContext();
        var font = new Font(context, "Inter.ttf");
        var texture = font.Textures[0].Id.Handle;
        var deletedBeforeDispose = backend.Deleted.Count;

        font.Dispose();

        Assert.AreEqual(1, driver.DoneFaceCount);
        Assert.AreEqual(0, driver.DoneFreeTypeCount);
        Assert.AreEqual(deletedBeforeDispose, backend.Deleted.Count);

        context.Dispose();

        Assert.AreEqual(deletedBeforeDispose, backend.Deleted.Count);

        context.GL.Dispose();

        Assert.AreEqual(1, driver.DoneFreeTypeCount);
        CollectionAssert.Contains(backend.Deleted, texture);
        driver.Dispose();
    }

    /// <summary>File, memory, and option constructors open faces and expose the initial atlas texture.</summary>
    [TestMethod]
    public void Constructors_OpenFacesAndExposeInitialTexture()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        try
        {
            ReadOnlySpan<byte> memoryBytes = [1, 2, 3];
            using var fileFont = new Font(context, "Inter.ttf");
            using var memoryFont = new Font(context, memoryBytes);
            using var defaultOptionsFont = new Font(context, new FontOptions());
            using var optionsFont = new Font(context, new FontOptions { Data = new byte[] { 4, 5 }, Index = 2 });

            Assert.AreEqual(1, fileFont.Textures.Length);
            Assert.AreEqual(1, memoryFont.Textures.Length);
            Assert.AreEqual(1, defaultOptionsFont.Textures.Length);
            Assert.AreEqual(1, optionsFont.Textures.Length);
            Assert.AreEqual(1, driver.NewFaceCount);
            Assert.AreEqual(3, driver.NewMemoryFaceCount);
            Assert.AreEqual("Inter.ttf", driver.LastFile);
            CollectionAssert.AreEqual(new byte[] { 4, 5 }, driver.LastMemoryBytes);
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

    /// <summary>Memory face construction releases its native byte copy when FreeType rejects the face.</summary>
    [TestMethod]
    public void Constructor_WhenMemoryFaceFails_ThrowsFontException()
    {
        var (_, driver, batch, context) = FontsTestHarness.CreateContext();
        driver.NewMemoryFaceError = 7;
        try
        {
            var exception = Assert.ThrowsException<FontException>(() => CreateMemoryFont(context));

            StringAssert.Contains(exception.Message, nameof(Ft.NewMemoryFace));
            CollectionAssert.AreEqual(new byte[] { 9, 8, 7 }, driver.LastMemoryBytes);
        }
        finally
        {
            context.Dispose();
            batch.Dispose();
            driver.Dispose();
        }

        static void CreateMemoryFont(FontContext context)
        {
            ReadOnlySpan<byte> memoryBytes = [9, 8, 7];
            using var font = new Font(context, memoryBytes);
        }
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
