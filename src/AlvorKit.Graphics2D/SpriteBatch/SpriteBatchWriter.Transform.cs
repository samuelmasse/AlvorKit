namespace AlvorKit;

public partial class SpriteBatchWriter
{
    /// <summary>
    /// Draws a source rectangle with an affine transform from local sprite pixels to canvas pixels.
    /// The local origin is the source rectangle's top-left corner; flips affect texture coordinates only.
    /// Clipping interpolates both position and texture coordinates and does not allocate managed objects.
    /// </summary>
    public void DrawTransformed(Texture texture, Box2 source, Mat4 transform, Vec4 color, SpriteBatchFlip flip)
    {
        var size = source.Size;

        if (size.X <= 0 || size.Y <= 0)
            return;

        var tex = TexCorners(source.Min, size, SpriteBatchRotation.None, flip);
        Span<PolygonVertex> polygon = stackalloc PolygonVertex[PolygonClipCapacity];
        polygon[0] = TransformVertex(Vec2.Zero, tex.TopLeft, transform);
        polygon[1] = TransformVertex(new Vec2(size.X, 0), tex.TopRight, transform);
        polygon[2] = TransformVertex(size, tex.BottomRight, transform);
        polygon[3] = TransformVertex(new Vec2(0, size.Y), tex.BottomLeft, transform);

        if (!clip.HasValue)
        {
            DrawPolygonQuad(texture, color, polygon[0], polygon[1], polygon[3], polygon[2]);
            return;
        }

        var bounds = clip.Value;

        if (bounds.Min.X >= bounds.Max.X || bounds.Min.Y >= bounds.Max.Y)
            return;

        var count = ClipPolygon(polygon, 4, bounds);

        for (var i = 2; i < count; i++)
            DrawPolygonTriangle(texture, color, polygon[0], polygon[i - 1], polygon[i]);
    }

    /// <summary>Transforms one local sprite corner while preserving its texture-space coordinate.</summary>
    private static PolygonVertex TransformVertex(Vec2 position, Vec2 texCoord, Mat4 transform) =>
        new((Vec2)Mat4.TransformPoint(transform, new Vec3(position, 0)), texCoord);
}
