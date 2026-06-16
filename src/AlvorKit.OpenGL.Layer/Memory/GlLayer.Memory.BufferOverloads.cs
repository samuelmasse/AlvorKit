namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.
    /// </remarks>
    public override void BufferData<T>(GlBufferTarget target, ReadOnlySpan<T> data, GlBufferUsage usage) =>
        base.BufferData(target, data, usage);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.
    /// </remarks>
    public override void BufferStorage<T>(GlBufferStorageTarget target, ReadOnlySpan<T> data, GlBufferStorageMask flags) =>
        base.BufferStorage(target, data, flags);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of buffer <paramref name="buffer"/>.
    /// </remarks>
    public override void NamedBufferData<T>(GlBufferHandle buffer, ReadOnlySpan<T> data, GlBufferUsage usage) =>
        base.NamedBufferData(buffer, data, usage);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of buffer <paramref name="buffer"/>.
    /// </remarks>
    public override void NamedBufferStorage<T>(GlBufferHandle buffer, ReadOnlySpan<T> data, GlBufferStorageMask flags) =>
        base.NamedBufferStorage(buffer, data, flags);
}
