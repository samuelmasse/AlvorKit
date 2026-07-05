namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted textures to be unbound, then releases their tracked memory.</remarks>
    public override void DeleteTextures(int n, nint textures)
    {
        var ids = RequireTrackedInTree(nameof(DeleteTextures), static layer => layer.textures, n, textures);
        RequireTexturesUnbound(nameof(DeleteTextures), ids);
        UntrackInTree(static layer => layer.textures, ids);
        foreach (var id in ids) { state.textureTargets.Remove(id); ReleaseTextureMemory(id); }
        base.DeleteTextures(n, textures);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted buffers to be unbound, then releases their tracked memory.</remarks>
    public override void DeleteBuffers(int n, nint buffers)
    {
        var ids = RequireTrackedInTree(nameof(DeleteBuffers), static layer => layer.buffers, n, buffers);
        RequireBuffersUnbound(nameof(DeleteBuffers), ids);
        UntrackInTree(static layer => layer.buffers, ids);
        foreach (var id in ids) ReleaseBufferMemory(id);
        base.DeleteBuffers(n, buffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted vertex arrays to be unbound before removing them from tracking.</remarks>
    public override void DeleteVertexArrays(int n, nint arrays)
    {
        var ids = RequireTrackedInTree(nameof(DeleteVertexArrays), static layer => layer.vertexArrays, n, arrays);
        RequireVertexArraysUnbound(nameof(DeleteVertexArrays), ids);
        UntrackInTree(static layer => layer.vertexArrays, ids);
        base.DeleteVertexArrays(n, arrays);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted framebuffers to be unbound before removing them from tracking.</remarks>
    public override void DeleteFramebuffers(int n, nint framebuffers)
    {
        var ids = RequireTrackedInTree(nameof(DeleteFramebuffers), static layer => layer.framebuffers, n, framebuffers);
        RequireFramebuffersUnbound(nameof(DeleteFramebuffers), ids);
        UntrackInTree(static layer => layer.framebuffers, ids);
        base.DeleteFramebuffers(n, framebuffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted renderbuffers to be unbound, then releases their tracked memory.</remarks>
    public override void DeleteRenderbuffers(int n, nint renderbuffers)
    {
        var ids = RequireTrackedInTree(nameof(DeleteRenderbuffers), static layer => layer.renderbuffers, n, renderbuffers);
        RequireRenderbuffersUnbound(nameof(DeleteRenderbuffers), ids);
        UntrackInTree(static layer => layer.renderbuffers, ids);
        foreach (var id in ids) ReleaseRenderbufferMemory(id);
        base.DeleteRenderbuffers(n, renderbuffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted samplers to be unbound before removing them from tracking.</remarks>
    public override void DeleteSamplers(int count, nint samplers)
    {
        var ids = RequireTrackedInTree(nameof(DeleteSamplers), static layer => layer.samplers, count, samplers);
        RequireSamplersUnbound(nameof(DeleteSamplers), ids);
        UntrackInTree(static layer => layer.samplers, ids);
        base.DeleteSamplers(count, samplers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted queries to be inactive before removing them from tracking.</remarks>
    public override void DeleteQueries(int n, nint ids)
    {
        var queryIds = RequireTrackedInTree(nameof(DeleteQueries), static layer => layer.queries, n, ids);
        RequireQueriesUnbound(nameof(DeleteQueries), queryIds);
        UntrackInTree(static layer => layer.queries, queryIds);
        base.DeleteQueries(n, ids);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted program pipelines to be unbound before removing them from tracking.</remarks>
    public override void DeleteProgramPipelines(int n, nint pipelines)
    {
        var ids = RequireTrackedInTree(nameof(DeleteProgramPipelines), static layer => layer.programPipelines, n, pipelines);
        RequireProgramPipelinesUnbound(nameof(DeleteProgramPipelines), ids);
        UntrackInTree(static layer => layer.programPipelines, ids);
        base.DeleteProgramPipelines(n, pipelines);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted transform feedback objects to be unbound before removing them from tracking.</remarks>
    public override void DeleteTransformFeedbacks(int n, nint ids)
    {
        var feedbacks = RequireTrackedInTree(nameof(DeleteTransformFeedbacks), static layer => layer.transformFeedbacks, n, ids);
        RequireTransformFeedbacksUnbound(nameof(DeleteTransformFeedbacks), feedbacks);
        UntrackInTree(static layer => layer.transformFeedbacks, feedbacks);
        base.DeleteTransformFeedbacks(n, ids);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted shader on the node that owns it.</remarks>
    public override void DeleteShader(GlShaderHandle shader)
    {
        var owner = FindOwner(static layer => layer.shaders, shader)
            ?? throw new GlResourceNotTrackedException<GlShaderHandle>(nameof(DeleteShader), "shader", shader);
        owner.shaders.TryUntrack(shader);
        base.DeleteShader(shader);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: requires the deleted program to be unused before removing it from tracking.</remarks>
    public override void DeleteProgram(GlProgramHandle program)
    {
        var owner = FindOwner(static layer => layer.programs, program)
            ?? throw new GlResourceNotTrackedException<GlProgramHandle>(nameof(DeleteProgram), "program", program);
        RequireProgramUnbound(nameof(DeleteProgram), program);
        owner.programs.TryUntrack(program);
        base.DeleteProgram(program);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted sync object on the node that owns it.</remarks>
    public override void DeleteSync(nint sync)
    {
        var owner = FindSyncOwner(sync)
            ?? throw new GlResourceNotTrackedException<nint>(nameof(DeleteSync), "sync", sync);
        owner.syncs.Remove(sync);
        base.DeleteSync(sync);
    }

    /// <summary>
    /// Finds the node in this layer's ancestry (self first) that tracks a sync object.
    /// </summary>
    /// <param name="sync">The sync object pointer to look up.</param>
    /// <returns>The owning node, or null when no ancestor tracks the sync object.</returns>
    private GlLayer? FindSyncOwner(nint sync)
    {
        for (var node = this; node is not null; node = node.parent)
            if (node.syncs.Contains(sync))
                return node;
        return null;
    }
}
