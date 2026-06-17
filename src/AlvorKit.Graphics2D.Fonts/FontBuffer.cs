namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns a framebuffer and scratch tablet used during atlas repacking.</summary>
internal sealed class FontBuffer : IDisposable
{
    /// <summary>The strict OpenGL layer that owns framebuffer resources.</summary>
    private readonly GlLayer gl;

    /// <summary>The framebuffer used as a render target while repacking.</summary>
    private readonly GlFramebufferHandle framebuffer;

    /// <summary>The scratch tablet that receives a repacked atlas.</summary>
    private FontTablet tablet;

    /// <summary>Creates framebuffer and scratch texture resources.</summary>
    internal FontBuffer(GlLayer gl)
    {
        this.gl = gl;
        framebuffer = gl.GenFramebuffer();
        tablet = new FontTablet(gl);
    }

    /// <summary>Gets the framebuffer used as a render target while repacking.</summary>
    internal GlFramebufferHandle Framebuffer => framebuffer;

    /// <summary>Gets the scratch tablet that receives a repacked atlas.</summary>
    internal ref FontTablet Tablet => ref tablet;

    /// <summary>Deletes framebuffer and scratch texture resources.</summary>
    public void Dispose()
    {
        gl.DeleteFramebuffer(framebuffer);
        tablet.Dispose();
    }
}
