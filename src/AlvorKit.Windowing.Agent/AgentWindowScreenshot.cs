namespace AlvorKit.Windowing;

/// <summary>Saves agent-host framebuffer captures for visual inspection.</summary>
/// <param name="gl">The OpenGL layer owned by the agent host after construction.</param>
/// <param name="save">Optional save callback for tests that should avoid framebuffer reads.</param>
[ExcludeFromCodeCoverage(Justification = "Reads a native OpenGL framebuffer and writes a PNG file.")]
internal sealed class AgentWindowScreenshot(GlLayer gl, Action<GlLayer, Vec2u, string>? save = null) : IDisposable
{
    private readonly Action<GlLayer, Vec2u, string> save = save ?? SaveFramebuffer;
    private bool disposed;

    /// <summary>Reads the current framebuffer and saves it as an RGB PNG.</summary>
    internal void Save(Vec2u size, string path) => save(gl, size, path);

    /// <summary>Disposes the OpenGL layer after the host is done with the window context.</summary>
    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        gl.Dispose();
    }

    /// <summary>Reads the current framebuffer and saves it as an RGB PNG.</summary>
    private static unsafe void SaveFramebuffer(GlLayer gl, Vec2u size, string path)
    {
        var width = checked((int)Math.Max(1u, size.X));
        var height = checked((int)Math.Max(1u, size.Y));
        var pixels = new byte[width * height * 4];

        fixed (byte* pointer = pixels)
            gl.ReadPixels(0, 0, width, height, GlPixelFormat.Rgba, GlPixelType.UnsignedByte, (nint)pointer);

        SavePng(pixels, width, height, path);
    }

    /// <summary>Encodes already-read RGBA framebuffer bytes as a PNG file.</summary>
    private static void SavePng(byte[] pixels, int width, int height, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var png = PngBuilder.Create(width, height, false);
        for (var y = 0; y < height; y++)
            WriteRow(png, pixels, width, height, y);

        using var stream = File.Create(path);
        png.Save(stream);
    }

    /// <summary>Copies one vertically flipped framebuffer row into the PNG builder.</summary>
    private static void WriteRow(PngBuilder png, byte[] pixels, int width, int height, int y)
    {
        var sourceY = height - 1 - y;
        var row = sourceY * width * 4;
        for (var x = 0; x < width; x++)
        {
            var pixel = row + x * 4;
            png.SetPixel(pixels[pixel], pixels[pixel + 1], pixels[pixel + 2], x, y);
        }
    }
}
