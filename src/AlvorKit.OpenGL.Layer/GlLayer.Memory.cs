namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    private readonly Dictionary<GlBufferHandle, long> bufferSizes = [];
    private long bufferUsage;
    private readonly Dictionary<GlTextureHandle, GlTextureInfo> textureSizes = [];
    private long textureUsage;
    private readonly Dictionary<GlRenderbufferHandle, GlRenderbufferInfo> renderbufferSizes = [];
    private long renderbufferUsage;

    /// <summary>Layer: total bytes of buffer storage allocated and not yet deleted.</summary>
    public long BufferUsage => bufferUsage;
    /// <summary>Layer: total bytes of texture storage allocated and not yet deleted.</summary>
    public long TextureUsage => textureUsage;
    /// <summary>Layer: total bytes of renderbuffer storage allocated and not yet deleted.</summary>
    public long RenderbufferUsage => renderbufferUsage;
    /// <summary>Layer: the last recorded byte size of each live buffer.</summary>
    public IReadOnlyDictionary<GlBufferHandle, long> BufferSizes => bufferSizes;
    /// <summary>Layer: the last recorded shape of each live texture.</summary>
    public IReadOnlyDictionary<GlTextureHandle, GlTextureInfo> TextureSizes => textureSizes;
    /// <summary>Layer: the last recorded shape of each live renderbuffer.</summary>
    public IReadOnlyDictionary<GlRenderbufferHandle, GlRenderbufferInfo> RenderbufferSizes => renderbufferSizes;

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.</remarks>
    public override void BufferData(GlBufferTarget target, nint size, nint data, GlBufferUsage usage) { TrackBoundBufferSize(nameof(BufferData), target, (long)size); base.BufferData(target, size, data, usage); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.</remarks>
    public override void BufferStorage(GlBufferStorageTarget target, nint size, nint data, GlBufferStorageMask flags) { TrackBoundBufferSize(nameof(BufferStorage), (GlBufferTarget)(uint)target, (long)size); base.BufferStorage(target, size, data, flags); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of buffer <paramref name="buffer"/>.</remarks>
    public override void NamedBufferData(GlBufferHandle buffer, nint size, nint data, GlBufferUsage usage) { TrackBufferSize(nameof(NamedBufferData), buffer, (long)size); base.NamedBufferData(buffer, size, data, usage); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of buffer <paramref name="buffer"/>.</remarks>
    public override void NamedBufferStorage(GlBufferHandle buffer, nint size, nint data, GlBufferStorageMask flags) { TrackBufferSize(nameof(NamedBufferStorage), buffer, (long)size); base.NamedBufferStorage(buffer, size, data, flags); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage1D(GlTextureTarget target, int level, GlInternalFormat internalformat, int width, int border, GlPixelFormat format, GlPixelType type, nint pixels) { TrackBoundTextureSize(nameof(TexImage1D), target, new(internalformat, (width, 1, 1), format, type)); base.TexImage1D(target, level, internalformat, width, border, format, type, pixels); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage2D(GlTextureTarget target, int level, GlInternalFormat internalformat, int width, int height, int border, GlPixelFormat format, GlPixelType type, nint pixels) { TrackBoundTextureSize(nameof(TexImage2D), target, new(internalformat, (width, height, 1), format, type)); base.TexImage2D(target, level, internalformat, width, height, border, format, type, pixels); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage3D(GlTextureTarget target, int level, GlInternalFormat internalformat, int width, int height, int depth, int border, GlPixelFormat format, GlPixelType type, nint pixels) { TrackBoundTextureSize(nameof(TexImage3D), target, new(internalformat, (width, height, depth), format, type)); base.TexImage3D(target, level, internalformat, width, height, depth, border, format, type, pixels); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage2DMultisample(GlTextureTarget target, int samples, GlInternalFormat internalformat, int width, int height, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexImage2DMultisample), target, new(internalformat, (width, height, 1), default, default, samples)); base.TexImage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage3DMultisample(GlTextureTarget target, int samples, GlInternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexImage3DMultisample), target, new(internalformat, (width, height, depth), default, default, samples)); base.TexImage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage1D(GlTextureTarget target, int level, GlInternalFormat internalformat, int width, int border, int imageSize, nint data) { TrackBoundTextureSize(nameof(CompressedTexImage1D), target, new(internalformat, (width, 1, 1), default, default)); base.CompressedTexImage1D(target, level, internalformat, width, border, imageSize, data); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage2D(GlTextureTarget target, int level, GlInternalFormat internalformat, int width, int height, int border, int imageSize, nint data) { TrackBoundTextureSize(nameof(CompressedTexImage2D), target, new(internalformat, (width, height, 1), default, default)); base.CompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage3D(GlTextureTarget target, int level, GlInternalFormat internalformat, int width, int height, int depth, int border, int imageSize, nint data) { TrackBoundTextureSize(nameof(CompressedTexImage3D), target, new(internalformat, (width, height, depth), default, default)); base.CompressedTexImage3D(target, level, internalformat, width, height, depth, border, imageSize, data); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage1D(GlTextureTarget target, int levels, GlSizedInternalFormat internalformat, int width) { TrackBoundTextureSize(nameof(TexStorage1D), target, new((GlInternalFormat)(uint)internalformat, (width, 1, 1), default, default)); base.TexStorage1D(target, levels, internalformat, width); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage2D(GlTextureTarget target, int levels, GlSizedInternalFormat internalformat, int width, int height) { TrackBoundTextureSize(nameof(TexStorage2D), target, new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default)); base.TexStorage2D(target, levels, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage3D(GlTextureTarget target, int levels, GlSizedInternalFormat internalformat, int width, int height, int depth) { TrackBoundTextureSize(nameof(TexStorage3D), target, new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default)); base.TexStorage3D(target, levels, internalformat, width, height, depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage2DMultisample(GlTextureTarget target, int samples, GlSizedInternalFormat internalformat, int width, int height, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexStorage2DMultisample), target, new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default, samples)); base.TexStorage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage3DMultisample(GlTextureTarget target, int samples, GlSizedInternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexStorage3DMultisample), target, new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default, samples)); base.TexStorage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CopyTexImage1D(GlTextureTarget target, int level, GlInternalFormat internalformat, int x, int y, int width, int border) { TrackBoundTextureSize(nameof(CopyTexImage1D), target, new(internalformat, (width, 1, 1), default, default)); base.CopyTexImage1D(target, level, internalformat, x, y, width, border); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CopyTexImage2D(GlTextureTarget target, int level, GlInternalFormat internalformat, int x, int y, int width, int height, int border) { TrackBoundTextureSize(nameof(CopyTexImage2D), target, new(internalformat, (width, height, 1), default, default)); base.CopyTexImage2D(target, level, internalformat, x, y, width, height, border); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage1D(GlTextureHandle texture, int levels, GlSizedInternalFormat internalformat, int width) { TrackTextureSize(nameof(TextureStorage1D), texture, new((GlInternalFormat)(uint)internalformat, (width, 1, 1), default, default)); base.TextureStorage1D(texture, levels, internalformat, width); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage2D(GlTextureHandle texture, int levels, GlSizedInternalFormat internalformat, int width, int height) { TrackTextureSize(nameof(TextureStorage2D), texture, new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default)); base.TextureStorage2D(texture, levels, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage3D(GlTextureHandle texture, int levels, GlSizedInternalFormat internalformat, int width, int height, int depth) { TrackTextureSize(nameof(TextureStorage3D), texture, new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default)); base.TextureStorage3D(texture, levels, internalformat, width, height, depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage2DMultisample(GlTextureHandle texture, int samples, GlSizedInternalFormat internalformat, int width, int height, bool fixedsamplelocations) { TrackTextureSize(nameof(TextureStorage2DMultisample), texture, new((GlInternalFormat)(uint)internalformat, (width, height, 1), default, default, samples)); base.TextureStorage2DMultisample(texture, samples, internalformat, width, height, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage3DMultisample(GlTextureHandle texture, int samples, GlSizedInternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations) { TrackTextureSize(nameof(TextureStorage3DMultisample), texture, new((GlInternalFormat)(uint)internalformat, (width, height, depth), default, default, samples)); base.TextureStorage3DMultisample(texture, samples, internalformat, width, height, depth, fixedsamplelocations); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the renderbuffer bound to <paramref name="target"/>.</remarks>
    public override void RenderbufferStorage(GlRenderbufferTarget target, GlInternalFormat internalformat, int width, int height) { TrackBoundRenderbufferSize(nameof(RenderbufferStorage), new(internalformat, width, height, 1)); base.RenderbufferStorage(target, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the renderbuffer bound to <paramref name="target"/>.</remarks>
    public override void RenderbufferStorageMultisample(GlRenderbufferTarget target, int samples, GlInternalFormat internalformat, int width, int height) { TrackBoundRenderbufferSize(nameof(RenderbufferStorageMultisample), new(internalformat, width, height, samples)); base.RenderbufferStorageMultisample(target, samples, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of renderbuffer <paramref name="renderbuffer"/>.</remarks>
    public override void NamedRenderbufferStorage(GlRenderbufferHandle renderbuffer, GlInternalFormat internalformat, int width, int height) { TrackRenderbufferSize(nameof(NamedRenderbufferStorage), renderbuffer, new(internalformat, width, height, 1)); base.NamedRenderbufferStorage(renderbuffer, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of renderbuffer <paramref name="renderbuffer"/>.</remarks>
    public override void NamedRenderbufferStorageMultisample(GlRenderbufferHandle renderbuffer, int samples, GlInternalFormat internalformat, int width, int height) { TrackRenderbufferSize(nameof(NamedRenderbufferStorageMultisample), renderbuffer, new(internalformat, width, height, samples)); base.NamedRenderbufferStorageMultisample(renderbuffer, samples, internalformat, width, height); }

    private void TrackBoundBufferSize(string function, GlBufferTarget target, long size)
    {
        if (!bufferBinds.TryGet(target, out var buffer) || buffer == 0)
            throw new GlException(function, $"cannot track buffer size: no buffer is bound to {target}.");
        TrackBufferSize(function, (GlBufferHandle)buffer, size);
    }

    private void TrackBufferSize(string function, GlBufferHandle buffer, long size)
    {
        if (!buffers.Contains(buffer))
            throw new GlException(function, $"cannot track buffer size: buffer {buffer} is not tracked.");
        bufferUsage += size - bufferSizes.GetValueOrDefault(buffer);
        bufferSizes[buffer] = size;
    }

    private void TrackBoundTextureSize(string function, GlTextureTarget target, GlTextureInfo info)
    {
        var unit = GetActiveTextureIndex(function);
        if (!textureBinds.TryGet((unit, target), out var texture) || texture == 0)
            throw new GlException(function, $"cannot track texture size: no texture is bound to {target} on unit {unit}.");
        TrackTextureSize(function, (GlTextureHandle)texture, info);
    }

    private void TrackTextureSize(string function, GlTextureHandle texture, GlTextureInfo info)
    {
        if (!textures.Contains(texture))
            throw new GlException(function, $"cannot track texture size: texture {texture} is not tracked.");
        textureUsage += info.MemoryUsage - textureSizes.GetValueOrDefault(texture).MemoryUsage;
        textureSizes[texture] = info;
    }

    private void TrackBoundRenderbufferSize(string function, GlRenderbufferInfo info)
    {
        var bound = renderbuffer.Current;
        if (bound == 0)
            throw new GlException(function, "cannot track renderbuffer size: no renderbuffer is bound.");
        TrackRenderbufferSize(function, (GlRenderbufferHandle)bound, info);
    }

    private void TrackRenderbufferSize(string function, GlRenderbufferHandle id, GlRenderbufferInfo info)
    {
        if (!renderbuffers.Contains(id))
            throw new GlException(function, $"cannot track renderbuffer size: renderbuffer {id} is not tracked.");
        renderbufferUsage += info.MemoryUsage - renderbufferSizes.GetValueOrDefault(id).MemoryUsage;
        renderbufferSizes[id] = info;
    }

    private void ReleaseBufferMemory(GlBufferHandle buffer) { if (bufferSizes.Remove(buffer, out var size)) bufferUsage -= size; }
    private void ReleaseTextureMemory(GlTextureHandle texture) { if (textureSizes.Remove(texture, out var info)) textureUsage -= info.MemoryUsage; }
    private void ReleaseRenderbufferMemory(GlRenderbufferHandle id) { if (renderbufferSizes.Remove(id, out var info)) renderbufferUsage -= info.MemoryUsage; }
}
