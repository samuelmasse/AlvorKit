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
        bufferBinds.Unbind(nameof(BindBuffer), target);
        base.BindBuffer(target, (GlBufferHandle)0u);
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindBufferBase</c> for <paramref name="target"/> at <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glBindBufferBase</c>.
    /// </summary>
    public void UnbindBufferBase(GlBufferTarget target, uint index)
    {
        bufferBinds.Bind(nameof(BindBufferBase), target, 0);
        indexedBufferBinds.Unbind(nameof(BindBufferBase), (target, index));
        base.BindBufferBase(target, index, (GlBufferHandle)0u);
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindBufferRange</c> for <paramref name="target"/> at <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glBindBufferRange</c>.
    /// </summary>
    public void UnbindBufferRange(GlBufferTarget target, uint index)
    {
        bufferBinds.Bind(nameof(BindBufferRange), target, 0);
        indexedBufferBinds.Unbind(nameof(BindBufferRange), (target, index));
        base.BindBufferRange(target, index, (GlBufferHandle)0u, 0, 0);
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindVertexBuffer</c> for binding <paramref name="bindingindex"/>.
    /// Must be paired with exactly one earlier call to <c>glBindVertexBuffer</c>.
    /// </summary>
    public void UnbindVertexBuffer(uint bindingindex)
    {
        vertexBufferBinds.Unbind(nameof(BindVertexBuffer), bindingindex);
        base.BindVertexBuffer(bindingindex, (GlBufferHandle)0u, 0, 0);
    }

    /// <summary>
    /// Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersBase(GlBufferTarget, uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindBuffersBase(GlBufferTarget, uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindBuffersBase(GlBufferTarget target, uint first, int count)
    {
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersBase), target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Unbind(nameof(BindBuffersBase), (target, first + (uint)i));
        uint* buffers = stackalloc uint[count];
        base.BindBuffersBase(target, first, count, (nint)buffers);
    }

    /// <summary>
    /// Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersRange(GlBufferTarget, uint, int, nint, nint, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindBuffersRange(GlBufferTarget, uint, int, nint, nint, nint)"/> for the same range.
    /// </summary>
    public void UnbindBuffersRange(GlBufferTarget target, uint first, int count)
    {
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersRange), target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Unbind(nameof(BindBuffersRange), (target, first + (uint)i));
        uint* buffers = stackalloc uint[count];
        nint* offsets = stackalloc nint[count];
        nint* sizes = stackalloc nint[count];
        base.BindBuffersRange(target, first, count, (nint)buffers, (nint)offsets, (nint)sizes);
    }

    /// <summary>
    /// Layer: Unbinds the range of vertex buffers bound by <see cref="BindVertexBuffers(uint, int, nint, nint, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindVertexBuffers(uint, int, nint, nint, nint)"/> for the same range.
    /// </summary>
    public void UnbindVertexBuffers(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            vertexBufferBinds.Unbind(nameof(BindVertexBuffers), first + (uint)i);
        uint* buffers = stackalloc uint[count];
        nint* offsets = stackalloc nint[count];
        int* strides = stackalloc int[count];
        base.BindVertexBuffers(first, count, (nint)buffers, (nint)offsets, (nint)strides);
    }
}
