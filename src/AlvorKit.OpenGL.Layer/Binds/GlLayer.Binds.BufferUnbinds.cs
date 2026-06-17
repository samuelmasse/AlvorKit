namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>
    /// Layer: Unbinds <c>glBindBuffer</c> for <paramref name="target"/>.
    /// Must be paired with exactly one earlier call to <c>glBindBuffer</c> for the same target.
    /// </summary>
    public void UnbindBuffer(GlBufferTarget target)
    {
        if (target == GlBufferTarget.ElementArrayBuffer && vertexArray.Current != 0)
            throw new GlBindConflictException(nameof(BindBuffer), "attempted to unbind ElementArrayBuffer while a VAO is still bound.");
        bufferBinds.RequireCanUnbind(nameof(BindBuffer), target);
        base.BindBuffer(target, (GlBufferHandle)0u);
        bufferBinds.UnbindKnownBound(target);
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindBufferBase</c> for <paramref name="target"/> at <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glBindBufferBase</c>.
    /// </summary>
    public void UnbindBufferBase(GlBufferTarget target, uint index)
    {
        bufferBinds.RequireCanBind(nameof(BindBufferBase), target, 0);
        indexedBufferBinds.RequireCanUnbind(nameof(BindBufferBase), (target, index));
        base.BindBufferBase(target, index, (GlBufferHandle)0u);
        bufferBinds.BindKnownFree(target, 0);
        indexedBufferBinds.UnbindKnownBound((target, index));
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindBufferRange</c> for <paramref name="target"/> at <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glBindBufferRange</c>.
    /// </summary>
    public void UnbindBufferRange(GlBufferTarget target, uint index)
    {
        bufferBinds.RequireCanBind(nameof(BindBufferRange), target, 0);
        indexedBufferBinds.RequireCanUnbind(nameof(BindBufferRange), (target, index));
        base.BindBufferRange(target, index, (GlBufferHandle)0u, 0, 0);
        bufferBinds.BindKnownFree(target, 0);
        indexedBufferBinds.UnbindKnownBound((target, index));
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindVertexBuffer</c> for binding <paramref name="bindingindex"/>.
    /// Must be paired with exactly one earlier call to <c>glBindVertexBuffer</c>.
    /// </summary>
    public void UnbindVertexBuffer(uint bindingindex)
    {
        vertexBufferBinds.RequireCanUnbind(nameof(BindVertexBuffer), bindingindex);
        base.BindVertexBuffer(bindingindex, (GlBufferHandle)0u, 0, 0);
        vertexBufferBinds.UnbindKnownBound(bindingindex);
    }

    /// <summary>
    /// Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersBase(GlBufferTarget, uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindBuffersBase(GlBufferTarget, uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindBuffersBase(GlBufferTarget target, uint first, int count)
    {
        if (count > 0)
            bufferBinds.RequireCanBind(nameof(BindBuffersBase), target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.RequireCanUnbind(nameof(BindBuffersBase), (target, first + (uint)i));
        Span<uint> buffers = stackalloc uint[count];
        buffers.Clear();
        fixed (uint* p = buffers)
            base.BindBuffersBase(target, first, count, (nint)p);
        if (count > 0)
            bufferBinds.BindKnownFree(target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.UnbindKnownBound((target, first + (uint)i));
    }

    /// <summary>
    /// Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersRange(GlBufferTarget, uint, int, nint, nint, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindBuffersRange(GlBufferTarget, uint, int, nint, nint, nint)"/> for the same range.
    /// </summary>
    public void UnbindBuffersRange(GlBufferTarget target, uint first, int count)
    {
        if (count > 0)
            bufferBinds.RequireCanBind(nameof(BindBuffersRange), target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.RequireCanUnbind(nameof(BindBuffersRange), (target, first + (uint)i));
        Span<uint> buffers = stackalloc uint[count];
        Span<nint> offsets = stackalloc nint[count];
        Span<nint> sizes = stackalloc nint[count];
        buffers.Clear();
        offsets.Clear();
        sizes.Clear();
        fixed (uint* pBuffers = buffers)
        fixed (nint* pOffsets = offsets)
        fixed (nint* pSizes = sizes)
            base.BindBuffersRange(target, first, count, (nint)pBuffers, (nint)pOffsets, (nint)pSizes);
        if (count > 0)
            bufferBinds.BindKnownFree(target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.UnbindKnownBound((target, first + (uint)i));
    }

    /// <summary>
    /// Layer: Unbinds the range of vertex buffers bound by <see cref="BindVertexBuffers(uint, int, nint, nint, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindVertexBuffers(uint, int, nint, nint, nint)"/> for the same range.
    /// </summary>
    public void UnbindVertexBuffers(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            vertexBufferBinds.RequireCanUnbind(nameof(BindVertexBuffers), first + (uint)i);
        Span<uint> buffers = stackalloc uint[count];
        Span<nint> offsets = stackalloc nint[count];
        Span<int> strides = stackalloc int[count];
        buffers.Clear();
        offsets.Clear();
        strides.Clear();
        fixed (uint* pBuffers = buffers)
        fixed (nint* pOffsets = offsets)
        fixed (int* pStrides = strides)
            base.BindVertexBuffers(first, count, (nint)pBuffers, (nint)pOffsets, (nint)pStrides);
        for (var i = 0; i < count; i++)
            vertexBufferBinds.UnbindKnownBound(first + (uint)i);
    }
}
