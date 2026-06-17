namespace AlvorKit.Graphics2D;

public partial class SpriteBatchWriter
{
    /// <summary>The maximum vertex count produced by clipping a line quad against an axis-aligned rectangle.</summary>
    private const int LineClipCapacity = 8;

    /// <summary>Identifies one clip-rectangle edge for line polygon clipping.</summary>
    private enum LineClipEdge : byte
    {
        /// <summary>The inclusive minimum x edge.</summary>
        MinX,

        /// <summary>The exclusive maximum x edge.</summary>
        MaxX,

        /// <summary>The inclusive minimum y edge.</summary>
        MinY,

        /// <summary>The exclusive maximum y edge.</summary>
        MaxY
    }

    /// <summary>One unclipped or clipped line polygon vertex in canvas and texture space.</summary>
    private readonly struct LineVertex(Vector2 position, Vector2 texCoord)
    {
        /// <summary>Gets the canvas-space pixel position.</summary>
        internal Vector2 Position => position;

        /// <summary>Gets the texture-space pixel coordinate.</summary>
        internal Vector2 TexCoord => texCoord;
    }

    /// <summary>Clips a line quad polygon in place against the supplied sprite-batch clip rectangle.</summary>
    private static int ClipLinePolygon(Span<LineVertex> polygon, int count, SpriteBatchClip clip)
    {
        Span<LineVertex> scratch = stackalloc LineVertex[LineClipCapacity];

        count = ClipLinePolygonEdge(polygon[..count], scratch, LineClipEdge.MinX, clip.Min.X);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);

        count = ClipLinePolygonEdge(polygon[..count], scratch, LineClipEdge.MaxX, clip.Max.X);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);

        count = ClipLinePolygonEdge(polygon[..count], scratch, LineClipEdge.MinY, clip.Min.Y);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);

        count = ClipLinePolygonEdge(polygon[..count], scratch, LineClipEdge.MaxY, clip.Max.Y);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);
        return count;
    }

    /// <summary>Clips a polygon against one clip edge, preserving interpolated texture coordinates.</summary>
    private static int ClipLinePolygonEdge(ReadOnlySpan<LineVertex> input, Span<LineVertex> output, LineClipEdge edge, float boundary)
    {
        var count = 0;
        var previous = input[^1];
        var previousInside = IsInsideLineClip(previous.Position, edge, boundary);

        for (var i = 0; i < input.Length; i++)
        {
            var current = input[i];
            var currentInside = IsInsideLineClip(current.Position, edge, boundary);

            if (currentInside)
            {
                if (!previousInside)
                    output[count++] = IntersectLineClip(previous, current, edge, boundary);

                output[count++] = current;
            }
            else if (previousInside)
            {
                output[count++] = IntersectLineClip(previous, current, edge, boundary);
            }

            previous = current;
            previousInside = currentInside;
        }

        return count;
    }

    /// <summary>Returns whether a canvas-space position is inside one clip edge.</summary>
    private static bool IsInsideLineClip(Vector2 position, LineClipEdge edge, float boundary) =>
        edge switch
        {
            LineClipEdge.MinX => position.X >= boundary,
            LineClipEdge.MaxX => position.X <= boundary,
            LineClipEdge.MinY => position.Y >= boundary,
            _ => position.Y <= boundary
        };

    /// <summary>Finds the edge intersection between two line-polygon vertices.</summary>
    private static LineVertex IntersectLineClip(LineVertex a, LineVertex b, LineClipEdge edge, float boundary)
    {
        var delta = b.Position - a.Position;
        var divisor = edge is LineClipEdge.MinX or LineClipEdge.MaxX ? delta.X : delta.Y;
        var distance = edge is LineClipEdge.MinX or LineClipEdge.MaxX ? boundary - a.Position.X : boundary - a.Position.Y;
        var t = distance / divisor;
        return new LineVertex(a.Position + (delta * t), a.TexCoord + ((b.TexCoord - a.TexCoord) * t));
    }
}
