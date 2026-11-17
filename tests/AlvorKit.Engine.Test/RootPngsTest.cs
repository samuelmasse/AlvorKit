namespace AlvorKit;

[TestClass]
public class RootPngsTest
{
    /// <summary>Indexed PNG transparency preserves palette alpha and straight RGB while reversing rows for OpenGL.</summary>
    [TestMethod]
    public void Read_PreservesPaletteTransparencyAndRowOrder()
    {
        // Two-bit palette: transparent black, opaque black, half-transparent red, opaque blue, in top-down row order.
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACAgMAAAAP2OW3AAAADFBMVEUAAAAAAAD/AAAAAP/8L3PX" +
            "AAAABHRSTlMA/4D/G4olsQAAAAxJREFUeJxjEGDYAAAA5ADBJ6joVwAAAABJRU5ErkJggg==");
        using var stream = new MemoryStream(png);
        var image = new RootPngs().Read(stream);

        Assert.AreEqual(new Vec2i(2, 2), image.Size);
        CollectionAssert.AreEqual(
            new Vec4u8[] { (255, 0, 0, 128), (0, 0, 255, 255), (0, 0, 0, 0), (0, 0, 0, 255) },
            image.Pixels.ToArray());
    }

    /// <summary>PNG decoding preserves opaque RGB pixels and leaves the caller's stream open.</summary>
    [TestMethod]
    public void Read_DecodesPngPixelsWithoutOwningStream()
    {
        var png = PngBuilder.Create(2, 1, false);
        png.SetPixel(1, 2, 3, 0, 0);
        png.SetPixel(4, 5, 6, 1, 0);
        using var stream = new MemoryStream();
        png.Save(stream);
        stream.Position = 0;
        var image = new RootPngs().Read(stream);

        Assert.AreEqual(new Vec2i(2, 1), image.Size);
        CollectionAssert.AreEqual(
            new Vec4u8[] { (1, 2, 3, 255), (4, 5, 6, 255) },
            image.Pixels.ToArray());
        Assert.IsTrue(stream.CanRead);
    }
}
