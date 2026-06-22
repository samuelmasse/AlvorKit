namespace AlvorKit.Engine;

/// <summary>Root-scoped view of the current drawable canvas.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootCanvas(WindowLoop window)
{
    private readonly WindowCanvas canvas = new(window);

    /// <summary>Gets the current drawable client size.</summary>
    public Vec2u Size => canvas.Size;
}
