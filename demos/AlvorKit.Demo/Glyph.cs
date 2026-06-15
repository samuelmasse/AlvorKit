using AlvorKit.FreeType;
using BigGustave;

namespace AlvorKit.Demo;

/// <summary>A glyph rasterized by FreeType into tight grayscale rows.</summary>
public sealed class GlyphBitmap
{
    /// <summary>Stores a copied grayscale bitmap owned by managed memory.</summary>
    /// <param name="width">The number of pixels in each row.</param>
    /// <param name="height">The number of rows in the bitmap.</param>
    /// <param name="pixels">The tightly packed grayscale pixels copied out of FreeType.</param>
    private GlyphBitmap(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    /// <summary>The number of pixels in each row.</summary>
    public int Width { get; }

    /// <summary>The number of rows in the bitmap.</summary>
    public int Height { get; }

    /// <summary>The tightly packed grayscale pixels copied out of FreeType.</summary>
    public byte[] Pixels { get; }

    /// <summary>Rasterizes one character through FreeType and returns a managed copy of the bitmap.</summary>
    /// <param name="ft">The FreeType API used for the font calls.</param>
    /// <param name="fontPath">The font file loaded for this demo run.</param>
    /// <param name="character">The character to render.</param>
    /// <param name="pixelHeight">The requested pixel height for the glyph.</param>
    public static unsafe GlyphBitmap Render(Ft ft, string fontPath, char character, uint pixelHeight)
    {
        nint library = 0;
        FtFaceRec* face = null;

        try
        {
            ft.InitFreeType(out library);
            ft.NewFace(library, fontPath, new(0), out face);
            ft.SetPixelSizes(face, 0, pixelHeight);
            ft.LoadChar(face, character, Ft.LoadRender);

            var bitmap = face->Glyph->Bitmap;
            var width = (int)bitmap.Width;
            var height = (int)bitmap.Rows;
            var pixels = new byte[width * height];

            for (var y = 0; y < height; y++)
                Marshal.Copy(bitmap.Buffer + y * bitmap.Pitch, pixels, y * width, width);

            return new GlyphBitmap(width, height, pixels);
        }
        finally
        {
            if (face != null)
                ft.DoneFace(face);
            if (library != 0)
                ft.DoneFreeType(library);
        }
    }

    /// <summary>Exports the managed grayscale pixels as a PNG file for quick inspection.</summary>
    /// <param name="path">The output PNG path; parent directories are created during export.</param>
    public void ExportPng(string path)
    {
        var png = PngBuilder.Create(Width, Height, false);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var value = Pixels[y * Width + x];
                png.SetPixel(new Pixel(value, value, value), x, y);
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path);
        png.Save(stream);
    }
}
