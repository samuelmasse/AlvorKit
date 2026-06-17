namespace AlvorKit.Graphics2D;

/// <summary>Axis-aligned clipping rectangle for sprite-batch draw calls.</summary>
/// <param name="Min">The inclusive minimum pixel coordinate.</param>
/// <param name="Max">The exclusive maximum pixel coordinate.</param>
public readonly record struct SpriteBatchClip(Vector2 Min, Vector2 Max)
{
    /// <summary>Creates a clip rectangle from scalar bounds.</summary>
    public SpriteBatchClip(float minX, float minY, float maxX, float maxY)
        : this(new Vector2(minX, minY), new Vector2(maxX, maxY))
    {
    }

    /// <summary>Gets the clip rectangle size in pixels.</summary>
    public Vector2 Size => Max - Min;
}
