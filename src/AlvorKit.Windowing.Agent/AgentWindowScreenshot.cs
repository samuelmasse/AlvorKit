namespace AlvorKit;

/// <summary>Saves agent-host framebuffer captures for visual inspection.</summary>
/// <param name="gl">The OpenGL layer used to read framebuffer pixels.</param>
/// <param name="save">Optional save callback for tests that should avoid framebuffer reads.</param>
[ExcludeFromCodeCoverage(Justification = "Reads a native OpenGL framebuffer and writes a PNG file.")]
internal sealed class AgentWindowScreenshot(GlLayer gl, Action<GlLayer, Vec2u, string>? save = null)
{
    private readonly Action<GlLayer, Vec2u, string> save = save ?? SaveFramebuffer;

    /// <summary>Reads the current framebuffer and saves it as an RGBA PNG.</summary>
    internal void Save(Vec2u size, string path) => save(gl, size, path);

    /// <summary>Reads the current framebuffer and returns it as RGBA PNG bytes.</summary>
    internal byte[] Capture(Vec2u size)
    {
        var width = ((int)Math.Max(1u, size.X));
        var height = ((int)Math.Max(1u, size.Y));
        var pixels = ReadFramebuffer(gl, width, height);
        return EncodePng(pixels, width, height);
    }

    /// <summary>Reads the current framebuffer and saves it as an RGBA PNG.</summary>
    private static void SaveFramebuffer(GlLayer gl, Vec2u size, string path)
    {
        var width = ((int)Math.Max(1u, size.X));
        var height = ((int)Math.Max(1u, size.Y));
        var pixels = ReadFramebuffer(gl, width, height);
        SavePng(pixels, width, height, path);
    }

    /// <summary>Reads one RGBA framebuffer into bottom-up byte storage.</summary>
    private static byte[] ReadFramebuffer(GlLayer gl, int width, int height)
    {
        var pixels = new byte[width * height * 4];
        Vec2u readSize = ((uint)width, (uint)height);
        gl.ReadPixels(readSize, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, pixels);
        return pixels;
    }

    /// <summary>Encodes already-read RGBA framebuffer bytes as a PNG file.</summary>
    private static void SavePng(byte[] pixels, int width, int height, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllBytes(path, EncodePng(pixels, width, height));
    }

    /// <summary>Encodes bottom-up RGBA framebuffer bytes as top-down PNG bytes.</summary>
    internal static byte[] EncodePng(byte[] pixels, int width, int height)
    {
        var png = PngBuilder.Create(width, height, true);
        for (var y = 0; y < height; y++)
            WriteRow(png, pixels, width, height, y);

        using var stream = new MemoryStream();
        png.Save(stream);
        return stream.ToArray();
    }

    /// <summary>Copies one vertically flipped framebuffer row into the PNG builder.</summary>
    private static void WriteRow(PngBuilder png, byte[] pixels, int width, int height, int y)
    {
        var sourceY = height - 1 - y;
        var row = sourceY * width * 4;
        for (var x = 0; x < width; x++)
        {
            var pixel = row + x * 4;
            png.SetPixel(
                new Pixel(
                    pixels[pixel],
                    pixels[pixel + 1],
                    pixels[pixel + 2],
                    pixels[pixel + 3],
                    false),
                x,
                y);
        }
    }
}
