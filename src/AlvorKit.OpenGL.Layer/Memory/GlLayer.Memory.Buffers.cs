namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.</remarks>
    public override void BufferData(GlBufferTarget target, nint size, nint data, GlBufferUsage usage)
    {
        TrackBoundBufferSize(nameof(BufferData), target, (long)size);
        base.BufferData(target, size, data, usage);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.</remarks>
    public override void BufferStorage(GlBufferStorageTarget target, nint size, nint data, GlBufferStorageMask flags)
    {
        TrackBoundBufferSize(nameof(BufferStorage), (GlBufferTarget)(uint)target, (long)size);
        base.BufferStorage(target, size, data, flags);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of buffer <paramref name="buffer"/>.</remarks>
    public override void NamedBufferData(GlBufferHandle buffer, nint size, nint data, GlBufferUsage usage)
    {
        TrackBufferSize(nameof(NamedBufferData), buffer, (long)size);
        base.NamedBufferData(buffer, size, data, usage);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of buffer <paramref name="buffer"/>.</remarks>
    public override void NamedBufferStorage(GlBufferHandle buffer, nint size, nint data, GlBufferStorageMask flags)
    {
        TrackBufferSize(nameof(NamedBufferStorage), buffer, (long)size);
        base.NamedBufferStorage(buffer, size, data, flags);
    }

    /// <summary>
    /// Tracks storage bytes for the buffer currently bound to a target.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="target">The buffer target whose bound buffer receives the size.</param>
    /// <param name="size">The new storage size in bytes.</param>
    private void TrackBoundBufferSize(string function, GlBufferTarget target, long size)
    {
        if (!bufferBinds.TryGet(target, out var buffer) || buffer == 0)
            throw new GlException(function, $"cannot track buffer size: no buffer is bound to {target}.");
        TrackBufferSize(function, (GlBufferHandle)buffer, size);
    }

    /// <summary>
    /// Tracks storage bytes for a specific live buffer handle.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="buffer">The buffer handle receiving the size.</param>
    /// <param name="size">The new storage size in bytes.</param>
    private void TrackBufferSize(string function, GlBufferHandle buffer, long size)
    {
        if (!buffers.Contains(buffer))
            throw new GlException(function, $"cannot track buffer size: buffer {buffer} is not tracked.");
        bufferUsage += size - bufferSizes.GetValueOrDefault(buffer);
        bufferSizes[buffer] = size;
    }

    /// <summary>
    /// Releases any tracked storage bytes for a buffer that is being deleted.
    /// </summary>
    /// <param name="buffer">The buffer handle whose memory accounting should be released.</param>
    private void ReleaseBufferMemory(GlBufferHandle buffer)
    {
        if (bufferSizes.Remove(buffer, out var size))
            bufferUsage -= size;
    }
}
