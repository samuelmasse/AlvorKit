namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted buffers to be unbound, then releases their tracked memory.
    /// </remarks>
    public override void DeleteBuffers(ReadOnlySpan<GlBufferHandle> buffers) => base.DeleteBuffers(buffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted buffer to be unbound, then releases its tracked memory.
    /// </remarks>
    public override void DeleteBuffer(GlBufferHandle buffer) => base.DeleteBuffer(buffer);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted textures to be unbound, then releases their tracked memory.
    /// </remarks>
    public override void DeleteTextures(ReadOnlySpan<GlTextureHandle> textures) => base.DeleteTextures(textures);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted texture to be unbound, then releases its tracked memory.
    /// </remarks>
    public override void DeleteTexture(GlTextureHandle texture) => base.DeleteTexture(texture);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted vertex arrays to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteVertexArrays(ReadOnlySpan<GlVertexArrayHandle> arrays) => base.DeleteVertexArrays(arrays);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted vertex array to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteVertexArray(GlVertexArrayHandle array) => base.DeleteVertexArray(array);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted framebuffers to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteFramebuffers(ReadOnlySpan<GlFramebufferHandle> framebuffers) => base.DeleteFramebuffers(framebuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted framebuffer to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteFramebuffer(GlFramebufferHandle framebuffer) => base.DeleteFramebuffer(framebuffer);
}
