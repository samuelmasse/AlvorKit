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
            if (state.bufferBinds.TryGet(GlBufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but buffer {vbo} is still bound to ArrayBuffer.");
            if (state.bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out var ebo) && ebo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but buffer {ebo} is still bound to ElementArrayBuffer.");
            if (state.vertexBufferBinds.HasAny)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but vertex buffer bindings are still set.");
        }
        state.vertexArray.RequireCanBind(nameof(BindVertexArray), id);
        base.BindVertexArray(array);
        state.vertexArray.BindKnownFree(id);
    }

    /// <summary>Layer: Unbinds the vertex array. Must be paired with exactly one earlier call to <c>glBindVertexArray</c>.</summary>
    public void UnbindVertexArray()
    {
        if (state.bufferBinds.TryGet(GlBufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
            throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to unbind VAO, but buffer {vbo} is still bound to ArrayBuffer.");
        state.vertexArray.RequireCanUnbind(nameof(BindVertexArray));
        if (state.bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out _))
            state.bufferBinds.RequireCanUnbind(nameof(BindVertexArray), GlBufferTarget.ElementArrayBuffer);
        base.BindVertexArray((GlVertexArrayHandle)0u);
        state.vertexArray.UnbindKnownBound();
        if (state.bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out _))
            state.bufferBinds.UnbindKnownBound(GlBufferTarget.ElementArrayBuffer);
        state.vertexBufferBinds.Clear();
    }
}
