namespace AlvorKit.Graphics2D;

public partial class SpriteBatchWriter
{
    /// <summary>Generates vertices for one sprite, applying clipping, rotation, and flips.</summary>
    private void DrawVertices(
        Texture texture,
        Vector2 position,
        Vector2 size,
        Vector4 color,
        Vector2 subPosition,
        Vector2 subSize,
        SpriteBatchRotation rotation,
        SpriteBatchFlip flip)
    {
        if (size.X <= 0f || size.Y <= 0f || subSize.X <= 0f || subSize.Y <= 0f)
            return;

        var unclipped = new SpriteBatchClip(position, position + size);
        var clipped = unclipped;

        if (clip.HasValue)
        {
            var clippedMin = Vector2.Max(unclipped.Min, clip.Value.Min);
            var clippedMax = Vector2.Min(unclipped.Max, clip.Value.Max);

            if (clippedMin.X >= clippedMax.X || clippedMin.Y >= clippedMax.Y)
                return;

            clipped = new SpriteBatchClip(clippedMin, clippedMax);
        }

        var leftT = (clipped.Min.X - unclipped.Min.X) / size.X;
        var rightT = (clipped.Max.X - unclipped.Min.X) / size.X;
        var topT = (clipped.Min.Y - unclipped.Min.Y) / size.Y;
        var bottomT = (clipped.Max.Y - unclipped.Min.Y) / size.Y;

        var texCorners = TexCorners(subPosition, subSize, rotation, flip);
        var texCoords = ClipCorners(texCorners, leftT, rightT, topT, bottomT);

        var normPosition = NormalizePosition(clipped.Min);
        var normSize = NormalizeSize(clipped.Size);
        var left = normPosition.X;
        var right = left + normSize.X;
        var top = normPosition.Y;
        var bottom = top - normSize.Y;

        DrawQuad(new Vector2(left, top), new Vector2(right, top), new Vector2(left, bottom), new Vector2(right, bottom), texture, color, texCoords);
    }

    /// <summary>Emits four vertices for one normalized quad.</summary>
    private void DrawQuad(Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4, Texture texture, Vector4 color, QuadCorners texCoords)
    {
        vertices.Add(texture, new SpriteBatchVertex(pos1, color, TexCoord(texture, texCoords.TopLeft)));
        vertices.Add(texture, new SpriteBatchVertex(pos2, color, TexCoord(texture, texCoords.TopRight)));
        vertices.Add(texture, new SpriteBatchVertex(pos3, color, TexCoord(texture, texCoords.BottomLeft)));
        vertices.Add(texture, new SpriteBatchVertex(pos4, color, TexCoord(texture, texCoords.BottomRight)));
    }

    /// <summary>Converts a texture-space pixel coordinate into a normalized sampler coordinate.</summary>
    private static Vector2 TexCoord(Texture texture, Vector2 position) =>
        new(position.X / texture.Size.X, 1f - (position.Y / texture.Size.Y));

    /// <summary>Normalizes a canvas pixel coordinate into clip space.</summary>
    private Vector2 NormalizePosition(Vector2 position) => (position * new Vector2(1f, -1f) / (canvas.Size / 2f)) - new Vector2(1f, -1f);

    /// <summary>Normalizes a canvas pixel size into clip-space dimensions.</summary>
    private Vector2 NormalizeSize(Vector2 size) => size / (canvas.Size / 2f);
}
