namespace AlvorKit.Graphics2D;

/// <summary>Describes one drawable run that fits within the available texture slots.</summary>
internal struct SpriteBatchSection
{
    /// <summary>The first texture index in the batch-level texture list.</summary>
    internal int TextureStart;

    /// <summary>The number of texture slots used by this section.</summary>
    internal int TextureCount;

    /// <summary>The number of vertices in this section.</summary>
    internal int Count;
}
