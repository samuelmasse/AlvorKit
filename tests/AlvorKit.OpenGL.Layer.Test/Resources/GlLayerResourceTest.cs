namespace AlvorKit.OpenGL.Layer.Test;

[TestClass]
public class GlLayerResourceTest
{
    /// <summary>Disposing the layer drains every tracked object through the wrapped backend.</summary>
    [TestMethod]
    public void Dispose_DeletesEveryAllocatedObject()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var buffer = gl.GenBuffer();
        var texture = gl.GenTexture();
        var vao = gl.GenVertexArray();
        var framebuffer = gl.GenFramebuffer();
        var renderbuffer = gl.GenRenderbuffer();
        var sampler = gl.GenSampler();
        var query = gl.GenQuery();
        var pipeline = gl.GenProgramPipeline();
        var feedback = gl.GenTransformFeedback();
        var shader = gl.CreateShader(GlShaderType.VertexShader);
        var program = gl.CreateProgram();
        var sync = gl.FenceSync(GlSyncCondition.SyncGpuCommandsComplete, 0);

        gl.Dispose();

        CollectionAssert.AreEquivalent(
            new[]
            {
                (uint)buffer,
                (uint)texture,
                (uint)vao,
                (uint)framebuffer,
                (uint)renderbuffer,
                (uint)sampler,
                (uint)query,
                (uint)pipeline,
                (uint)feedback,
                (uint)shader,
                (uint)program,
                (uint)sync,
            },
            inner.Deleted);
    }

    /// <summary>Disposing after an explicit delete does not delete that object a second time.</summary>
    [TestMethod]
    public void Dispose_AfterManualDelete_DoesNotDeleteAgain()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var buffer = gl.GenBuffer();
        gl.DeleteBuffer(buffer);
        gl.Dispose();

        Assert.AreEqual(1, inner.Deleted.Count(id => id == (uint)buffer));
    }

    /// <summary>A generated buffer appears in and leaves the live resource set around deletion.</summary>
    [TestMethod]
    public void GenAndDelete_ReflectedInLiveSet()
    {
        var gl = new GlLayer(new RecordingGl());

        var buffer = gl.GenBuffer();
        Assert.IsTrue(gl.Buffers.Contains(buffer));

        gl.DeleteBuffer(buffer);
        Assert.IsFalse(gl.Buffers.Contains(buffer));
    }

    /// <summary>Deleting a buffer that the layer did not generate reports the missing tracked resource.</summary>
    [TestMethod]
    public void DeleteUntrackedBuffer_Throws()
    {
        var gl = new GlLayer(new RecordingGl());
        Assert.Throws<GlResourceNotTrackedException<GlBufferHandle>>(() => gl.DeleteBuffer((GlBufferHandle)123u));
    }

    /// <summary>Deleting a texture that the layer did not generate reports the missing tracked resource.</summary>
    [TestMethod]
    public void DeleteUntrackedTexture_Throws()
    {
        var gl = new GlLayer(new RecordingGl());
        Assert.Throws<GlResourceNotTrackedException<GlTextureHandle>>(() => gl.DeleteTexture((GlTextureHandle)123u));
    }

    /// <summary>Deleting a program that the layer did not create reports the missing tracked resource.</summary>
    [TestMethod]
    public void DeleteUntrackedProgram_Throws()
    {
        var gl = new GlLayer(new RecordingGl());
        Assert.Throws<GlResourceNotTrackedException<GlProgramHandle>>(() => gl.DeleteProgram((GlProgramHandle)123u));
    }

    /// <summary>Disposing an empty layer has no backend delete calls to forward.</summary>
    [TestMethod]
    public void Dispose_WhenNothingAllocated_DeletesNothing()
    {
        var inner = new RecordingGl();
        new GlLayer(inner).Dispose();
        Assert.AreEqual(0, inner.Deleted.Count);
    }

    /// <summary>Buffers must be unbound from generic, indexed, and vertex binding slots before deletion.</summary>
    [TestMethod]
    public void DeleteBuffer_WhenBound_ThrowsUntilUnbound()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var generic = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, generic);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteBuffer(generic),
            () => gl.UnbindBuffer(GlBufferTarget.ArrayBuffer),
            () => gl.Buffers.Contains(generic),
            inner,
            (uint)generic);

        var indexed = gl.GenBuffer();
        gl.BindBufferBase(GlBufferTarget.UniformBuffer, 0, indexed);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteBuffer(indexed),
            () =>
            {
                gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
                gl.UnbindBufferBase(GlBufferTarget.UniformBuffer, 0);
                gl.UnbindBuffer(GlBufferTarget.UniformBuffer);
            },
            () => gl.Buffers.Contains(indexed),
            inner,
            (uint)indexed);

        var vertex = gl.GenBuffer();
        gl.BindVertexBuffer(0, vertex, 0, 16);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteBuffer(vertex),
            () => gl.UnbindVertexBuffer(0),
            () => gl.Buffers.Contains(vertex),
            inner,
            (uint)vertex);
    }

    /// <summary>Textures must be unbound from texture binding slots before deletion.</summary>
    [TestMethod]
    public void DeleteTexture_WhenTextureBound_ThrowsUntilUnbound()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);
        var texture = gl.GenTexture();

        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);

        AssertDeleteRequiresUnbound(
            () => gl.DeleteTexture(texture),
            () =>
            {
                gl.UnbindTexture(GlTextureTarget.Texture2D);
                gl.ResetActiveTexture();
            },
            () => gl.Textures.Contains(texture),
            inner,
            (uint)texture);
    }

    /// <summary>Textures must be unbound from image units before deletion.</summary>
    [TestMethod]
    public void DeleteTexture_WhenImageBound_ThrowsUntilUnbound()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);
        var texture = gl.GenTexture();

        gl.BindImageTexture(0, texture, 0, false, 0, GlBufferAccess.ReadOnly, GlInternalFormat.Rgba8);

        AssertDeleteRequiresUnbound(
            () => gl.DeleteTexture(texture),
            () => gl.UnbindImageTexture(0),
            () => gl.Textures.Contains(texture),
            inner,
            (uint)texture);
    }

    /// <summary>Render target resources must be unbound from their current slots before deletion.</summary>
    [TestMethod]
    public void DeleteRenderTargetResources_WhenBound_ThrowUntilUnbound()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var vertexArray = gl.GenVertexArray();
        gl.BindVertexArray(vertexArray);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteVertexArray(vertexArray),
            gl.UnbindVertexArray,
            () => gl.VertexArrays.Contains(vertexArray),
            inner,
            (uint)vertexArray);

        var framebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(GlFramebufferTarget.Framebuffer, framebuffer);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteFramebuffer(framebuffer),
            () => gl.UnbindFramebuffer(GlFramebufferTarget.Framebuffer),
            () => gl.Framebuffers.Contains(framebuffer),
            inner,
            (uint)framebuffer);

        var drawFramebuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(GlFramebufferTarget.DrawFramebuffer, drawFramebuffer);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteFramebuffer(drawFramebuffer),
            () => gl.UnbindFramebuffer(GlFramebufferTarget.DrawFramebuffer),
            () => gl.Framebuffers.Contains(drawFramebuffer),
            inner,
            (uint)drawFramebuffer);

        var renderbuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(GlRenderbufferTarget.Renderbuffer, renderbuffer);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteRenderbuffer(renderbuffer),
            () => gl.UnbindRenderbuffer(GlRenderbufferTarget.Renderbuffer),
            () => gl.Renderbuffers.Contains(renderbuffer),
            inner,
            (uint)renderbuffer);
    }

    /// <summary>Samplers must be unbound from texture units before deletion.</summary>
    [TestMethod]
    public void DeleteSampler_WhenBound_ThrowsUntilUnbound()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);
        var sampler = gl.GenSampler();

        gl.BindSampler(0, sampler);

        AssertDeleteRequiresUnbound(
            () => gl.DeleteSampler(sampler),
            () => gl.UnbindSampler(0),
            () => gl.Samplers.Contains(sampler),
            inner,
            (uint)sampler);
    }

    /// <summary>Queries must be inactive across normal, indexed, and conditional query scopes before deletion.</summary>
    [TestMethod]
    public void DeleteQuery_WhenActive_ThrowsUntilInactive()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var query = gl.GenQuery();
        gl.BeginQuery(GlQueryTarget.SamplesPassed, query);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteQuery(query),
            () => gl.EndQuery(GlQueryTarget.SamplesPassed),
            () => gl.Queries.Contains(query),
            inner,
            (uint)query);

        var indexed = gl.GenQuery();
        gl.BeginQueryIndexed(GlQueryTarget.SamplesPassed, 0, indexed);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteQuery(indexed),
            () => gl.EndQueryIndexed(GlQueryTarget.SamplesPassed, 0),
            () => gl.Queries.Contains(indexed),
            inner,
            (uint)indexed);

        var conditional = gl.GenQuery();
        gl.BeginConditionalRender((uint)conditional, GlConditionalRenderMode.QueryWait);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteQuery(conditional),
            gl.EndConditionalRender,
            () => gl.Queries.Contains(conditional),
            inner,
            (uint)conditional);
    }

    /// <summary>Program-like resources must be unused or unbound before deletion.</summary>
    [TestMethod]
    public void DeleteProgramResources_WhenBound_ThrowUntilUnbound()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var pipeline = gl.GenProgramPipeline();
        gl.BindProgramPipeline(pipeline);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteProgramPipeline(pipeline),
            gl.UnbindProgramPipeline,
            () => gl.ProgramPipelines.Contains(pipeline),
            inner,
            (uint)pipeline);

        var feedback = gl.GenTransformFeedback();
        gl.BindTransformFeedback(GlBindTransformFeedbackTarget.TransformFeedback, feedback);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteTransformFeedback(feedback),
            () => gl.UnbindTransformFeedback(GlBindTransformFeedbackTarget.TransformFeedback),
            () => gl.TransformFeedbacks.Contains(feedback),
            inner,
            (uint)feedback);

        var program = gl.CreateProgram();
        gl.UseProgram(program);
        AssertDeleteRequiresUnbound(
            () => gl.DeleteProgram(program),
            gl.UnuseProgram,
            () => gl.Programs.Contains(program),
            inner,
            (uint)program);
    }

    /// <summary>Verifies failed bound deletes preserve tracking and do not reach the backend.</summary>
    /// <param name="delete">The delete action expected to fail while the resource is bound.</param>
    /// <param name="unbind">The action that releases the resource from every live binding slot.</param>
    /// <param name="isTracked">A predicate that reports whether the layer still tracks the resource.</param>
    /// <param name="inner">The backend that records forwarded deletes.</param>
    /// <param name="id">The raw resource id requested for deletion.</param>
    private static void AssertDeleteRequiresUnbound(
        Action delete,
        Action unbind,
        Func<bool> isTracked,
        RecordingGl inner,
        uint id)
    {
        Assert.Throws<GlBindConflictException>(delete);
        Assert.IsTrue(isTracked());
        Assert.IsFalse(inner.Deleted.Contains(id));

        unbind();
        delete();

        Assert.IsFalse(isTracked());
        Assert.IsTrue(inner.Deleted.Contains(id));
    }
}
