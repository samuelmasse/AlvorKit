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
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
    }

    [TestMethod]
    public void BindBuffer_OverLiveBinding_Throws()
    {
        gl.BindBuffer(BufferTarget.ArrayBuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(BufferTarget.ArrayBuffer, 2));
    }

    [TestMethod]
    public void BindZero_OverLiveBinding_Throws()
    {
        gl.BindBuffer(BufferTarget.ArrayBuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(BufferTarget.ArrayBuffer, 0));
    }

    [TestMethod]
    public void UnbindBuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindBuffer(BufferTarget.ArrayBuffer));
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
    public void BindVertexArray_ThenUnbind_Succeeds()
    {
        gl.BindVertexArray(1);
        gl.UnbindVertexArray();
    }

    [TestMethod]
    public void UseProgram_ThenUnuse_Succeeds()
    {
        gl.UseProgram(1);
        gl.UnuseProgram();
    }

    [TestMethod]
    public void UseProgram_OverLiveProgram_Throws()
    {
        gl.UseProgram(1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.UseProgram(2));
    }

    [TestMethod]
    public void UnuseProgram_WhenNothingUsed_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnuseProgram());
    }

    [TestMethod]
    public void BindFramebuffer_OverLive_Throws()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindFramebuffer(FramebufferTarget.Framebuffer, 2));
    }

    [TestMethod]
    public void BindFramebuffer_UnbindThenRebind_Succeeds()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
        gl.UnbindFramebuffer(FramebufferTarget.Framebuffer);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 2);
    }

    [TestMethod]
    public void UnbindFramebuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindFramebuffer(FramebufferTarget.Framebuffer));
    }

    [TestMethod]
    public void BindTexture_ThenUnbind_Succeeds()
    {
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, 1);
        gl.UnbindTexture(TextureTarget.Texture2D);
    }

    [TestMethod]
    public void BindTexture_WithoutActiveTexture_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() => gl.BindTexture(TextureTarget.Texture2D, 1));
    }

    [TestMethod]
    public void ActiveTexture_SetTwiceWithoutReset_Throws()
    {
        gl.ActiveTexture(TextureUnit.Texture0);
        Assert.Throws<GlAlreadySetException>(() => gl.ActiveTexture(TextureUnit.Texture1));
    }

    [TestMethod]
    public void ResetActiveTexture_WhenNotSet_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetActiveTexture());
    }

    [TestMethod]
    public void BindTexture_SameTextureDifferentTarget_Throws()
    {
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, 1);
        gl.UnbindTexture(TextureTarget.Texture2D);
        Assert.Throws<GlBindConflictException>(() => gl.BindTexture(TextureTarget.Texture3D, 1));
    }

    [TestMethod]
    public void BindTexture_SameTargetDifferentUnits_DoNotConflict()
    {
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, 1);
        gl.UnbindTexture(TextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.ActiveTexture(TextureUnit.Texture1);
        gl.BindTexture(TextureTarget.Texture2D, 2);
    }

    [TestMethod]
    public void BindSampler_ThenUnbind_Succeeds()
    {
        gl.BindSampler(0, 1);
        gl.UnbindSampler(0);
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

    [TestMethod]
    public void BindSamplers_ThenUnbind_Succeeds()
    {
        gl.BindSamplers(0, [1, 2, 3]);
        gl.UnbindSamplers(0, 3);
    }

    [TestMethod]
    public void BindSamplers_OverLiveSingularBind_Throws()
    {
        gl.BindSampler(1, 7);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindSamplers(0, [4, 5, 6]));
    }

    [TestMethod]
    public void UnbindSamplers_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindSamplers(0, 2));
    }

    [TestMethod]
    public void BindTextures_ThenUnbind_Succeeds()
    {
        gl.BindTextures(0, [1, 2]);
        gl.UnbindTextures(0, 2);
    }

    [TestMethod]
    public void BindBuffersBase_ThenUnbind_Succeeds()
    {
        gl.BindBuffersBase(BufferTarget.UniformBuffer, 0, [1, 2]);
        gl.UnbindBuffersBase(BufferTarget.UniformBuffer, 0, 2);
    }

    [TestMethod]
    public void BindBuffersBase_OverLiveSingularBind_Throws()
    {
        gl.BindBufferBase(BufferTarget.UniformBuffer, 0, 9);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffersBase(BufferTarget.UniformBuffer, 0, [1, 2]));
    }

    [TestMethod]
    public void DrawBuffer_ThenReset_Succeeds()
    {
        gl.DrawBuffer(DrawBufferMode.ColorAttachment0);
        gl.ResetDrawBuffers();
    }

    [TestMethod]
    public void DrawBuffers_ThenReset_Succeeds()
    {
        gl.DrawBuffers([DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1]);
        gl.ResetDrawBuffers();
    }

    [TestMethod]
    public void DrawBuffer_SetTwiceWithoutReset_Throws()
    {
        gl.DrawBuffer(DrawBufferMode.ColorAttachment0);
        Assert.Throws<GlAlreadySetException>(() => gl.DrawBuffer(DrawBufferMode.ColorAttachment1));
    }

    [TestMethod]
    public void ResetDrawBuffers_WhenNotSet_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetDrawBuffers());
    }
}
