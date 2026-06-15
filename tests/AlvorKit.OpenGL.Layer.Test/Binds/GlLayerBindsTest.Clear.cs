namespace AlvorKit.OpenGL.Layer.Test;

public partial class GlLayerBindsTest
{
    [TestMethod]
    public void DrawBuffer_ThenReset_Succeeds()
    {
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        gl.ResetDrawBuffers();
    }

    [TestMethod]
    public void DrawBuffers_ThenReset_Succeeds()
    {
        gl.DrawBuffers([GlDrawBufferMode.ColorAttachment0, GlDrawBufferMode.ColorAttachment1]);
        gl.ResetDrawBuffers();
    }

    [TestMethod]
    public void DrawBuffer_SetTwiceWithoutReset_Throws()
    {
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        Assert.Throws<GlAlreadySetException>(() => gl.DrawBuffer(GlDrawBufferMode.ColorAttachment1));
    }

    [TestMethod]
    public void ResetDrawBuffers_WhenNotSet_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetDrawBuffers());
    }

    [TestMethod]
    public void Clear_ColorWithoutClearColor_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() => gl.Clear(GlClearBufferMask.ColorBufferBit));
    }

    [TestMethod]
    public void Clear_DepthWithoutClearDepth_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() => gl.Clear(GlClearBufferMask.DepthBufferBit));
    }

    [TestMethod]
    public void Clear_ColorWithFramebufferWithoutDrawBuffers_Throws()
    {
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1));
        gl.ClearColor(0, 0, 0, 1);

        Assert.Throws<GlMissingPrerequisiteException>(() => gl.Clear(GlClearBufferMask.ColorBufferBit));
    }

    [TestMethod]
    public void Clear_WithScissorTestWithoutScissor_Throws()
    {
        gl.ClearColor(0, 0, 0, 1);
        gl.Enable(GlEnableCap.ScissorTest);

        Assert.Throws<GlMissingPrerequisiteException>(() => gl.Clear(GlClearBufferMask.ColorBufferBit));
    }

    [TestMethod]
    public void Clear_WithRequiredState_Succeeds()
    {
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, Framebuffer(1));
        gl.ClearColor(0, 0, 0, 1);
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);

        gl.Clear(GlClearBufferMask.ColorBufferBit);
    }
}
