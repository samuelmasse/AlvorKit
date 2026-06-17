namespace AlvorKit.Graphics2D.Test;

/// <summary>Creates shared Graphics2D test fixtures.</summary>
internal static class Graphics2DTestHarness
{
    /// <summary>Creates a recording backend wrapped in a strict OpenGL layer.</summary>
    internal static (Graphics2DTestGl Backend, GlLayer Layer) CreateLayer()
    {
        var backend = new Graphics2DTestGl();
        return (backend, new GlLayer(backend));
    }
}
