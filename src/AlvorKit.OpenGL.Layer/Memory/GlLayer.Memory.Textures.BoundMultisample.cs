namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage2DMultisample(
        GlTextureTarget target,
        int samples,
        GlInternalFormat internalformat,
        int width,
        int height,
        bool fixedsamplelocations)
    {
        TrackBoundTextureSize(
            nameof(TexImage2DMultisample),
            target,
            0,
            new(internalformat, (width, height, 1), default, default, samples, MipmapDimensions: 2));
        base.TexImage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage3DMultisample(
        GlTextureTarget target,
        int samples,
        GlInternalFormat internalformat,
        int width,
        int height,
        int depth,
        bool fixedsamplelocations)
    {
        TrackBoundTextureSize(
            nameof(TexImage3DMultisample),
            target,
            0,
            new(internalformat, (width, height, depth), default, default, samples, MipmapDimensions: MipmapDimensionsFor(target)));
        base.TexImage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations);
    }
}
