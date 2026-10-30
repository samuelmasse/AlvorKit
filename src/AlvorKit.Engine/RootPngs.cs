namespace AlvorKit;

/// <summary>Loads PNG files into bottom-left-origin RGBA image data for OpenGL uploads.</summary>
[Root]
public class RootPngs
{
    /// <summary>Decodes the PNG at <paramref name="file"/>.</summary>
    public ImageData this[string file]
    {
        get
        {
            using var stream = File.OpenRead(file);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            var source = MemoryMarshal.Cast<byte, Vec4u8>(image.Data.AsSpan());
            var pixels = new Vec4u8[image.Width * image.Height];

            for (var y = 0; y < image.Height; y++)
                source.Slice(y * image.Width, image.Width).CopyTo(pixels.AsSpan((image.Height - 1 - y) * image.Width));

            return new(new(image.Width, image.Height), pixels);
        }
    }
}
