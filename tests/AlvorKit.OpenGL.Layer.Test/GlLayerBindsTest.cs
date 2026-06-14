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
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, 1);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
    }

    [TestMethod]
    public void BindBuffer_OverLiveBinding_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(GlBufferTarget.ArrayBuffer, 2));
    }

    [TestMethod]
    public void BindZero_OverLiveBinding_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffer(GlBufferTarget.ArrayBuffer, 0));
    }

    [TestMethod]
    public void UnbindBuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindBuffer(GlBufferTarget.ArrayBuffer));
    }

    [TestMethod]
    public void BindBuffer_DifferentTargets_DoNotConflict()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, 1);
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, 2);
    }

    [TestMethod]
    public void BindVertexArray_WhenVboBound_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, 2);
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(1));
    }

    [TestMethod]
    public void BindVertexArray_WhenEboBound_Throws()
    {
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, 2);
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(1));
    }

    [TestMethod]
    public void BindVertexArray_WhenVertexBufferBound_Throws()
    {
        gl.BindVertexBuffer(0, 2, 0, 16);
        Assert.Throws<GlBindConflictException>(() => gl.BindVertexArray(1));
    }

    [TestMethod]
    public void BindVertexArray_ThenUnbind_Succeeds()
    {
        gl.BindVertexArray(1);
        gl.UnbindVertexArray();
    }

    [TestMethod]
    public void UnbindVertexArray_WhenArrayBufferBound_Throws()
    {
        gl.BindVertexArray(1);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, 2);
        Assert.Throws<GlBindConflictException>(() => gl.UnbindVertexArray());
    }

    [TestMethod]
    public void UnbindVertexArray_ClearsVaoOwnedBufferBindings()
    {
        gl.BindVertexArray(1);
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, 2);
        gl.BindVertexBuffer(0, 3, 0, 16);

        gl.UnbindVertexArray();

        Assert.Throws<GlNotBoundException>(() => gl.UnbindBuffer(GlBufferTarget.ElementArrayBuffer));
        Assert.Throws<GlNotBoundException>(() => gl.UnbindVertexBuffer(0));
    }

    [TestMethod]
    public void UnbindElementArrayBuffer_WhenVaoBound_Throws()
    {
        gl.BindVertexArray(1);
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, 2);

        Assert.Throws<GlBindConflictException>(() => gl.UnbindBuffer(GlBufferTarget.ElementArrayBuffer));
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
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 2));
    }

    [TestMethod]
    public void BindFramebuffer_UnbindThenRebind_Succeeds()
    {
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1);
        gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer);
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 2);
    }

    [TestMethod]
    public void BindFramebuffer_WhenDrawBuffersSet_Throws()
    {
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        Assert.Throws<GlBindConflictException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1));
    }

    [TestMethod]
    public void BindFramebuffer_ReadFramebufferWhenDrawBuffersSet_Succeeds()
    {
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);
        gl.BindFramebuffer(GlFramebufferTarget.ReadFramebuffer, 1);
    }

    [TestMethod]
    public void BindFramebuffer_WhenViewportSet_Throws()
    {
        gl.Viewport(0, 0, 640, 480);
        Assert.Throws<GlBindConflictException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1));
    }

    [TestMethod]
    public void BindFramebuffer_WhenReadBufferSet_Throws()
    {
        gl.ReadBuffer(GlReadBufferMode.ColorAttachment0);
        Assert.Throws<GlBindConflictException>(() => gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1));
    }

    [TestMethod]
    public void UnbindFramebuffer_WhenNothingBound_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer));
    }

    [TestMethod]
    public void BindTexture_ThenUnbind_Succeeds()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, 1);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
    }

    [TestMethod]
    public void BindTexture_WithoutActiveTexture_Throws()
    {
        Assert.Throws<GlMissingPrerequisiteException>(() => gl.BindTexture(GlTextureTarget.Texture2D, 1));
    }

    [TestMethod]
    public void ActiveTexture_SetTwiceWithoutReset_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        Assert.Throws<GlAlreadySetException>(() => gl.ActiveTexture(GlTextureUnit.Texture1));
    }

    [TestMethod]
    public void ResetActiveTexture_WhenNotSet_Throws()
    {
        Assert.Throws<GlAlreadyUnsetException>(() => gl.ResetActiveTexture());
    }

    [TestMethod]
    public void BindTexture_SameTextureDifferentTarget_Throws()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, 1);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        Assert.Throws<GlBindConflictException>(() => gl.BindTexture(GlTextureTarget.Texture3D, 1));
    }

    [TestMethod]
    public void BindTexture_SameTargetDifferentUnits_DoNotConflict()
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, 1);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.ActiveTexture(GlTextureUnit.Texture1);
        gl.BindTexture(GlTextureTarget.Texture2D, 2);
    }

    [TestMethod]
    public void BindTextureUnit_WithoutKnownTarget_Throws()
    {
        Assert.Throws<GlException>(() => gl.BindTextureUnit(0, 42));
    }

    [TestMethod]
    public void BindTexture_ThenBindTextureUnitSameUnit_Throws()
    {
        TrackTextureTarget(42, GlTextureTarget.Texture2D);
        TrackTextureTarget(43, GlTextureTarget.Texture2D);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, 42);

        Assert.Throws<GlAlreadyBoundException>(() => gl.BindTextureUnit(0, 43));
    }

    [TestMethod]
    public void BindTextureUnit_ThenBindTextureSameUnit_Throws()
    {
        TrackTextureTarget(42, GlTextureTarget.Texture2D);
        gl.BindTextureUnit(0, 42);
        gl.ActiveTexture(GlTextureUnit.Texture0);

        Assert.Throws<GlAlreadyBoundException>(() => gl.BindTexture(GlTextureTarget.Texture2D, 43));
    }

    [TestMethod]
    public void BindTexture_ThenBindTextureUnitSameUnitDifferentTarget_Succeeds()
    {
        TrackTextureTarget(42, GlTextureTarget.Texture2D);
        TrackTextureTarget(43, GlTextureTarget.TextureCubeMap);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, 42);

        gl.BindTextureUnit(0, 43);
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
        gl.BeginQuery(GlQueryTarget.SamplesPassed, 1);
        gl.EndQuery(GlQueryTarget.SamplesPassed);
    }

    [TestMethod]
    public void Query_BeginWhileActive_Throws()
    {
        gl.BeginQuery(GlQueryTarget.SamplesPassed, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BeginQuery(GlQueryTarget.SamplesPassed, 2));
    }

    [TestMethod]
    public void Query_EndWithoutBegin_Throws()
    {
        Assert.Throws<GlNotBoundException>(() => gl.EndQuery(GlQueryTarget.SamplesPassed));
    }

    [TestMethod]
    public void ConditionalRender_BeginThenEnd_Succeeds()
    {
        gl.BeginConditionalRender(1, GlConditionalRenderMode.QueryWait);
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
        TrackTextureTarget(1, GlTextureTarget.Texture2D);
        TrackTextureTarget(2, GlTextureTarget.Texture2D);
        gl.BindTextures(0, [1, 2]);
        gl.UnbindTextures(0, 2);
    }

    [TestMethod]
    public void BindTextureUnit_ThenBindTexturesSameUnit_Throws()
    {
        TrackTextureTarget(1, GlTextureTarget.Texture2D);
        TrackTextureTarget(2, GlTextureTarget.Texture2D);
        gl.BindTextureUnit(0, 1);

        Assert.Throws<GlAlreadyBoundException>(() => gl.BindTextures(0, [2]));
    }

    [TestMethod]
    public void BindBuffersBase_ThenUnbind_Succeeds()
    {
        gl.BindBuffersBase(GlBufferTarget.UniformBuffer, 0, [1, 2]);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBuffersBase(GlBufferTarget.UniformBuffer, 0, 2);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
    }

    [TestMethod]
    public void BindBufferBase_TwoIndicesWithoutGenericUnbind_Throws()
    {
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, 1);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBufferBase(GlBufferTarget.UniformBuffer, 1, 2));
    }

    [TestMethod]
    public void BindBufferBase_TwoIndicesWithGenericUnbind_Succeeds()
    {
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, 1);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 1, 2);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBufferBase(GlBufferTarget.UniformBuffer, 0);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBufferBase(GlBufferTarget.UniformBuffer, 1);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
    }

    [TestMethod]
    public void BindBuffersBase_OverLiveSingularBind_Throws()
    {
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, 9);
        Assert.Throws<GlAlreadyBoundException>(() => gl.BindBuffersBase(GlBufferTarget.UniformBuffer, 0, [1, 2]));
    }

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
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1);
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
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, 1);
        gl.ClearColor(0, 0, 0, 1);
        gl.DrawBuffer(GlDrawBufferMode.ColorAttachment0);

        gl.Clear(GlClearBufferMask.ColorBufferBit);
    }

    private void TrackTextureTarget(uint texture, GlTextureTarget target)
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(target, texture);
        gl.UnbindTexture(target);
        gl.ResetActiveTexture();
    }
}
