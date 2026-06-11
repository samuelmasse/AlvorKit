using AlvorKit.FreeType;
using BigGustave;

namespace AlvorKit.Demo;

/// <summary>A glyph rasterized by FreeType into tight grayscale rows.</summary>
public record Glyph(int Width, int Height, byte[] Pixels)
{
    public static Glyph Render(string fontPath, char character, uint pixelHeight)
    {
        if (Ft.InitFreeType(out var freetype) != 0)
            throw new InvalidOperationException("Failed to initialize FreeType.");
        try
        {
            if (Ft.NewFace(freetype, fontPath, new(0), out var face) != 0)
                throw new InvalidOperationException($"Failed to load font: {fontPath}");
            try
            {
                if (Ft.SetPixelSizes(face, 0, pixelHeight) != 0 || Ft.LoadChar(face, new(character), Ft.LoadRender) != 0)
                    throw new InvalidOperationException($"Failed to render glyph: {character}");

                var bitmap = Marshal.PtrToStructure<FtGlyphSlotRec>(Marshal.PtrToStructure<FtFaceRec>(face).Glyph).Bitmap;
                var (width, height) = ((int)bitmap.Width, (int)bitmap.Rows);
                var pixels = new byte[width * height];
                for (var y = 0; y < height; y++)
                    Marshal.Copy(bitmap.Buffer + y * bitmap.Pitch, pixels, y * width, width);
                return new(width, height, pixels);
            }
            finally
            {
                Ft.DoneFace(face);
            }
        }
        finally
        {
            Ft.DoneFreeType(freetype);
        }
    }

    public void ExportPng(string path)
    {
        var png = PngBuilder.Create(Width, Height, false);
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var value = Pixels[y * Width + x];
                png.SetPixel(new Pixel(value, value, value), x, y);
            }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path);
        png.Save(stream);
    }
}
