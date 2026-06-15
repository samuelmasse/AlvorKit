namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage1D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int border,
        int imageSize,
        nint data)
    {
        TrackBoundTextureSize(nameof(CompressedTexImage1D), target, new(internalformat, (width, 1, 1), default, default));
        base.CompressedTexImage1D(target, level, internalformat, width, border, imageSize, data);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage2D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int border,
        int imageSize,
        nint data)
    {
        TrackBoundTextureSize(nameof(CompressedTexImage2D), target, new(internalformat, (width, height, 1), default, default));
        base.CompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage3D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int depth,
        int border,
        int imageSize,
        nint data)
    {
        TrackBoundTextureSize(nameof(CompressedTexImage3D), target, new(internalformat, (width, height, depth), default, default));
        base.CompressedTexImage3D(target, level, internalformat, width, height, depth, border, imageSize, data);
    }
}
