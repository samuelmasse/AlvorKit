namespace AlvorKit.Graphics2D;

/// <summary>Owns the dynamic vertex buffer used by the sprite batch.</summary>
/// <typeparam name="T">The unmanaged vertex type stored in the buffer.</typeparam>
internal class QuadVertexBuffer<T>(GlLayer gl, int vertexSize) : IDisposable
    where T : unmanaged
{
    /// <summary>The tracked array-buffer handle.</summary>
    private readonly GlBufferHandle id = gl.GenBuffer();

    /// <summary>The number of vertices that fit in the current buffer allocation.</summary>
    private int capacity;

    /// <summary>Gets the tracked array-buffer handle.</summary>
    internal GlBufferHandle Id => id;

    /// <summary>Gets the number of vertices that fit in the current buffer allocation.</summary>
    internal int Capacity => capacity;

    /// <summary>Transfers a caller-owned vertex span into the buffer, growing storage only when needed.</summary>
    /// <param name="data">The vertex data to upload for the duration of the call.</param>
    internal void Transfer(ReadOnlySpan<T> data)
    {
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, id);

        if (capacity <= data.Length)
        {
            var newCapacity = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)data.Length + 1u);
            gl.BufferData(GlBufferTarget.ArrayBuffer, newCapacity * vertexSize, 0, GlBufferUsage.DynamicDraw);
            capacity = newCapacity;
        }

        gl.BufferSubData(GlBufferTarget.ArrayBuffer, 0, data);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
    }

    /// <summary>Deletes the tracked array-buffer handle.</summary>
    public void Dispose() => gl.DeleteBuffer(id);
}
