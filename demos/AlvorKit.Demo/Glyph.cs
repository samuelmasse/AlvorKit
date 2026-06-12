using AlvorKit.FreeType;
using BigGustave;
using System.Runtime.InteropServices;

namespace AlvorKit.Demo;

/// <summary>A glyph rasterized by FreeType into tight grayscale rows.</summary>
public sealed class GlyphBitmap
{
    private GlyphBitmap(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }

    public static GlyphBitmap Render(Ft ft, string fontPath, char character, uint pixelHeight)
    {
        ThrowIfError(ft.InitFreeType(out var freetype), "Failed to initialize FreeType.");
        try
        {
            ThrowIfError(ft.NewFace(freetype, fontPath, new(0), out var face), $"Failed to load font: {fontPath}");
            try
            {
                ThrowIfError(ft.SetPixelSizes(face, 0, pixelHeight), $"Failed to set glyph size: {pixelHeight}");
                ThrowIfError(ft.LoadChar(face, new(character), Ft.LoadRender), $"Failed to render glyph: {character}");

                var faceRec = Marshal.PtrToStructure<FtFaceRec>(face);
                var glyphSlot = Marshal.PtrToStructure<FtGlyphSlotRec>(faceRec.Glyph);
                var bitmap = glyphSlot.Bitmap;
                var width = (int)bitmap.Width;
                var height = (int)bitmap.Rows;
                var pixels = new byte[width * height];

                for (var y = 0; y < height; y++)
                    Marshal.Copy(bitmap.Buffer + y * bitmap.Pitch, pixels, y * width, width);

                return new GlyphBitmap(width, height, pixels);
            }
            finally
            {
                ft.DoneFace(face);
            }
        }
        finally
        {
            ft.DoneFreeType(freetype);
        }
    }

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

    private static void ThrowIfError(int result, string message)
    {
        if (result != 0)
            throw new InvalidOperationException(message);
    }
}
