namespace AlvorKit.Engine;

/// <summary>Loads PNG files into bottom-left-origin RGBA image data for OpenGL uploads.</summary>
[Root]
public sealed class RootPngs
{
    /// <summary>Decodes the PNG at <paramref name="file"/>.</summary>
    public ImageData this[string file]
    {
        get
        {
            var image = Png.Open(RootAssetFiles.Resolve(file));
            return new((checked((uint)image.Width), checked((uint)image.Height)), GetPixels(image));
        }
    }

    private static (byte, byte, byte, byte)[] GetPixels(Png image)
    {
        var pixels = new (byte, byte, byte, byte)[image.Width * image.Height];
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                pixels[(image.Height - 1 - y) * image.Width + x] = (pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        return pixels;
    }
}
