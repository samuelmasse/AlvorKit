namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage1D(GlTextureTarget target, int levels, GlSizedInternalFormat internalformat, int width)
    {
        TrackBoundTextureSize(
            nameof(TexStorage1D),
            target,
            new((GlInternalFormat)(uint)internalformat, (width, 1, 1), default, default, Levels: levels, MipmapDimensions: 1));
        base.TexStorage1D(target, levels, internalformat, width);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage2D(
        GlTextureTarget target,
        int levels,
        GlSizedInternalFormat internalformat,
        int width,
        int height)
    {
        TrackBoundTextureSize(
            nameof(TexStorage2D),
            target,
            new(
                (GlInternalFormat)(uint)internalformat,
                (width, height, DepthForStorageTarget(target)),
                default,
                default,
                Levels: levels,
                MipmapDimensions: MipmapDimensionsFor(target)));
        base.TexStorage2D(target, levels, internalformat, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage3D(
        GlTextureTarget target,
        int levels,
        GlSizedInternalFormat internalformat,
        int width,
        int height,
        int depth)
    {
        TrackBoundTextureSize(
            nameof(TexStorage3D),
            target,
            new(
                (GlInternalFormat)(uint)internalformat,
                (width, height, depth),
                default,
                default,
                Levels: levels,
                MipmapDimensions: MipmapDimensionsFor(target)));
        base.TexStorage3D(target, levels, internalformat, width, height, depth);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage2DMultisample(
        GlTextureTarget target,
        int samples,
        GlSizedInternalFormat internalformat,
        int width,
        int height,
        bool fixedsamplelocations)
    {
        TrackBoundTextureSize(
            nameof(TexStorage2DMultisample),
            target,
            new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default, samples, MipmapDimensions: 2));
        base.TexStorage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage3DMultisample(
        GlTextureTarget target,
        int samples,
        GlSizedInternalFormat internalformat,
        int width,
        int height,
        int depth,
        bool fixedsamplelocations)
    {
        TrackBoundTextureSize(
            nameof(TexStorage3DMultisample),
            target,
            new(
                (GlInternalFormat)(uint)internalformat,
                (width, height, depth),
                default,
                default,
                samples,
                MipmapDimensions: MipmapDimensionsFor(target)));
        base.TexStorage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations);
    }
}
