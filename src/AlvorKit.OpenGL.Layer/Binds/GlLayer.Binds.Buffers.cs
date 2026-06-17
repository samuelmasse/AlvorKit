namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindBuffer"/> for the same target.
    /// </remarks>
    public override void BindBuffer(GlBufferTarget target, GlBufferHandle buffer)
    {
        var id = (uint)buffer;
        bufferBinds.RequireCanBind(nameof(BindBuffer), target, id);
        base.BindBuffer(target, buffer);
        bufferBinds.BindKnownFree(target, id);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindBufferBase"/> for the same target and index.
    /// </remarks>
    public override void BindBufferBase(GlBufferTarget target, uint index, GlBufferHandle buffer)
    {
        var id = (uint)buffer;
        bufferBinds.RequireCanBind(nameof(BindBufferBase), target, id);
        indexedBufferBinds.RequireCanBind(nameof(BindBufferBase), (target, index), id);
        base.BindBufferBase(target, index, buffer);
        bufferBinds.BindKnownFree(target, id);
        indexedBufferBinds.BindKnownFree((target, index), id);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindBufferRange"/> for the same target and index.
    /// </remarks>
    public override void BindBufferRange(
        GlBufferTarget target,
        uint index,
        GlBufferHandle buffer,
        nint offset,
        nint size)
    {
        var id = (uint)buffer;
        bufferBinds.RequireCanBind(nameof(BindBufferRange), target, id);
        indexedBufferBinds.RequireCanBind(nameof(BindBufferRange), (target, index), id);
        base.BindBufferRange(target, index, buffer, offset, size);
        bufferBinds.BindKnownFree(target, id);
        indexedBufferBinds.BindKnownFree((target, index), id);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindVertexBuffer"/> for the same binding index.
    /// </remarks>
    public override void BindVertexBuffer(uint bindingindex, GlBufferHandle buffer, nint offset, int stride)
    {
        var id = (uint)buffer;
        vertexBufferBinds.RequireCanBind(nameof(BindVertexBuffer), bindingindex, id);
        base.BindVertexBuffer(bindingindex, buffer, offset, stride);
        vertexBufferBinds.BindKnownFree(bindingindex, id);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with
    /// exactly one later call to <see cref="UnbindBuffersBase"/> for the same target and range.
    /// </remarks>
    public override void BindBuffersBase(GlBufferTarget target, uint first, int count, nint buffers)
    {
        var ids = (uint*)buffers;
        var finalId = count > 0 && buffers != 0 ? ids[count - 1] : 0u;
        if (count > 0)
            bufferBinds.RequireCanBind(nameof(BindBuffersBase), target, finalId);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.RequireCanBind(nameof(BindBuffersBase), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersBase(target, first, count, buffers);
        if (count > 0)
            bufferBinds.BindKnownFree(target, finalId);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.BindKnownFree((target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with
    /// exactly one later call to <see cref="UnbindBuffersRange"/> for the same target and range.
    /// </remarks>
    public override void BindBuffersRange(
        GlBufferTarget target,
        uint first,
        int count,
        nint buffers,
        nint offsets,
        nint sizes)
    {
        var ids = (uint*)buffers;
        var finalId = count > 0 && buffers != 0 ? ids[count - 1] : 0u;
        if (count > 0)
            bufferBinds.RequireCanBind(nameof(BindBuffersRange), target, finalId);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.RequireCanBind(nameof(BindBuffersRange), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersRange(target, first, count, buffers, offsets, sizes);
        if (count > 0)
            bufferBinds.BindKnownFree(target, finalId);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.BindKnownFree((target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer to vertex binding points <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindVertexBuffers"/> for the same range.
    /// </remarks>
    public override void BindVertexBuffers(uint first, int count, nint buffers, nint offsets, nint strides)
    {
        var ids = (uint*)buffers;
        for (var i = 0; i < count; i++)
            vertexBufferBinds.RequireCanBind(nameof(BindVertexBuffers), first + (uint)i, buffers == 0 ? 0u : ids[i]);
        base.BindVertexBuffers(first, count, buffers, offsets, strides);
        for (var i = 0; i < count; i++)
            vertexBufferBinds.BindKnownFree(first + (uint)i, buffers == 0 ? 0u : ids[i]);
    }
}
