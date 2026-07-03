namespace AlvorKit.Windowing;

/// <summary>Exposes the current drawable surface for a window loop.</summary>
public class WindowCanvas(WindowLoop window)
{
    /// <summary>Gets the drawable client size.</summary>
    public Vec2u Size => window.Physical.Size;
}
