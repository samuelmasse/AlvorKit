namespace AlvorKit.Graphics2D;

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
        var endLeft = new LineVertex(end - (normal * width), Vec2.Zero);
        var endRight = new LineVertex(end + (normal * width), (texture.Size.X, 0f));
        var startLeft = new LineVertex(start - (normal * width), (0f, texture.Size.Y));
        var startRight = new LineVertex(start + (normal * width), texture.Size);

        if (!clip.HasValue)
        {
            DrawLineQuad(texture, color, endLeft, endRight, startLeft, startRight);
            return;
        }

        var clipValue = clip.Value;
        if (clipValue.Min.X >= clipValue.Max.X || clipValue.Min.Y >= clipValue.Max.Y)
            return;

        Span<LineVertex> polygon = stackalloc LineVertex[LineClipCapacity];
        polygon[0] = endLeft;
        polygon[1] = endRight;
        polygon[2] = startRight;
        polygon[3] = startLeft;

        var count = ClipLinePolygon(polygon, 4, clipValue);
        if (count < 3)
            return;

        for (var i = 2; i < count; i++)
            DrawLineTriangle(texture, color, polygon[0], polygon[i - 1], polygon[i]);
    }

    /// <summary>Emits one line quad in the vertex order expected by the shared quad index buffer.</summary>
    private void DrawLineQuad(Texture texture, Vec4 color, LineVertex endLeft, LineVertex endRight, LineVertex startLeft, LineVertex startRight)
    {
        AddLineVertex(texture, color, endLeft);
        AddLineVertex(texture, color, endRight);
        AddLineVertex(texture, color, startLeft);
        AddLineVertex(texture, color, startRight);
    }

    /// <summary>Emits one clipped triangle as a degenerate quad so the existing quad index buffer can draw it.</summary>
    private void DrawLineTriangle(Texture texture, Vec4 color, LineVertex a, LineVertex b, LineVertex c)
    {
        AddLineVertex(texture, color, a);
        AddLineVertex(texture, color, a);
        AddLineVertex(texture, color, b);
        AddLineVertex(texture, color, c);
    }

    /// <summary>Adds one line vertex after converting canvas pixels and texture pixels to shader coordinates.</summary>
    private void AddLineVertex(Texture texture, Vec4 color, LineVertex vertex) =>
        vertices.Add(texture, new SpriteBatchVertex(NormalizePosition(vertex.Position), color, TexCoord(texture, vertex.TexCoord)));
}
