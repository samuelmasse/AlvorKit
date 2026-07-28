namespace AlvorKit.Windowing.Test;

[TestClass]
public sealed class AgentWindowScreenshotTest
{
    /// <summary>PNG encoding preserves every RGBA channel in a high-color framebuffer.</summary>
    [TestMethod]
    public void EncodePng_PreservesHighColorFramebuffer()
    {
        const int width = 1024;
        var pixels = new byte[width * 4];
        for (var x = 0; x < width; x++)
        {
            var offset = x * 4;
            pixels[offset] = (byte)x;
            pixels[offset + 1] = (byte)(x >> 8);
            pixels[offset + 2] = (byte)(255 - x);
            pixels[offset + 3] = (byte)(x * 17);
        }

        var bytes = AgentWindowScreenshot.EncodePng(pixels, width, 1);

        using var stream = new MemoryStream(bytes);
        var png = BigGustave.Png.Open(stream);
        for (var x = 0; x < width; x++)
        {
            var pixel = png.GetPixel(x, 0);
            Assert.AreEqual((byte)x, pixel.R);
            Assert.AreEqual((byte)(x >> 8), pixel.G);
            Assert.AreEqual((byte)(255 - x), pixel.B);
            Assert.AreEqual((byte)(x * 17), pixel.A);
        }
    }
}
