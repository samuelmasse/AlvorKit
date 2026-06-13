namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlLayerBindsTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());

    [TestMethod]
    public void BindBuffer_ThenUnbind_Succeeds()
    {
        gl.BindBuffer(BufferTarget.ArrayBuffer, 1);
        gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    [TestMethod]
    public void BindBuffer_OverLiveBinding_Throws()
    {
        gl.BindBuffer(BufferTarget.ArrayBuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(BufferTarget.ArrayBuffer, 2));
    }

    [TestMethod]
    public void UnbindBuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.BindBuffer(BufferTarget.ArrayBuffer, 0));
    }

    [TestMethod]
    public void BindBuffer_DifferentTargets_DoNotConflict()
    {
        gl.BindBuffer(BufferTarget.ArrayBuffer, 1);
        gl.BindBuffer(BufferTarget.ElementArrayBuffer, 2);
    }

    [TestMethod]
    public void BindVertexArray_WhenVboBound_Throws()
    {
        gl.BindBuffer(BufferTarget.ArrayBuffer, 2);
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(1));
    }

    [TestMethod]
    public void BindVertexArray_WhenNoBuffersBound_Succeeds()
    {
        gl.BindVertexArray(1);
        gl.BindVertexArray(0);
    }

    [TestMethod]
    public void UseProgram_ThenUnuse_Succeeds()
    {
        gl.UseProgram(1);
        gl.UseProgram(0);
    }

    [TestMethod]
    public void UseProgram_OverLiveProgram_Throws()
    {
        gl.UseProgram(1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.UseProgram(2));
    }

    [TestMethod]
    public void BindFramebuffer_DefaultRepeatedly_Allowed()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    [TestMethod]
    public void BindFramebuffer_SwitchWithoutReturningToDefault_Throws()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindFramebuffer(FramebufferTarget.Framebuffer, 2));
    }

    [TestMethod]
    public void BindFramebuffer_SwitchViaDefault_Succeeds()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 2);
    }

    [TestMethod]
    public void BindTexture_DefaultActiveUnit_Succeeds()
    {
        gl.BindTexture(TextureTarget.Texture2D, 1);
        gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    [TestMethod]
    public void BindTexture_SameTextureDifferentTarget_Throws()
    {
        gl.BindTexture(TextureTarget.Texture2D, 1);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        Assert.Throws<GlBindConflictException>(() => gl.BindTexture(TextureTarget.Texture3D, 1));
    }

    [TestMethod]
    public void BindTexture_SameTargetDifferentUnits_DoNotConflict()
    {
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, 1);
        gl.ActiveTexture(TextureUnit.Texture1);
        gl.BindTexture(TextureTarget.Texture2D, 2);
    }

    [TestMethod]
    public void BindSampler_ThenUnbind_Succeeds()
    {
        gl.BindSampler(0, 1);
        gl.BindSampler(0, 0);
    }

    [TestMethod]
    public void Query_BeginThenEnd_Succeeds()
    {
        gl.BeginQuery(QueryTarget.SamplesPassed, 1);
        gl.EndQuery(QueryTarget.SamplesPassed);
    }

    [TestMethod]
    public void Query_BeginWhileActive_Throws()
    {
        gl.BeginQuery(QueryTarget.SamplesPassed, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BeginQuery(QueryTarget.SamplesPassed, 2));
    }

    [TestMethod]
    public void Query_EndWithoutBegin_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.EndQuery(QueryTarget.SamplesPassed));
    }

    [TestMethod]
    public void ConditionalRender_BeginThenEnd_Succeeds()
    {
        gl.BeginConditionalRender(1, ConditionalRenderMode.QueryWait);
        gl.EndConditionalRender();
    }

    [TestMethod]
    public void ConditionalRender_EndWithoutBegin_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.EndConditionalRender());
    }
}
