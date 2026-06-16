namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with
    /// exactly one later call to <see cref="UnbindBuffersBase"/> for the same target and range.
    /// </remarks>
    public override void BindBuffersBase(GlBufferTarget target, uint first, ReadOnlySpan<GlBufferHandle> buffers) =>
        base.BindBuffersBase(target, first, buffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with
    /// exactly one later call to <see cref="UnbindBuffersRange"/> for the same target and range.
    /// </remarks>
    public override void BindBuffersRange(
        GlBufferTarget target,
        uint first,
        ReadOnlySpan<GlBufferHandle> buffers,
        ReadOnlySpan<nint> offsets,
        ReadOnlySpan<nint> sizes) =>
        base.BindBuffersRange(target, first, buffers, offsets, sizes);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each buffer to vertex binding points <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindVertexBuffers"/> for the same range.
    /// </remarks>
    public override void BindVertexBuffers(
        uint first,
        ReadOnlySpan<GlBufferHandle> buffers,
        ReadOnlySpan<nint> offsets,
        ReadOnlySpan<int> strides) =>
        base.BindVertexBuffers(first, buffers, offsets, strides);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each sampler to texture units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindSamplers"/> for the same range.
    /// </remarks>
    public override void BindSamplers(uint first, ReadOnlySpan<GlSamplerHandle> samplers) => base.BindSamplers(first, samplers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each texture to image units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindImageTextures"/> for the same range.
    /// </remarks>
    public override void BindImageTextures(uint first, ReadOnlySpan<GlTextureHandle> textures) => base.BindImageTextures(first, textures);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each texture to units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindTextures"/> for the same range.
    /// </remarks>
    public override void BindTextures(uint first, ReadOnlySpan<GlTextureHandle> textures) => base.BindTextures(first, textures);
}
