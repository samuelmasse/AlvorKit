namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created textures, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateTextures(GlTextureTarget target, Span<GlTextureHandle> textures) => base.CreateTextures(target, textures);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created texture, deleted when the layer is disposed.
    /// </remarks>
    public override GlTextureHandle CreateTexture(GlTextureTarget target) => base.CreateTexture(target);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created buffers, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateBuffers(Span<GlBufferHandle> buffers) => base.CreateBuffers(buffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created buffer, deleted when the layer is disposed.
    /// </remarks>
    public override GlBufferHandle CreateBuffer() => base.CreateBuffer();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created vertex arrays, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateVertexArrays(Span<GlVertexArrayHandle> arrays) => base.CreateVertexArrays(arrays);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created vertex array, deleted when the layer is disposed.
    /// </remarks>
    public override GlVertexArrayHandle CreateVertexArray() => base.CreateVertexArray();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created framebuffers, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateFramebuffers(Span<GlFramebufferHandle> framebuffers) => base.CreateFramebuffers(framebuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created framebuffer, deleted when the layer is disposed.
    /// </remarks>
    public override GlFramebufferHandle CreateFramebuffer() => base.CreateFramebuffer();
}
