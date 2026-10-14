namespace AlvorKit;

public partial class SpriteBatchWriter
{
    /// <summary>Draws a one-pixel white line.</summary>
    public void DrawLine(Vec2 start, Vec2 end) => DrawLine(texture, start, end, 1f, Vec4.One);

    /// <summary>Draws a tinted line with the default white texture.</summary>
    public void DrawLine(Vec2 start, Vec2 end, float width, Vec4 color) => DrawLine(texture, start, end, width, color);

    /// <summary>Draws a one-pixel textured line.</summary>
    public void DrawLine(Texture texture, Vec2 start, Vec2 end) => DrawLine(texture, start, end, 1f, Vec4.One);

    /// <summary>Draws a tinted textured line with the supplied edge offset.</summary>
    public void DrawLine(Texture texture, Vec2 start, Vec2 end, float width, Vec4 color)
    {
        var delta = end - start;
        if (width <= 0f || delta.LengthSquared <= 0f)
            return;

        var normal = Vec2.Normalize((-delta.Y, delta.X));
        var endLeft = new PolygonVertex(end - (normal * width), Vec2.Zero);
        var endRight = new PolygonVertex(end + (normal * width), (texture.Size.X, 0f));
        var startLeft = new PolygonVertex(start - (normal * width), (0f, texture.Size.Y));
        var startRight = new PolygonVertex(start + (normal * width), texture.Size);

        if (!clip.HasValue)
        {
            DrawPolygonQuad(texture, color, endLeft, endRight, startLeft, startRight);
            return;
        }

        var clipValue = clip.Value;
        if (clipValue.Min.X >= clipValue.Max.X || clipValue.Min.Y >= clipValue.Max.Y)
            return;

        Span<PolygonVertex> polygon = stackalloc PolygonVertex[PolygonClipCapacity];
        polygon[0] = endLeft;
        polygon[1] = endRight;
        polygon[2] = startRight;
        polygon[3] = startLeft;

        var count = ClipPolygon(polygon, 4, clipValue);
        if (count < 3)
            return;

        for (var i = 2; i < count; i++)
            DrawPolygonTriangle(texture, color, polygon[0], polygon[i - 1], polygon[i]);
    }

    /// <summary>Emits a textured quad in the vertex order expected by the shared quad index buffer.</summary>
    private void DrawPolygonQuad(Texture texture, Vec4 color, PolygonVertex endLeft, PolygonVertex endRight,
        PolygonVertex startLeft, PolygonVertex startRight)
    {
        AddPolygonVertex(texture, color, endLeft);
        AddPolygonVertex(texture, color, endRight);
        AddPolygonVertex(texture, color, startLeft);
        AddPolygonVertex(texture, color, startRight);
    }

    /// <summary>Emits one clipped triangle as a degenerate quad so the existing quad index buffer can draw it.</summary>
    private void DrawPolygonTriangle(Texture texture, Vec4 color, PolygonVertex a, PolygonVertex b, PolygonVertex c)
    {
        AddPolygonVertex(texture, color, a);
        AddPolygonVertex(texture, color, a);
        AddPolygonVertex(texture, color, c);
        AddPolygonVertex(texture, color, b);
    }

    /// <summary>Adds a polygon vertex after converting canvas pixels and texture pixels to shader coordinates.</summary>
    private void AddPolygonVertex(Texture texture, Vec4 color, PolygonVertex vertex) =>
        vertices.Add(texture, new SpriteBatchVertex(NormalizePosition(vertex.Position), color, TexCoord(texture, vertex.TexCoord)));
}
