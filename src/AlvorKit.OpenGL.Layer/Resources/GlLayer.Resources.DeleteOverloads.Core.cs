namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteBuffers(int, nint)"/>, pinning <paramref name="buffers"/> for the call.
    /// Layer: requires the deleted buffers to be unbound, then releases their tracked memory.
    /// </remarks>
    public override void DeleteBuffers(ReadOnlySpan<GlBufferHandle> buffers) => base.DeleteBuffers(buffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteBuffers(int, nint)"/> with one stack-addressed buffer.
    /// Layer: requires the deleted buffer to be unbound, then releases its tracked memory.
    /// </remarks>
    public override void DeleteBuffer(GlBufferHandle buffer) => base.DeleteBuffer(buffer);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteTextures(int, nint)"/>, pinning <paramref name="textures"/> for the call.
    /// Layer: requires the deleted textures to be unbound, then releases their tracked memory.
    /// </remarks>
    public override void DeleteTextures(ReadOnlySpan<GlTextureHandle> textures) => base.DeleteTextures(textures);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteTextures(int, nint)"/> with one stack-addressed texture.
    /// Layer: requires the deleted texture to be unbound, then releases its tracked memory.
    /// </remarks>
    public override void DeleteTexture(GlTextureHandle texture) => base.DeleteTexture(texture);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteVertexArrays(int, nint)"/>, pinning <paramref name="arrays"/> for the call.
    /// Layer: requires the deleted vertex arrays to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteVertexArrays(ReadOnlySpan<GlVertexArrayHandle> arrays) => base.DeleteVertexArrays(arrays);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteVertexArrays(int, nint)"/> with one stack-addressed vertex array.
    /// Layer: requires the deleted vertex array to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteVertexArray(GlVertexArrayHandle array) => base.DeleteVertexArray(array);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteFramebuffers(int, nint)"/>, pinning <paramref name="framebuffers"/> for the call.
    /// Layer: requires the deleted framebuffers to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteFramebuffers(ReadOnlySpan<GlFramebufferHandle> framebuffers) => base.DeleteFramebuffers(framebuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteFramebuffers(int, nint)"/> with one stack-addressed framebuffer.
    /// Layer: requires the deleted framebuffer to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteFramebuffer(GlFramebufferHandle framebuffer) => base.DeleteFramebuffer(framebuffer);
}
