namespace AlvorKit.Graphics2D;

/// <summary>Stores the current canvas size used to normalize sprite coordinates.</summary>
internal class SpriteBatchCanvas
{
    /// <summary>The current canvas size in pixels.</summary>
    private Vector2 size;

    /// <summary>Gets or sets the current canvas size in pixels.</summary>
    internal Vector2 Size { get => size; set => size = value; }
}
