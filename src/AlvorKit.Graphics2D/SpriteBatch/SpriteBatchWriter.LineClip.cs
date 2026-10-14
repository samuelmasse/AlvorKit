namespace AlvorKit;

public partial class SpriteBatchWriter
{
    /// <summary>The maximum vertex count produced by clipping a line quad against an axis-aligned rectangle.</summary>
    private const int PolygonClipCapacity = 8;

    /// <summary>Identifies one clip-rectangle edge for line polygon clipping.</summary>
    private enum PolygonClipEdge : byte
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
    private readonly struct PolygonVertex(Vec2 position, Vec2 texCoord)
    {
        /// <summary>Gets the canvas-space pixel position.</summary>
        internal Vec2 Position => position;

        /// <summary>Gets the texture-space pixel coordinate.</summary>
        internal Vec2 TexCoord => texCoord;
    }

    /// <summary>Clips a line quad polygon in place against the supplied sprite-batch clip rectangle.</summary>
    private static int ClipPolygon(Span<PolygonVertex> polygon, int count, SpriteBatchClip clip)
    {
        Span<PolygonVertex> scratch = stackalloc PolygonVertex[PolygonClipCapacity];

        count = ClipPolygonEdge(polygon[..count], scratch, PolygonClipEdge.MinX, clip.Min.X);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);

        count = ClipPolygonEdge(polygon[..count], scratch, PolygonClipEdge.MaxX, clip.Max.X);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);

        count = ClipPolygonEdge(polygon[..count], scratch, PolygonClipEdge.MinY, clip.Min.Y);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);

        count = ClipPolygonEdge(polygon[..count], scratch, PolygonClipEdge.MaxY, clip.Max.Y);
        if (count == 0)
            return 0;
        scratch[..count].CopyTo(polygon);
        return count;
    }

    /// <summary>Clips a polygon against one clip edge, preserving interpolated texture coordinates.</summary>
    private static int ClipPolygonEdge(ReadOnlySpan<PolygonVertex> input, Span<PolygonVertex> output, PolygonClipEdge edge, float boundary)
    {
        var count = 0;
        var previous = input[^1];
        var previousInside = IsInsidePolygonClip(previous.Position, edge, boundary);

        for (var i = 0; i < input.Length; i++)
        {
            var current = input[i];
            var currentInside = IsInsidePolygonClip(current.Position, edge, boundary);

            if (currentInside)
            {
                if (!previousInside)
                    output[count++] = IntersectPolygonClip(previous, current, edge, boundary);

                output[count++] = current;
            }
            else if (previousInside)
            {
                output[count++] = IntersectPolygonClip(previous, current, edge, boundary);
            }

            previous = current;
            previousInside = currentInside;
        }

        return count;
    }

    /// <summary>Returns whether a canvas-space position is inside one clip edge.</summary>
    private static bool IsInsidePolygonClip(Vec2 position, PolygonClipEdge edge, float boundary) =>
        edge switch
        {
            PolygonClipEdge.MinX => position.X >= boundary,
            PolygonClipEdge.MaxX => position.X <= boundary,
            PolygonClipEdge.MinY => position.Y >= boundary,
            _ => position.Y <= boundary
        };

    /// <summary>Finds the edge intersection between two line-polygon vertices.</summary>
    private static PolygonVertex IntersectPolygonClip(PolygonVertex a, PolygonVertex b, PolygonClipEdge edge, float boundary)
    {
        var delta = b.Position - a.Position;
        var divisor = edge is PolygonClipEdge.MinX or PolygonClipEdge.MaxX ? delta.X : delta.Y;
        var distance = edge is PolygonClipEdge.MinX or PolygonClipEdge.MaxX ? boundary - a.Position.X : boundary - a.Position.Y;
        var t = distance / divisor;
        return new PolygonVertex(a.Position + (delta * t), a.TexCoord + ((b.TexCoord - a.TexCoord) * t));
    }
}
