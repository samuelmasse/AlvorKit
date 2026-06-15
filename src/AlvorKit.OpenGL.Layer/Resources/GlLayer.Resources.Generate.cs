namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated textures, deleted when the layer is disposed.</remarks>
    public override void GenTextures(int n, nint textures) { base.GenTextures(n, textures); this.textures.Track(n, textures); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated buffers, deleted when the layer is disposed.</remarks>
    public override void GenBuffers(int n, nint buffers) { base.GenBuffers(n, buffers); this.buffers.Track(n, buffers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated vertex arrays, deleted when the layer is disposed.</remarks>
    public override void GenVertexArrays(int n, nint arrays) { base.GenVertexArrays(n, arrays); vertexArrays.Track(n, arrays); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated framebuffers, deleted when the layer is disposed.</remarks>
    public override void GenFramebuffers(int n, nint framebuffers) { base.GenFramebuffers(n, framebuffers); this.framebuffers.Track(n, framebuffers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated renderbuffers, deleted when the layer is disposed.</remarks>
    public override void GenRenderbuffers(int n, nint renderbuffers) { base.GenRenderbuffers(n, renderbuffers); this.renderbuffers.Track(n, renderbuffers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated samplers, deleted when the layer is disposed.</remarks>
    public override void GenSamplers(int count, nint samplers) { base.GenSamplers(count, samplers); this.samplers.Track(count, samplers); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated queries, deleted when the layer is disposed.</remarks>
    public override void GenQueries(int n, nint ids) { base.GenQueries(n, ids); queries.Track(n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated program pipelines, deleted when the layer is disposed.</remarks>
    public override void GenProgramPipelines(int n, nint pipelines) { base.GenProgramPipelines(n, pipelines); programPipelines.Track(n, pipelines); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated transform feedback objects, deleted when the layer is disposed.</remarks>
    public override void GenTransformFeedbacks(int n, nint ids) { base.GenTransformFeedbacks(n, ids); transformFeedbacks.Track(n, ids); }
}
