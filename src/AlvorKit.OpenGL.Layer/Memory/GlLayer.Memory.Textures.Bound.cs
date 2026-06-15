namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage1D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        nint pixels)
    {
        TrackBoundTextureSize(nameof(TexImage1D), target, new(internalformat, (width, 1, 1), format, type));
        base.TexImage1D(target, level, internalformat, width, border, format, type, pixels);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage2D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        nint pixels)
    {
        TrackBoundTextureSize(nameof(TexImage2D), target, new(internalformat, (width, height, 1), format, type));
        base.TexImage2D(target, level, internalformat, width, height, border, format, type, pixels);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage3D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int depth,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        nint pixels)
    {
        TrackBoundTextureSize(nameof(TexImage3D), target, new(internalformat, (width, height, depth), format, type));
        base.TexImage3D(target, level, internalformat, width, height, depth, border, format, type, pixels);
    }
}
