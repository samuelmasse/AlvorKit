namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void BindFramebuffer_OverLive_Throws()
    {
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1));
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(2)));
    }

    [TestMethod]
    public void BindFramebuffer_UnbindThenRebind_Succeeds()
    {
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1));
        gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer);
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(2));
    }

    [TestMethod]
    public void BindFramebuffer_WhenDrawBuffersSet_Throws()
    {
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        Assert.Throws<GlBindConflictException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1)));
    }

    [TestMethod]
    public void BindFramebuffer_ReadFramebufferWhenDrawBuffersSet_Succeeds()
    {
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        gl.BindFramebuffer(GlFramebufferTarget.ReadFramebuffer, Framebuffer(1));
    }

    [TestMethod]
    public void BindFramebuffer_WhenViewportSet_Throws()
    {
        gl.Viewport(0, 0, 640, 480);
        Assert.Throws<GlBindConflictException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1)));
    }

    [TestMethod]
    public void BindFramebuffer_WhenReadBufferSet_Throws()
    {
        gl.ReadBuffer(GlReadBufferMode.ColorAttachment0);
        Assert.Throws<GlBindConflictException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1)));
    }

    [TestMethod]
    public void UnbindFramebuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer));
    }
}
