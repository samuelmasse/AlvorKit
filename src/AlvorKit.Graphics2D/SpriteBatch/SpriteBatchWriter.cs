namespace AlvorKit.Graphics2D;

/// <summary>Accepts high-level sprite and line draw calls for a running <see cref="SpriteBatch"/>.</summary>
public partial class SpriteBatchWriter
{
    /// <summary>The default white texture used for shape-only draws.</summary>
    private readonly Texture texture;

    /// <summary>The current canvas dimensions used for coordinate normalization.</summary>
    private readonly SpriteBatchCanvas canvas;

    /// <summary>The pending vertex collector that receives generated quad vertices.</summary>
    private readonly SpriteBatchVertices vertices;

    /// <summary>The optional clipping rectangle applied to subsequent draw calls.</summary>
    private SpriteBatchClip? clip;

    /// <summary>Gets or sets the optional clipping rectangle applied to subsequent draw calls.</summary>
    public SpriteBatchClip? Clip { get => clip; set => clip = value; }

    /// <summary>Creates a writer over shared sprite-batch state.</summary>
    internal SpriteBatchWriter(Texture texture, SpriteBatchCanvas canvas, SpriteBatchVertices vertices)
    {
        this.texture = texture;
        this.canvas = canvas;
        this.vertices = vertices;
    }

    /// <summary>Draws a solid quad with the default white texture.</summary>
    public void Draw(Vector2 position, Vector2 size) =>
        Draw(texture, position, size, Vector2.Zero, texture.Size, Vector4.One, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a tinted quad with the default white texture.</summary>
    public void Draw(Vector2 position, Vector2 size, Vector4 color) =>
        Draw(texture, position, size, Vector2.Zero, texture.Size, color, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a texture at its natural size.</summary>
    public void Draw(Texture texture, Vector2 position) =>
        Draw(texture, position, texture.Size, Vector2.Zero, texture.Size, Vector4.One, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a texture stretched to the supplied size.</summary>
    public void Draw(Texture texture, Vector2 position, Vector2 size) =>
        Draw(texture, position, size, Vector2.Zero, texture.Size, Vector4.One, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a tinted texture stretched to the supplied size.</summary>
    public void Draw(Texture texture, Vector2 position, Vector2 size, Vector4 color) =>
        Draw(texture, position, size, Vector2.Zero, texture.Size, color, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a subregion of a texture stretched to the supplied size.</summary>
    public void Draw(Texture texture, Vector2 position, Vector2 size, Vector2 subPosition, Vector2 subSize) =>
        Draw(texture, position, size, subPosition, subSize, Vector4.One, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a tinted subregion of a texture stretched to the supplied size.</summary>
    public void Draw(Texture texture, Vector2 position, Vector2 size, Vector2 subPosition, Vector2 subSize, Vector4 color) =>
        Draw(texture, position, size, subPosition, subSize, color, SpriteBatchRotation.None, SpriteBatchFlip.None);

    /// <summary>Draws a tinted, rotated, and flipped texture subregion.</summary>
    public void Draw(
        Texture texture,
        Vector2 position,
        Vector2 size,
        Vector2 subPosition,
        Vector2 subSize,
        Vector4 color,
        SpriteBatchRotation rotation,
        SpriteBatchFlip flip) =>
        DrawVertices(texture, position, size, color, subPosition, subSize, rotation, flip);

    /// <summary>Draws a one-pixel white line.</summary>
    public void DrawLine(Vector2 start, Vector2 end) => DrawLine(texture, start, end, 1f, Vector4.One);

    /// <summary>Draws a tinted line with the default white texture.</summary>
    public void DrawLine(Vector2 start, Vector2 end, float width, Vector4 color) => DrawLine(texture, start, end, width, color);

    /// <summary>Draws a one-pixel textured line.</summary>
    public void DrawLine(Texture texture, Vector2 start, Vector2 end) => DrawLine(texture, start, end, 1f, Vector4.One);

    /// <summary>Draws a tinted textured line with the supplied width.</summary>
    public void DrawLine(Texture texture, Vector2 start, Vector2 end, float width, Vector4 color)
    {
        var delta = end - start;
        var normStart = NormalizePosition(start);
        var normEnd = NormalizePosition(end);
        var norm = Vector2.Normalize(new Vector2(-delta.Y, -delta.X));
        var size = NormalizeSize(new Vector2(width, width));

        DrawQuad(normEnd - (norm * size), normEnd + (norm * size), normStart - (norm * size), normStart + (norm * size), texture, color, Vector2.Zero, texture.Size);
    }
}
