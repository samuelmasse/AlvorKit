namespace AlvorKit.Graphics2D;

/// <summary>Axis-aligned clipping rectangle for sprite-batch draw calls.</summary>
/// <param name="Min">The inclusive minimum pixel coordinate.</param>
/// <param name="Max">The exclusive maximum pixel coordinate.</param>
public readonly record struct SpriteBatchClip(Vec2 Min, Vec2 Max)
{
    /// <summary>Creates a clip rectangle from scalar bounds.</summary>
    public SpriteBatchClip(float minX, float minY, float maxX, float maxY)
        : this((minX, minY), (maxX, maxY))
    {
    }

    /// <summary>Gets the clip rectangle size in pixels.</summary>
    public Vec2 Size => Max - Min;
}
