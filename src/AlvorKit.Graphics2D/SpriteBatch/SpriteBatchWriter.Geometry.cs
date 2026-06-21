namespace AlvorKit.Graphics2D;

public partial class SpriteBatchWriter
{
    /// <summary>Generates vertices for one sprite, applying clipping, rotation, and flips.</summary>
    private void DrawVertices(
        Texture texture,
        Vec2 position,
        Vec2 size,
        Vec4 color,
        Vec2 subPosition,
        Vec2 subSize,
        SpriteBatchRotation rotation,
        SpriteBatchFlip flip)
    {
        if (size.X <= 0f || size.Y <= 0f || subSize.X <= 0f || subSize.Y <= 0f)
            return;

        var unclipped = new SpriteBatchClip(position, position + size);
        var clipped = unclipped;

        if (clip.HasValue)
        {
            var clippedMin = Vec2.Max(unclipped.Min, clip.Value.Min);
            var clippedMax = Vec2.Min(unclipped.Max, clip.Value.Max);

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

        DrawQuad((left, top), (right, top), (left, bottom), (right, bottom), texture, color, texCoords);
    }

    /// <summary>Emits four vertices for one normalized quad.</summary>
    private void DrawQuad(Vec2 pos1, Vec2 pos2, Vec2 pos3, Vec2 pos4, Texture texture, Vec4 color, QuadCorners texCoords)
    {
        vertices.Add(texture, new SpriteBatchVertex(pos1, color, TexCoord(texture, texCoords.TopLeft)));
        vertices.Add(texture, new SpriteBatchVertex(pos2, color, TexCoord(texture, texCoords.TopRight)));
        vertices.Add(texture, new SpriteBatchVertex(pos3, color, TexCoord(texture, texCoords.BottomLeft)));
        vertices.Add(texture, new SpriteBatchVertex(pos4, color, TexCoord(texture, texCoords.BottomRight)));
    }

    /// <summary>Converts a texture-space pixel coordinate into a normalized sampler coordinate.</summary>
    private static Vec2 TexCoord(Texture texture, Vec2 position) =>
        (position.X / texture.Size.X, 1f - (position.Y / texture.Size.Y));

    /// <summary>Normalizes a canvas pixel coordinate into clip space.</summary>
    private Vec2 NormalizePosition(Vec2 position)
    {
        Vec2 yFlip = (1f, -1f);
        return (position * yFlip / (canvas.Size / 2f)) - yFlip;
    }

    /// <summary>Normalizes a canvas pixel size into clip-space dimensions.</summary>
    private Vec2 NormalizeSize(Vec2 size) => size / (canvas.Size / 2f);
}
