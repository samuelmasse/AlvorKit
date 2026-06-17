namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Creates shared Graphics2D font test fixtures.</summary>
internal static class FontsTestHarness
{
    /// <summary>Creates a recording backend wrapped in a strict OpenGL layer.</summary>
    internal static (FontsTestGl Backend, GlLayer Layer) CreateLayer()
    {
        var backend = new FontsTestGl();
        return (backend, new GlLayer(backend));
    }

    /// <summary>Creates a complete font context around fake OpenGL and FreeType bindings.</summary>
    internal static (FontsTestGl Backend, FontsTestFt FreeType, SpriteBatch Batch, FontContext Context) CreateContext()
    {
        var (backend, gl) = CreateLayer();
        var driver = new FontsTestFt();
        var batch = new SpriteBatch(gl);
        var context = new FontContext(gl, driver, batch);
        return (backend, driver, batch, context);
    }
}
