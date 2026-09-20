namespace AlvorKit;

/// <summary>Decoded RGBA image pixels and their dimensions.</summary>
public record struct ImageData(Vec2i Size, ReadOnlyMemory<Vec4u8> Pixels)
{
    /// <summary>Returns a copy with reversed row order, preserving dimensions and each row's pixel order.</summary>
    /// <remarks>Allocates a new pixel buffer for asset loading; the source pixels remain unchanged.</remarks>
    public readonly ImageData FlippedVertically()
    {
        var source = Pixels.Span;
        var pixels = new Vec4u8[source.Length];

        for (var y = 0; y < Size.Y; y++)
        {
            var sourceRow = (Size.Y - 1 - y) * Size.X;
            var targetRow = y * Size.X;
            source.Slice(sourceRow, Size.X).CopyTo(pixels.AsSpan(targetRow, Size.X));
        }

        return new(Size, pixels);
    }
}
