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

    /// <summary>Creates a complete font context around a fake OpenGL and FreeType driver pair.</summary>
    internal static (FontsTestGl Backend, FontsTestDriver Driver, SpriteBatch Batch, FontContext Context) CreateContext()
    {
        var (backend, gl) = CreateLayer();
        var driver = new FontsTestDriver();
        var batch = new SpriteBatch(gl);
        var context = new FontContext(gl, driver, batch);
        return (backend, driver, batch, context);
    }
}
