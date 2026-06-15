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
        bufferBinds.Bind(nameof(BindBuffer), target, id);
        base.BindBuffer(target, buffer);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindBufferBase"/> for the same target and index.
    /// </remarks>
    public override void BindBufferBase(GlBufferTarget target, uint index, GlBufferHandle buffer)
    {
        bufferBinds.Bind(nameof(BindBufferBase), target, (uint)buffer);
        indexedBufferBinds.Bind(nameof(BindBufferBase), (target, index), (uint)buffer);
        base.BindBufferBase(target, index, buffer);
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
        bufferBinds.Bind(nameof(BindBufferRange), target, (uint)buffer);
        indexedBufferBinds.Bind(nameof(BindBufferRange), (target, index), (uint)buffer);
        base.BindBufferRange(target, index, buffer, offset, size);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindVertexBuffer"/> for the same binding index.
    /// </remarks>
    public override void BindVertexBuffer(uint bindingindex, GlBufferHandle buffer, nint offset, int stride)
    {
        vertexBufferBinds.Bind(nameof(BindVertexBuffer), bindingindex, (uint)buffer);
        base.BindVertexBuffer(bindingindex, buffer, offset, stride);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with
    /// exactly one later call to <see cref="UnbindBuffersBase"/> for the same target and range.
    /// </remarks>
    public override void BindBuffersBase(GlBufferTarget target, uint first, int count, nint buffers)
    {
        var ids = (uint*)buffers;
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersBase), target, buffers == 0 ? 0u : ids[count - 1]);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Bind(nameof(BindBuffersBase), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersBase(target, first, count, buffers);
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
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersRange), target, buffers == 0 ? 0u : ids[count - 1]);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Bind(nameof(BindBuffersRange), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersRange(target, first, count, buffers, offsets, sizes);
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
            vertexBufferBinds.Bind(nameof(BindVertexBuffers), first + (uint)i, buffers == 0 ? 0u : ids[i]);
        base.BindVertexBuffers(first, count, buffers, offsets, strides);
    }
}
