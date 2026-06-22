namespace AlvorKit.Engine;

/// <summary>Queues OpenGL object deletes so finalizers can defer cleanup to the root GL thread.</summary>
public class GlBin : IDisposable
{
    private readonly RootGl gl;
    private readonly GlBin? parent;
    private readonly List<GlBin> children = [];
    private readonly ConcurrentQueue<GlTextureHandle> textures = new();
    private readonly ConcurrentQueue<GlBufferHandle> buffers = new();
    private readonly ConcurrentQueue<GlRenderbufferHandle> renderbuffers = new();
    private readonly ConcurrentQueue<GlVertexArrayHandle> vertexArrays = new();
    private readonly ConcurrentQueue<GlFramebufferHandle> framebuffers = new();

    /// <summary>Creates a bin attached to an optional parent bin.</summary>
    public GlBin(RootGl gl, GlBin? parent)
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

    /// <summary>Deletes all queued objects on the current thread.</summary>
    public void Empty()
    {
        while (textures.TryDequeue(out var texture))
            gl.DeleteTexture(texture);
        while (buffers.TryDequeue(out var buffer))
            gl.DeleteBuffer(buffer);
        while (renderbuffers.TryDequeue(out var renderbuffer))
            gl.DeleteRenderbuffer(renderbuffer);
        while (vertexArrays.TryDequeue(out var vertexArray))
            gl.DeleteVertexArray(vertexArray);
        while (framebuffers.TryDequeue(out var framebuffer))
            gl.DeleteFramebuffer(framebuffer);
    }

    /// <summary>Drains queued deletes and detaches this bin from its parent.</summary>
    public void Dispose()
    {
        Empty();
        parent?.children.Remove(this);
    }
}
