namespace AlvorKit;

[TestClass]
public class RootPngsTest
{
    /// <summary>Indexed PNG transparency preserves palette alpha and straight RGB while reversing rows for OpenGL.</summary>
    [TestMethod]
    public void Indexer_PreservesPaletteTransparencyAndRowOrder()
    {
        using var workspace = TempWorkspace.Create();
        var path = workspace.PathFor("PaletteTransparency.png");
        // Two-bit palette: transparent black, opaque black, half-transparent red, opaque blue, in top-down row order.
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACAgMAAAAP2OW3AAAADFBMVEUAAAAAAAD/AAAAAP/8L3PX" +
            "AAAABHRSTlMA/4D/G4olsQAAAAxJREFUeJxjEGDYAAAA5ADBJ6joVwAAAABJRU5ErkJggg==");
        File.WriteAllBytes(path, png);

        var image = new RootPngs()[path];

        Assert.AreEqual(new Vec2i(2, 2), image.Size);
        CollectionAssert.AreEqual(
            new Vec4u8[] { (255, 0, 0, 128), (0, 0, 255, 255), (0, 0, 0, 0), (0, 0, 0, 255) },
            image.Pixels.ToArray());
    }

    /// <summary>PNG loading returns decoded RGBA pixels in bottom-left-origin order.</summary>
    [TestMethod]
    public void Indexer_DecodesPngPixels()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        try
        {
            var png = PngBuilder.Create(2, 1, false);
            png.SetPixel(1, 2, 3, 0, 0);
            png.SetPixel(4, 5, 6, 1, 0);
            using (var stream = File.Create(path))
                png.Save(stream);

            var image = new RootPngs()[path];

            Assert.AreEqual(new Vec2i(2, 1), image.Size);
            CollectionAssert.AreEqual(
                new Vec4u8[] { (1, 2, 3, 255), (4, 5, 6, 255) },
                image.Pixels.ToArray());
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>PNG loading reads the direct path supplied by the caller.</summary>
    [TestMethod]
    public void Indexer_DecodesPngFromDirectPath()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "Pixel.png");

        try
        {
            var png = PngBuilder.Create(2, 1, false);
            png.SetPixel(9, 8, 7, 0, 0);
            png.SetPixel(6, 5, 4, 1, 0);
            using (var stream = File.Create(path))
                png.Save(stream);

            var image = new RootPngs()[path];

            Assert.AreEqual(new Vec2i(2, 1), image.Size);
            CollectionAssert.AreEqual(
                new Vec4u8[] { (9, 8, 7, 255), (6, 5, 4, 255) },
                image.Pixels.ToArray());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
