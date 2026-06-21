namespace AlvorKit.Graphics2D;

/// <summary>Owns the reusable index buffer that expands quads into indexed triangles.</summary>
internal class QuadIndexBuffer(GlLayer gl) : IDisposable
{
    /// <summary>The tracked element-array buffer handle.</summary>
    private readonly GlBufferHandle id = gl.GenBuffer();

    /// <summary>The vertex capacity represented by the current index data.</summary>
    private int capacity;

    /// <summary>Gets the tracked element-array buffer handle.</summary>
    internal GlBufferHandle Id => id;

    /// <summary>Gets the vertex capacity represented by the current index data.</summary>
    internal int Capacity => capacity;

    /// <summary>
    /// Ensures enough indices exist for the requested quad vertex count.
    /// The managed array allocation occurs only when capacity grows.
    /// </summary>
    /// <param name="quadVertexCount">The number of quad vertices that may be drawn.</param>
    internal void EnsureCapacity(int quadVertexCount)
    {
        if (quadVertexCount <= capacity)
            return;

        var newCapacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)quadVertexCount + 1u);
        var indexCount = newCapacity / 4 * 6;
        var indexValues = new uint[indexCount];
        uint vertexIndex = 0;

        for (var i = 0; i < indexCount; i += 6)
        {
            indexValues[i] = vertexIndex + 2u;
            indexValues[i + 1] = vertexIndex + 3u;
            indexValues[i + 2] = vertexIndex + 1u;
            indexValues[i + 3] = vertexIndex + 1u;
            indexValues[i + 4] = vertexIndex;
            indexValues[i + 5] = vertexIndex + 2u;
            vertexIndex += 4u;
        }

        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, id);
        gl.BufferData(GlBufferTarget.ElementArrayBuffer, indexValues, GlBufferUsage.StaticDraw);
        capacity = newCapacity;
    }

    /// <summary>Deletes the tracked element-array buffer handle.</summary>
    public void Dispose() => gl.DeleteBuffer(id);
}
