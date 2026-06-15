namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindVertexArray"/>.
    /// Cannot bind a VAO while a buffer is bound to <see cref="GlBufferTarget.ArrayBuffer"/>
    /// or <see cref="GlBufferTarget.ElementArrayBuffer"/>.
    /// </remarks>
    public override void BindVertexArray(GlVertexArrayHandle array)
    {
        var id = (uint)array;
        if (id != 0)
        {
            if (bufferBinds.TryGet(GlBufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but buffer {vbo} is still bound to ArrayBuffer.");
            if (bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out var ebo) && ebo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but buffer {ebo} is still bound to ElementArrayBuffer.");
            if (vertexBufferBinds.HasAny)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but vertex buffer bindings are still set.");
        }
        vertexArray.Bind(nameof(BindVertexArray), id);
        base.BindVertexArray(array);
    }

    /// <summary>Layer: Unbinds the vertex array. Must be paired with exactly one earlier call to <c>glBindVertexArray</c>.</summary>
    public void UnbindVertexArray()
    {
        if (bufferBinds.TryGet(GlBufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
            throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to unbind VAO, but buffer {vbo} is still bound to ArrayBuffer.");
        vertexArray.Unbind(nameof(BindVertexArray));
        if (bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out _))
            bufferBinds.Unbind(nameof(BindVertexArray), GlBufferTarget.ElementArrayBuffer);
        vertexBufferBinds.Clear();
        base.BindVertexArray((GlVertexArrayHandle)0u);
    }
}
