namespace AlvorKit.Graphics2D;

/// <summary>Owns the vertex array object that captures sprite batch vertex layout.</summary>
internal class QuadVertexArray : IDisposable
{
    /// <summary>The strict OpenGL command surface that owns the vertex array.</summary>
    private readonly GlLayer gl;

    /// <summary>The vertex buffer captured by the vertex array.</summary>
    private readonly QuadVertexBuffer<SpriteBatchVertex> vertexBuffer;

    /// <summary>The index buffer captured by the vertex array.</summary>
    private readonly QuadIndexBuffer indexBuffer;

    /// <summary>The tracked vertex-array handle.</summary>
    private readonly GlVertexArrayHandle id;

    /// <summary>Gets the tracked vertex-array handle.</summary>
    internal GlVertexArrayHandle Id => id;

    /// <summary>Gets the vertex buffer captured by the vertex array.</summary>
    internal QuadVertexBuffer<SpriteBatchVertex> VertexBuffer => vertexBuffer;

    /// <summary>Gets the index buffer captured by the vertex array.</summary>
    internal QuadIndexBuffer IndexBuffer => indexBuffer;

    /// <summary>Creates a vertex array and records the supplied attribute layout.</summary>
    internal QuadVertexArray(
        GlLayer gl,
        Action descriptor,
        QuadVertexBuffer<SpriteBatchVertex> vertexBuffer,
        QuadIndexBuffer indexBuffer)
    {
        this.gl = gl;
        this.vertexBuffer = vertexBuffer;
        this.indexBuffer = indexBuffer;

        id = gl.GenVertexArray();
        gl.BindVertexArray(id);
        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, indexBuffer.Id);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer.Id);
        descriptor.Invoke();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
    }

    /// <summary>Deletes the tracked vertex-array handle.</summary>
    public void Dispose() => gl.DeleteVertexArray(id);
}
