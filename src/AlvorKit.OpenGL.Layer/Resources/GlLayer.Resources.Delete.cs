namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted textures and releases their tracked memory.</remarks>
    public override void DeleteTextures(int n, nint textures)
    {
        var ids = this.textures.Untrack(nameof(DeleteTextures), n, textures);
        foreach (var id in ids) { textureTargets.Remove(id); ReleaseTextureMemory(id); }
        base.DeleteTextures(n, textures);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted buffers and releases their tracked memory.</remarks>
    public override void DeleteBuffers(int n, nint buffers)
    {
        var ids = this.buffers.Untrack(nameof(DeleteBuffers), n, buffers);
        foreach (var id in ids) ReleaseBufferMemory(id);
        base.DeleteBuffers(n, buffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted vertex arrays.</remarks>
    public override void DeleteVertexArrays(int n, nint arrays) { vertexArrays.Untrack(nameof(DeleteVertexArrays), n, arrays); base.DeleteVertexArrays(n, arrays); }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted framebuffers.</remarks>
    public override void DeleteFramebuffers(int n, nint framebuffers)
    {
        this.framebuffers.Untrack(nameof(DeleteFramebuffers), n, framebuffers);
        base.DeleteFramebuffers(n, framebuffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted renderbuffers and releases their tracked memory.</remarks>
    public override void DeleteRenderbuffers(int n, nint renderbuffers)
    {
        var ids = this.renderbuffers.Untrack(nameof(DeleteRenderbuffers), n, renderbuffers);
        foreach (var id in ids) ReleaseRenderbufferMemory(id);
        base.DeleteRenderbuffers(n, renderbuffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted samplers.</remarks>
    public override void DeleteSamplers(int count, nint samplers)
    {
        this.samplers.Untrack(nameof(DeleteSamplers), count, samplers);
        base.DeleteSamplers(count, samplers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted queries.</remarks>
    public override void DeleteQueries(int n, nint ids) { queries.Untrack(nameof(DeleteQueries), n, ids); base.DeleteQueries(n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program pipelines.</remarks>
    public override void DeleteProgramPipelines(int n, nint pipelines)
    {
        programPipelines.Untrack(nameof(DeleteProgramPipelines), n, pipelines);
        base.DeleteProgramPipelines(n, pipelines);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted transform feedback objects.</remarks>
    public override void DeleteTransformFeedbacks(int n, nint ids)
    {
        transformFeedbacks.Untrack(nameof(DeleteTransformFeedbacks), n, ids);
        base.DeleteTransformFeedbacks(n, ids);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted shader.</remarks>
    public override void DeleteShader(GlShaderHandle shader) { shaders.Untrack(nameof(DeleteShader), shader); base.DeleteShader(shader); }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program.</remarks>
    public override void DeleteProgram(GlProgramHandle program) { programs.Untrack(nameof(DeleteProgram), program); base.DeleteProgram(program); }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted sync object.</remarks>
    public override void DeleteSync(nint sync)
    {
        if (!syncs.Remove(sync))
            throw new GlResourceNotTrackedException<nint>(nameof(DeleteSync), "sync", sync);
        base.DeleteSync(sync);
    }
}
