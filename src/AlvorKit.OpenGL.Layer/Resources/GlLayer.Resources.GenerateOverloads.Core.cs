namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated textures, deleted when the layer is disposed.
    /// </remarks>
    public override void GenTextures(Span<GlTextureHandle> textures) => base.GenTextures(textures);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated texture, deleted when the layer is disposed.
    /// </remarks>
    public override GlTextureHandle GenTexture() => base.GenTexture();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated buffers, deleted when the layer is disposed.
    /// </remarks>
    public override void GenBuffers(Span<GlBufferHandle> buffers) => base.GenBuffers(buffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated buffer, deleted when the layer is disposed.
    /// </remarks>
    public override GlBufferHandle GenBuffer() => base.GenBuffer();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated vertex arrays, deleted when the layer is disposed.
    /// </remarks>
    public override void GenVertexArrays(Span<GlVertexArrayHandle> arrays) => base.GenVertexArrays(arrays);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated vertex array, deleted when the layer is disposed.
    /// </remarks>
    public override GlVertexArrayHandle GenVertexArray() => base.GenVertexArray();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated framebuffers, deleted when the layer is disposed.
    /// </remarks>
    public override void GenFramebuffers(Span<GlFramebufferHandle> framebuffers) => base.GenFramebuffers(framebuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated framebuffer, deleted when the layer is disposed.
    /// </remarks>
    public override GlFramebufferHandle GenFramebuffer() => base.GenFramebuffer();
}
