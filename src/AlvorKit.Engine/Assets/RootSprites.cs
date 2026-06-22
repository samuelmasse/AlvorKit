namespace AlvorKit.Engine;

/// <summary>Root-owned sprite batch used by two-dimensional engine drawing.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootSprites(SpriteBatch spriteBatch) : IDisposable
{
    /// <summary>Gets the writer used to append sprite and primitive draw commands.</summary>
    public SpriteBatchWriter Batch => spriteBatch.Writer;

    /// <summary>Releases sprite-batch GL resources.</summary>
    public void Dispose() => spriteBatch.Dispose();

    /// <summary>Starts collecting draw commands for a canvas or draw area.</summary>
    internal void Begin(Vec2 size) => spriteBatch.Begin(size);

    /// <summary>Flushes collected draw commands.</summary>
    internal void End() => spriteBatch.End();
}
