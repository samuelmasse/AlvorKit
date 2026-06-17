namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Shares FreeType, OpenGL atlas staging, and sprite batching resources between fonts.</summary>
public sealed class FontContext : IDisposable
{
    /// <summary>The strict OpenGL layer used by font atlas resources.</summary>
    private readonly GlLayer gl;

    /// <summary>The FreeType binding used by opened fonts.</summary>
    private readonly Ft ft;

    /// <summary>The sprite batch used when repacking atlas textures.</summary>
    private readonly SpriteBatch batch;

    /// <summary>The FreeType library lifetime owner.</summary>
    private readonly FontLibrary library;

    /// <summary>The framebuffer and scratch texture used while repacking atlases.</summary>
    private readonly FontBuffer buffer;

    /// <summary>Creates a context using the generated AlvorKit FreeType backend.</summary>
    public FontContext(GlLayer gl, SpriteBatch batch) : this(gl, new FtBackend(), batch) { }

    /// <summary>Creates a context with a caller-supplied FreeType binding for tests.</summary>
    internal FontContext(GlLayer gl, Ft ft, SpriteBatch batch)
    {
        this.gl = gl;
        this.ft = ft;
        this.batch = batch;
        library = new FontLibrary(ft);
        buffer = new FontBuffer(gl);
    }

    /// <summary>Gets the strict OpenGL layer used by font atlas resources.</summary>
    internal GlLayer GL => gl;

    /// <summary>Gets the FreeType binding used by opened fonts.</summary>
    internal Ft FreeType => ft;

    /// <summary>Gets the sprite batch used when repacking atlas textures.</summary>
    internal SpriteBatch Batch => batch;

    /// <summary>Gets the FreeType library lifetime owner.</summary>
    internal FontLibrary Library => library;

    /// <summary>Gets the framebuffer and scratch texture used while repacking atlases.</summary>
    internal FontBuffer Buffer => buffer;

    /// <summary>Releases shared FreeType and OpenGL staging resources.</summary>
    public void Dispose()
    {
        library.Dispose();
        buffer.Dispose();
    }
}
