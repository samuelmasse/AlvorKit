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
            new((GlInternalFormat)(uint)internalformat, (width, 1, 1), default, default));
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
            new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default));
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
            new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default));
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
            new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default, samples));
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
            new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default, samples));
        base.TextureStorage3DMultisample(texture, samples, internalformat, width, height, depth, fixedsamplelocations);
    }

    /// <summary>
    /// Tracks storage bytes for the texture currently bound to a target on the active texture unit.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="target">The texture target whose bound texture receives the shape.</param>
    /// <param name="info">The texture shape used for accounting.</param>
    private void TrackBoundTextureSize(string function, GlTextureTarget target, GlTextureInfo info)
    {
        var unit = GetActiveTextureIndex(function);
        if (!textureBinds.TryGet((unit, target), out var texture) || texture == 0)
            throw new GlException(function, $"cannot track texture size: no texture is bound to {target} on unit {unit}.");
        TrackTextureSize(function, (GlTextureHandle)texture, info);
    }

    /// <summary>
    /// Tracks storage bytes for a specific live texture handle.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="texture">The texture handle receiving the shape.</param>
    /// <param name="info">The texture shape used for accounting.</param>
    private void TrackTextureSize(string function, GlTextureHandle texture, GlTextureInfo info)
    {
        if (!textures.Contains(texture))
            throw new GlException(function, $"cannot track texture size: texture {texture} is not tracked.");
        textureUsage += info.MemoryUsage - textureSizes.GetValueOrDefault(texture).MemoryUsage;
        textureSizes[texture] = info;
    }

    /// <summary>
    /// Releases any tracked storage bytes for a texture that is being deleted.
    /// </summary>
    /// <param name="texture">The texture handle whose memory accounting should be released.</param>
    private void ReleaseTextureMemory(GlTextureHandle texture)
    {
        if (textureSizes.Remove(texture, out var info))
            textureUsage -= info.MemoryUsage;
    }
}
