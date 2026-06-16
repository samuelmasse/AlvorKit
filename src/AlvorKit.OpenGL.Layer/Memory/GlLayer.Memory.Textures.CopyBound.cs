namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CopyTexImage1D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int x,
        int y,
        int width,
        int border)
    {
        TrackBoundTextureSize(nameof(CopyTexImage1D), target, level, new(internalformat, (width, 1, 1), default, default, MipmapDimensions: 1));
        base.CopyTexImage1D(target, level, internalformat, x, y, width, border);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CopyTexImage2D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int x,
        int y,
        int width,
        int height,
        int border)
    {
        TrackBoundTextureSize(
            nameof(CopyTexImage2D),
            target,
            level,
            new(internalformat, (width, height, 1), default, default, MipmapDimensions: MipmapDimensionsFor(target)));
        base.CopyTexImage2D(target, level, internalformat, x, y, width, height, border);
    }
}
