namespace AlvorKit.Graphics2D;

/// <summary>One interleaved vertex consumed by the sprite batch shader.</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct SpriteBatchVertex(Vec2 position, Vec4 color, Vec2 texCoord, float texIndex = 0f)
{
    /// <summary>The byte size of one interleaved vertex.</summary>
    public static int Size => sizeof(float) * 9;

    /// <summary>The normalized clip-space position.</summary>
    internal Vec2 Position = position;

    /// <summary>The per-vertex tint color.</summary>
    internal Vec4 Color = color;

    /// <summary>The normalized texture coordinate.</summary>
    internal Vec2 TexCoord = texCoord;

    /// <summary>The texture-slot index encoded for the fragment shader.</summary>
    internal float TexIndex = texIndex;
}
