namespace AlvorKit;

/// <summary>Owns a framebuffer and scratch tablet used during atlas repacking.</summary>
internal class FontBuffer(GlLayer gl) : IDisposable
{
    /// <summary>The framebuffer used as a render target while repacking.</summary>
    private readonly GlFramebufferHandle framebuffer = gl.GenFramebuffer();

    /// <summary>The scratch tablet that receives a repacked atlas.</summary>
    private FontTablet tablet = new(gl);

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
