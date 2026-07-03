namespace AlvorKit.Engine;

/// <summary>Queues OpenGL object deletes so finalizers can defer cleanup to the root GL thread.</summary>
public class GlBin : IDisposable
{
    private readonly GlLayer gl;
    private readonly GlBin? parent;
    private readonly List<GlBin> children = [];
    private readonly ConcurrentQueue<GlTextureHandle> textures = new();
    private readonly ConcurrentQueue<GlBufferHandle> buffers = new();
    private readonly ConcurrentQueue<GlRenderbufferHandle> renderbuffers = new();
    private readonly ConcurrentQueue<GlVertexArrayHandle> vertexArrays = new();
    private readonly ConcurrentQueue<GlFramebufferHandle> framebuffers = new();
    private readonly ConcurrentQueue<GlSamplerHandle> samplers = new();
    private readonly ConcurrentQueue<GlQueryHandle> queries = new();
    private readonly ConcurrentQueue<GlProgramPipelineHandle> programPipelines = new();
    private readonly ConcurrentQueue<GlTransformFeedbackHandle> transformFeedbacks = new();
    private readonly ConcurrentQueue<GlShaderHandle> shaders = new();
    private readonly ConcurrentQueue<GlProgramHandle> programs = new();
    private readonly ConcurrentQueue<nint> syncs = new();

    /// <summary>Creates a bin attached to an optional parent bin.</summary>
    public GlBin(GlLayer gl, GlBin? parent)
    {
        this.gl = gl;
        this.parent = parent;
        parent?.children.Add(this);
    }

    /// <summary>Gets child bins without copying.</summary>
    public ReadOnlySpan<GlBin> Children => CollectionsMarshal.AsSpan(children);

    /// <summary>Enqueues a texture for deletion.</summary>
    public void DeleteTexture(GlTextureHandle id) => textures.Enqueue(id);

    /// <summary>Enqueues a buffer for deletion.</summary>
    public void DeleteBuffer(GlBufferHandle id) => buffers.Enqueue(id);

    /// <summary>Enqueues a renderbuffer for deletion.</summary>
    public void DeleteRenderbuffer(GlRenderbufferHandle id) => renderbuffers.Enqueue(id);

    /// <summary>Enqueues a vertex array for deletion.</summary>
    public void DeleteVertexArray(GlVertexArrayHandle id) => vertexArrays.Enqueue(id);

    /// <summary>Enqueues a framebuffer for deletion.</summary>
    public void DeleteFramebuffer(GlFramebufferHandle id) => framebuffers.Enqueue(id);

    /// <summary>Enqueues a sampler for deletion.</summary>
    public void DeleteSampler(GlSamplerHandle id) => samplers.Enqueue(id);

    /// <summary>Enqueues a query for deletion.</summary>
    public void DeleteQuery(GlQueryHandle id) => queries.Enqueue(id);

    /// <summary>Enqueues a program pipeline for deletion.</summary>
    public void DeleteProgramPipeline(GlProgramPipelineHandle id) => programPipelines.Enqueue(id);

    /// <summary>Enqueues a transform feedback object for deletion.</summary>
    public void DeleteTransformFeedback(GlTransformFeedbackHandle id) => transformFeedbacks.Enqueue(id);

    /// <summary>Enqueues a shader for deletion.</summary>
    public void DeleteShader(GlShaderHandle id) => shaders.Enqueue(id);

    /// <summary>Enqueues a program for deletion.</summary>
    public void DeleteProgram(GlProgramHandle id) => programs.Enqueue(id);

    /// <summary>Enqueues a sync object for deletion.</summary>
    public void DeleteSync(nint sync) => syncs.Enqueue(sync);

    /// <summary>Deletes all queued objects on the current thread.</summary>
    public void Empty()
    {
        while (textures.TryDequeue(out var texture)) gl.DeleteTexture(texture);
        while (buffers.TryDequeue(out var buffer)) gl.DeleteBuffer(buffer);
        while (renderbuffers.TryDequeue(out var renderbuffer)) gl.DeleteRenderbuffer(renderbuffer);
        while (vertexArrays.TryDequeue(out var vertexArray)) gl.DeleteVertexArray(vertexArray);
        while (framebuffers.TryDequeue(out var framebuffer)) gl.DeleteFramebuffer(framebuffer);
        while (samplers.TryDequeue(out var sampler)) gl.DeleteSampler(sampler);
        while (queries.TryDequeue(out var query)) gl.DeleteQuery(query);
        while (programPipelines.TryDequeue(out var programPipeline)) gl.DeleteProgramPipeline(programPipeline);
        while (transformFeedbacks.TryDequeue(out var transformFeedback)) gl.DeleteTransformFeedback(transformFeedback);
        while (shaders.TryDequeue(out var shader)) gl.DeleteShader(shader);
        while (programs.TryDequeue(out var program)) gl.DeleteProgram(program);
        while (syncs.TryDequeue(out var sync)) gl.DeleteSync(sync);
    }

    /// <summary>Drains queued deletes and detaches this bin from its parent.</summary>
    public void Dispose()
    {
        Empty();
        parent?.children.Remove(this);
    }
}
