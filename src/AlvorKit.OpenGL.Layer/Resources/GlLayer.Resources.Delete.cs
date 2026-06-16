namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted textures to be unbound, then releases their tracked memory.</remarks>
    public override void DeleteTextures(int n, nint textures)
    {
        var ids = this.textures.RequireTracked(nameof(DeleteTextures), n, textures);
        RequireTexturesUnbound(nameof(DeleteTextures), ids);
        this.textures.UntrackKnown(ids);
        foreach (var id in ids) { textureTargets.Remove(id); ReleaseTextureMemory(id); }
        base.DeleteTextures(n, textures);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted buffers to be unbound, then releases their tracked memory.</remarks>
    public override void DeleteBuffers(int n, nint buffers)
    {
        var ids = this.buffers.RequireTracked(nameof(DeleteBuffers), n, buffers);
        RequireBuffersUnbound(nameof(DeleteBuffers), ids);
        this.buffers.UntrackKnown(ids);
        foreach (var id in ids) ReleaseBufferMemory(id);
        base.DeleteBuffers(n, buffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted vertex arrays to be unbound before removing them from tracking.</remarks>
    public override void DeleteVertexArrays(int n, nint arrays)
    {
        var ids = vertexArrays.RequireTracked(nameof(DeleteVertexArrays), n, arrays);
        RequireVertexArraysUnbound(nameof(DeleteVertexArrays), ids);
        vertexArrays.UntrackKnown(ids);
        base.DeleteVertexArrays(n, arrays);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted framebuffers to be unbound before removing them from tracking.</remarks>
    public override void DeleteFramebuffers(int n, nint framebuffers)
    {
        var ids = this.framebuffers.RequireTracked(nameof(DeleteFramebuffers), n, framebuffers);
        RequireFramebuffersUnbound(nameof(DeleteFramebuffers), ids);
        this.framebuffers.UntrackKnown(ids);
        base.DeleteFramebuffers(n, framebuffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted renderbuffers to be unbound, then releases their tracked memory.</remarks>
    public override void DeleteRenderbuffers(int n, nint renderbuffers)
    {
        var ids = this.renderbuffers.RequireTracked(nameof(DeleteRenderbuffers), n, renderbuffers);
        RequireRenderbuffersUnbound(nameof(DeleteRenderbuffers), ids);
        this.renderbuffers.UntrackKnown(ids);
        foreach (var id in ids) ReleaseRenderbufferMemory(id);
        base.DeleteRenderbuffers(n, renderbuffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted samplers to be unbound before removing them from tracking.</remarks>
    public override void DeleteSamplers(int count, nint samplers)
    {
        var ids = this.samplers.RequireTracked(nameof(DeleteSamplers), count, samplers);
        RequireSamplersUnbound(nameof(DeleteSamplers), ids);
        this.samplers.UntrackKnown(ids);
        base.DeleteSamplers(count, samplers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted queries to be inactive before removing them from tracking.</remarks>
    public override void DeleteQueries(int n, nint ids)
    {
        var queryIds = queries.RequireTracked(nameof(DeleteQueries), n, ids);
        RequireQueriesUnbound(nameof(DeleteQueries), queryIds);
        queries.UntrackKnown(queryIds);
        base.DeleteQueries(n, ids);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted program pipelines to be unbound before removing them from tracking.</remarks>
    public override void DeleteProgramPipelines(int n, nint pipelines)
    {
        var ids = programPipelines.RequireTracked(nameof(DeleteProgramPipelines), n, pipelines);
        RequireProgramPipelinesUnbound(nameof(DeleteProgramPipelines), ids);
        programPipelines.UntrackKnown(ids);
        base.DeleteProgramPipelines(n, pipelines);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted transform feedback objects to be unbound before removing them from tracking.</remarks>
    public override void DeleteTransformFeedbacks(int n, nint ids)
    {
        var feedbacks = transformFeedbacks.RequireTracked(nameof(DeleteTransformFeedbacks), n, ids);
        RequireTransformFeedbacksUnbound(nameof(DeleteTransformFeedbacks), feedbacks);
        transformFeedbacks.UntrackKnown(feedbacks);
        base.DeleteTransformFeedbacks(n, ids);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted shader.</remarks>
    public override void DeleteShader(GlShaderHandle shader) { shaders.Untrack(nameof(DeleteShader), shader); base.DeleteShader(shader); }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted program to be unused before removing it from tracking.</remarks>
    public override void DeleteProgram(GlProgramHandle program)
    {
        if (!programs.Contains(program))
            throw new GlResourceNotTrackedException<GlProgramHandle>(nameof(DeleteProgram), "program", program);
        RequireProgramUnbound(nameof(DeleteProgram), program);
        programs.Untrack(nameof(DeleteProgram), program);
        base.DeleteProgram(program);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted sync object.</remarks>
    public override void DeleteSync(nint sync)
    {
        if (!syncs.Remove(sync))
            throw new GlResourceNotTrackedException<nint>(nameof(DeleteSync), "sync", sync);
        base.DeleteSync(sync);
    }
}
