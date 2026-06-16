namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Exercises strict bind APIs that are structurally similar to the primary bind tests.
/// </summary>
[TestClass]
public unsafe class GlLayerBindsCoverageTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());

    [TestMethod]
    public void BufferRangeAndVertexRange_BindThenUnbind()
    {
        gl.BindBufferRange(GlBufferTarget.UniformBuffer, 0, Buffer(1), 0, 16);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBufferRange(GlBufferTarget.UniformBuffer, 0);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);

        uint* buffers = stackalloc uint[] { 2, 3 };
        nint* offsets = stackalloc nint[] { 0, 16 };
        nint* sizes = stackalloc nint[] { 16, 16 };
        gl.BindBuffersRange(GlBufferTarget.UniformBuffer, 0, 2, (nint)buffers, (nint)offsets, (nint)sizes);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBuffersRange(GlBufferTarget.UniformBuffer, 0, 2);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);

        int* strides = stackalloc int[] { 8, 8 };
        gl.BindVertexBuffers(0, 2, (nint)buffers, (nint)offsets, (nint)strides);
        gl.UnbindVertexBuffers(0, 2);
    }

    /// <summary>Span bulk-buffer bind overloads use the same strict bind tracking as raw pointer calls.</summary>
    [TestMethod]
    public void SpanBufferRangeAndVertexRange_BindThenUnbind()
    {
        gl.BindBuffersRange(
            GlBufferTarget.UniformBuffer,
            0,
            [Buffer(1), Buffer(2)],
            [0, 16],
            [16, 16]);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
        gl.UnbindBuffersRange(GlBufferTarget.UniformBuffer, 0, 2);
        gl.UnbindBuffer(GlBufferTarget.UniformBuffer);

        gl.BindVertexBuffers(0, [Buffer(3), Buffer(4)], [0, 16], [8, 8]);
        gl.UnbindVertexBuffers(0, 2);
    }

    [TestMethod]
    public void PipelineQueriesAndTransformFeedback_BindThenUnbind()
    {
        gl.BindProgramPipeline((GlProgramPipelineHandle)1u);
        gl.UnbindProgramPipeline();

        gl.BeginQueryIndexed(GlQueryTarget.SamplesPassed, 0, (GlQueryHandle)2u);
        gl.EndQueryIndexed(GlQueryTarget.SamplesPassed, 0);

        gl.BindTransformFeedback(GlBindTransformFeedbackTarget.TransformFeedback, (GlTransformFeedbackHandle)3u);
        gl.UnbindTransformFeedback(GlBindTransformFeedbackTarget.TransformFeedback);
    }

    [TestMethod]
    public void RenderbufferTextureUnitAndImages_BindThenUnbind()
    {
        gl.BindRenderbuffer(GlRenderbufferTarget.Renderbuffer, (GlRenderbufferHandle)1u);
        gl.UnbindRenderbuffer(GlRenderbufferTarget.Renderbuffer);

        Assert.Throws<GlException>(() => gl.BindTextureUnit(0, (GlTextureHandle)0u));
        TrackTextureTarget((GlTextureHandle)2u, GlTextureTarget.Texture2D);
        gl.BindTextureUnit(0, (GlTextureHandle)2u);
        gl.UnbindTextureUnit(0);

        gl.BindImageTexture(1, (GlTextureHandle)2u, 0, false, 0, GlBufferAccess.ReadOnly, GlInternalFormat.Rgba8);
        gl.UnbindImageTexture(1);

        uint* textures = stackalloc uint[] { 2, 3 };
        TrackTextureTarget((GlTextureHandle)3u, GlTextureTarget.Texture2D);
        gl.BindImageTextures(2, 2, (nint)textures);
        gl.UnbindImageTextures(2, 2);
    }

    /// <summary>Span image-texture bind overloads use the same strict bind tracking as raw pointer calls.</summary>
    [TestMethod]
    public void SpanImageTextures_BindThenUnbind()
    {
        TrackTextureTarget((GlTextureHandle)4u, GlTextureTarget.Texture2D);

        gl.BindImageTextures(0, [(GlTextureHandle)4u]);
        gl.UnbindImageTextures(0, 1);
    }

    private static GlBufferHandle Buffer(uint id) => (GlBufferHandle)id;

    private void TrackTextureTarget(GlTextureHandle texture, GlTextureTarget target)
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(target, texture);
        gl.UnbindTexture(target);
        gl.ResetActiveTexture();
    }
}
