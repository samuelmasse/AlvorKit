namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage1D(
        GlTextureHandle texture,
        int levels,
        GlSizedInternalFormat internalformat,
        int width)
    {
        TrackTextureSize(
            nameof(TextureStorage1D),
            texture,
            new((GlInternalFormat)(uint)internalformat, (width, 1, 1), default, default, Levels: levels, MipmapDimensions: 1));
        base.TextureStorage1D(texture, levels, internalformat, width);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage2D(
        GlTextureHandle texture,
        int levels,
        GlSizedInternalFormat internalformat,
        int width,
        int height)
    {
        TrackTextureSize(
            nameof(TextureStorage2D),
            texture,
            new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default, Levels: levels, MipmapDimensions: 2));
        base.TextureStorage2D(texture, levels, internalformat, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage3D(
        GlTextureHandle texture,
        int levels,
        GlSizedInternalFormat internalformat,
        int width,
        int height,
        int depth)
    {
        TrackTextureSize(
            nameof(TextureStorage3D),
            texture,
            new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default, Levels: levels, MipmapDimensions: 3));
        base.TextureStorage3D(texture, levels, internalformat, width, height, depth);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage2DMultisample(
        GlTextureHandle texture,
        int samples,
        GlSizedInternalFormat internalformat,
        int width,
        int height,
        bool fixedsamplelocations)
    {
        TrackTextureSize(
            nameof(TextureStorage2DMultisample),
            texture,
            new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default, samples, MipmapDimensions: 2));
        base.TextureStorage2DMultisample(texture, samples, internalformat, width, height, fixedsamplelocations);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage3DMultisample(
        GlTextureHandle texture,
        int samples,
        GlSizedInternalFormat internalformat,
        int width,
        int height,
        int depth,
        bool fixedsamplelocations)
    {
        TrackTextureSize(
            nameof(TextureStorage3DMultisample),
            texture,
            new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default, samples, MipmapDimensions: 3));
        base.TextureStorage3DMultisample(texture, samples, internalformat, width, height, depth, fixedsamplelocations);
    }
}
