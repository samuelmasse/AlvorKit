namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.
    /// </remarks>
    public override void TexImage1D<T>(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        ReadOnlySpan<T> pixels) =>
        base.TexImage1D(target, level, internalformat, width, border, format, type, pixels);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.
    /// </remarks>
    public override void TexImage2D<T>(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        ReadOnlySpan<T> pixels) =>
        base.TexImage2D(target, level, internalformat, width, height, border, format, type, pixels);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.
    /// </remarks>
    public override void TexImage3D<T>(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int depth,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        ReadOnlySpan<T> pixels) =>
        base.TexImage3D(target, level, internalformat, width, height, depth, border, format, type, pixels);
}
