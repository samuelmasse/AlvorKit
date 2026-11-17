namespace AlvorKit;

/// <summary>Decodes PNG streams into bottom-left-origin RGBA image data for OpenGL uploads.</summary>
[Root]
public class RootPngs
{
    /// <summary>Decodes a PNG without taking ownership of the stream.</summary>
    public ImageData Read(Stream stream)
    {
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var source = MemoryMarshal.Cast<byte, Vec4u8>(image.Data.AsSpan());
        var pixels = new Vec4u8[image.Width * image.Height];

        for (var y = 0; y < image.Height; y++)
            source.Slice(y * image.Width, image.Width).CopyTo(pixels.AsSpan((image.Height - 1 - y) * image.Width));

        return new(new(image.Width, image.Height), pixels);
    }
}
