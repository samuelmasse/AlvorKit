namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created textures, deleted when the layer is disposed.</remarks>
    public override void CreateTextures(GlTextureTarget target, int n, nint textures)
    {
        base.CreateTextures(target, n, textures);
        this.textures.Track(n, textures);
        var ids = (uint*)textures;
        for (var i = 0; i < n; i++)
            TrackTextureTarget(nameof(CreateTextures), (GlTextureHandle)ids[i], target);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created buffers, deleted when the layer is disposed.</remarks>
    public override void CreateBuffers(int n, nint buffers) { base.CreateBuffers(n, buffers); this.buffers.Track(n, buffers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created vertex arrays, deleted when the layer is disposed.</remarks>
    public override void CreateVertexArrays(int n, nint arrays) { base.CreateVertexArrays(n, arrays); vertexArrays.Track(n, arrays); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created framebuffers, deleted when the layer is disposed.</remarks>
    public override void CreateFramebuffers(int n, nint framebuffers) { base.CreateFramebuffers(n, framebuffers); this.framebuffers.Track(n, framebuffers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created renderbuffers, deleted when the layer is disposed.</remarks>
    public override void CreateRenderbuffers(int n, nint renderbuffers) { base.CreateRenderbuffers(n, renderbuffers); this.renderbuffers.Track(n, renderbuffers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created samplers, deleted when the layer is disposed.</remarks>
    public override void CreateSamplers(int n, nint samplers) { base.CreateSamplers(n, samplers); this.samplers.Track(n, samplers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created queries, deleted when the layer is disposed.</remarks>
    public override void CreateQueries(GlQueryTarget target, int n, nint ids) { base.CreateQueries(target, n, ids); queries.Track(n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program pipelines, deleted when the layer is disposed.</remarks>
    public override void CreateProgramPipelines(int n, nint pipelines) { base.CreateProgramPipelines(n, pipelines); programPipelines.Track(n, pipelines); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created transform feedback objects, deleted when the layer is disposed.</remarks>
    public override void CreateTransformFeedbacks(int n, nint ids) { base.CreateTransformFeedbacks(n, ids); transformFeedbacks.Track(n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created shader, deleted when the layer is disposed.</remarks>
    public override GlShaderHandle CreateShader(GlShaderType type) { var id = base.CreateShader(type); shaders.Track(id); return id; }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program, deleted when the layer is disposed.</remarks>
    public override GlProgramHandle CreateProgram() { var id = base.CreateProgram(); programs.Track(id); return id; }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program, deleted when the layer is disposed.</remarks>
    public override GlProgramHandle CreateShaderProgramv(GlShaderType type, int count, nint strings)
    {
        var id = base.CreateShaderProgramv(type, count, strings);
        programs.Track(id);
        return id;
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created sync object, deleted when the layer is disposed.</remarks>
    public override nint FenceSync(GlSyncCondition condition, GlSyncBehaviorFlags flags)
    {
        var sync = base.FenceSync(condition, flags);
        syncs.Add(sync);
        return sync;
    }
}
