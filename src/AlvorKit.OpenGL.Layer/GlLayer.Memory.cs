namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    private readonly Dictionary<uint, long> bufferSizes = [];
    private long bufferUsage;
    private readonly Dictionary<uint, GlTextureInfo> textureSizes = [];
    private long textureUsage;
    private readonly Dictionary<uint, GlRenderbufferInfo> renderbufferSizes = [];
    private long renderbufferUsage;

    /// <summary>Layer: total bytes of buffer storage allocated and not yet deleted.</summary>
    public long BufferUsage => bufferUsage;
    /// <summary>Layer: total bytes of texture storage allocated and not yet deleted.</summary>
    public long TextureUsage => textureUsage;
    /// <summary>Layer: total bytes of renderbuffer storage allocated and not yet deleted.</summary>
    public long RenderbufferUsage => renderbufferUsage;
    /// <summary>Layer: the last recorded byte size of each live buffer.</summary>
    public IReadOnlyDictionary<uint, long> BufferSizes => bufferSizes;
    /// <summary>Layer: the last recorded shape of each live texture.</summary>
    public IReadOnlyDictionary<uint, GlTextureInfo> TextureSizes => textureSizes;
    /// <summary>Layer: the last recorded shape of each live renderbuffer.</summary>
    public IReadOnlyDictionary<uint, GlRenderbufferInfo> RenderbufferSizes => renderbufferSizes;

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.</remarks>
    public override void BufferData(BufferTarget target, nint size, nint data, BufferUsage usage) { TrackBoundBufferSize(nameof(BufferData), target, (long)size); base.BufferData(target, size, data, usage); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the buffer bound to <paramref name="target"/>.</remarks>
    public override void BufferStorage(BufferStorageTarget target, nint size, nint data, BufferStorageMask flags) { TrackBoundBufferSize(nameof(BufferStorage), (BufferTarget)(uint)target, (long)size); base.BufferStorage(target, size, data, flags); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of buffer <paramref name="buffer"/>.</remarks>
    public override void NamedBufferData(uint buffer, nint size, nint data, BufferUsage usage) { TrackBufferSize(nameof(NamedBufferData), buffer, (long)size); base.NamedBufferData(buffer, size, data, usage); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of buffer <paramref name="buffer"/>.</remarks>
    public override void NamedBufferStorage(uint buffer, nint size, nint data, BufferStorageMask flags) { TrackBufferSize(nameof(NamedBufferStorage), buffer, (long)size); base.NamedBufferStorage(buffer, size, data, flags); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage1D(TextureTarget target, int level, InternalFormat internalformat, int width, int border, PixelFormat format, PixelType type, nint pixels) { TrackBoundTextureSize(nameof(TexImage1D), target, new(internalformat, (width, 1, 1), format, type)); base.TexImage1D(target, level, internalformat, width, border, format, type, pixels); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage2D(TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, PixelFormat format, PixelType type, nint pixels) { TrackBoundTextureSize(nameof(TexImage2D), target, new(internalformat, (width, height, 1), format, type)); base.TexImage2D(target, level, internalformat, width, height, border, format, type, pixels); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage3D(TextureTarget target, int level, InternalFormat internalformat, int width, int height, int depth, int border, PixelFormat format, PixelType type, nint pixels) { TrackBoundTextureSize(nameof(TexImage3D), target, new(internalformat, (width, height, depth), format, type)); base.TexImage3D(target, level, internalformat, width, height, depth, border, format, type, pixels); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage2DMultisample(TextureTarget target, int samples, InternalFormat internalformat, int width, int height, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexImage2DMultisample), target, new(internalformat, (width, height, 1), default, default, samples)); base.TexImage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexImage3DMultisample(TextureTarget target, int samples, InternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexImage3DMultisample), target, new(internalformat, (width, height, depth), default, default, samples)); base.TexImage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage1D(TextureTarget target, int level, InternalFormat internalformat, int width, int border, int imageSize, nint data) { TrackBoundTextureSize(nameof(CompressedTexImage1D), target, new(internalformat, (width, 1, 1), default, default)); base.CompressedTexImage1D(target, level, internalformat, width, border, imageSize, data); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage2D(TextureTarget target, int level, InternalFormat internalformat, int width, int height, int border, int imageSize, nint data) { TrackBoundTextureSize(nameof(CompressedTexImage2D), target, new(internalformat, (width, height, 1), default, default)); base.CompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, data); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CompressedTexImage3D(TextureTarget target, int level, InternalFormat internalformat, int width, int height, int depth, int border, int imageSize, nint data) { TrackBoundTextureSize(nameof(CompressedTexImage3D), target, new(internalformat, (width, height, depth), default, default)); base.CompressedTexImage3D(target, level, internalformat, width, height, depth, border, imageSize, data); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage1D(TextureTarget target, int levels, SizedInternalFormat internalformat, int width) { TrackBoundTextureSize(nameof(TexStorage1D), target, new((InternalFormat)(uint)internalformat, (width, 1, 1), default, default)); base.TexStorage1D(target, levels, internalformat, width); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage2D(TextureTarget target, int levels, SizedInternalFormat internalformat, int width, int height) { TrackBoundTextureSize(nameof(TexStorage2D), target, new((InternalFormat)(uint)internalformat, (width, height, 1), default, default)); base.TexStorage2D(target, levels, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage3D(TextureTarget target, int levels, SizedInternalFormat internalformat, int width, int height, int depth) { TrackBoundTextureSize(nameof(TexStorage3D), target, new((InternalFormat)(uint)internalformat, (width, height, depth), default, default)); base.TexStorage3D(target, levels, internalformat, width, height, depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage2DMultisample(TextureTarget target, int samples, SizedInternalFormat internalformat, int width, int height, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexStorage2DMultisample), target, new((InternalFormat)(uint)internalformat, (width, height, 1), default, default, samples)); base.TexStorage2DMultisample(target, samples, internalformat, width, height, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void TexStorage3DMultisample(TextureTarget target, int samples, SizedInternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations) { TrackBoundTextureSize(nameof(TexStorage3DMultisample), target, new((InternalFormat)(uint)internalformat, (width, height, depth), default, default, samples)); base.TexStorage3DMultisample(target, samples, internalformat, width, height, depth, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CopyTexImage1D(TextureTarget target, int level, InternalFormat internalformat, int x, int y, int width, int border) { TrackBoundTextureSize(nameof(CopyTexImage1D), target, new(internalformat, (width, 1, 1), default, default)); base.CopyTexImage1D(target, level, internalformat, x, y, width, border); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the texture bound to <paramref name="target"/> on the active unit.</remarks>
    public override void CopyTexImage2D(TextureTarget target, int level, InternalFormat internalformat, int x, int y, int width, int height, int border) { TrackBoundTextureSize(nameof(CopyTexImage2D), target, new(internalformat, (width, height, 1), default, default)); base.CopyTexImage2D(target, level, internalformat, x, y, width, height, border); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage1D(uint texture, int levels, SizedInternalFormat internalformat, int width) { TrackTextureSize(nameof(TextureStorage1D), texture, new((InternalFormat)(uint)internalformat, (width, 1, 1), default, default)); base.TextureStorage1D(texture, levels, internalformat, width); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage2D(uint texture, int levels, SizedInternalFormat internalformat, int width, int height) { TrackTextureSize(nameof(TextureStorage2D), texture, new((InternalFormat)(uint)internalformat, (width, height, 1), default, default)); base.TextureStorage2D(texture, levels, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage3D(uint texture, int levels, SizedInternalFormat internalformat, int width, int height, int depth) { TrackTextureSize(nameof(TextureStorage3D), texture, new((InternalFormat)(uint)internalformat, (width, height, depth), default, default)); base.TextureStorage3D(texture, levels, internalformat, width, height, depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage2DMultisample(uint texture, int samples, SizedInternalFormat internalformat, int width, int height, bool fixedsamplelocations) { TrackTextureSize(nameof(TextureStorage2DMultisample), texture, new((InternalFormat)(uint)internalformat, (width, height, 1), default, default, samples)); base.TextureStorage2DMultisample(texture, samples, internalformat, width, height, fixedsamplelocations); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of texture <paramref name="texture"/>.</remarks>
    public override void TextureStorage3DMultisample(uint texture, int samples, SizedInternalFormat internalformat, int width, int height, int depth, bool fixedsamplelocations) { TrackTextureSize(nameof(TextureStorage3DMultisample), texture, new((InternalFormat)(uint)internalformat, (width, height, depth), default, default, samples)); base.TextureStorage3DMultisample(texture, samples, internalformat, width, height, depth, fixedsamplelocations); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the renderbuffer bound to <paramref name="target"/>.</remarks>
    public override void RenderbufferStorage(RenderbufferTarget target, InternalFormat internalformat, int width, int height) { TrackBoundRenderbufferSize(nameof(RenderbufferStorage), new(internalformat, width, height, 1)); base.RenderbufferStorage(target, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of the renderbuffer bound to <paramref name="target"/>.</remarks>
    public override void RenderbufferStorageMultisample(RenderbufferTarget target, int samples, InternalFormat internalformat, int width, int height) { TrackBoundRenderbufferSize(nameof(RenderbufferStorageMultisample), new(internalformat, width, height, samples)); base.RenderbufferStorageMultisample(target, samples, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of renderbuffer <paramref name="renderbuffer"/>.</remarks>
    public override void NamedRenderbufferStorage(uint renderbuffer, InternalFormat internalformat, int width, int height) { TrackRenderbufferSize(nameof(NamedRenderbufferStorage), renderbuffer, new(internalformat, width, height, 1)); base.NamedRenderbufferStorage(renderbuffer, internalformat, width, height); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the memory usage of renderbuffer <paramref name="renderbuffer"/>.</remarks>
    public override void NamedRenderbufferStorageMultisample(uint renderbuffer, int samples, InternalFormat internalformat, int width, int height) { TrackRenderbufferSize(nameof(NamedRenderbufferStorageMultisample), renderbuffer, new(internalformat, width, height, samples)); base.NamedRenderbufferStorageMultisample(renderbuffer, samples, internalformat, width, height); }

    private void TrackBoundBufferSize(string function, BufferTarget target, long size)
    {
        if (!bufferBinds.TryGet(target, out var buffer) || buffer == 0)
            throw new GlException(function, $"cannot track buffer size: no buffer is bound to {target}.");
        TrackBufferSize(function, buffer, size);
    }

    private void TrackBufferSize(string function, uint buffer, long size)
    {
        if (!buffers.Contains(buffer))
            throw new GlException(function, $"cannot track buffer size: buffer {buffer} is not tracked.");
        bufferUsage += size - bufferSizes.GetValueOrDefault(buffer);
        bufferSizes[buffer] = size;
    }

    private void TrackBoundTextureSize(string function, TextureTarget target, GlTextureInfo info)
    {
        var unit = GetActiveTextureIndex(function);
        if (!textureBinds.TryGet((unit, target), out var texture) || texture == 0)
            throw new GlException(function, $"cannot track texture size: no texture is bound to {target} on unit {unit}.");
        TrackTextureSize(function, texture, info);
    }

    private void TrackTextureSize(string function, uint texture, GlTextureInfo info)
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
        TrackRenderbufferSize(function, bound, info);
    }

    private void TrackRenderbufferSize(string function, uint id, GlRenderbufferInfo info)
    {
        if (!renderbuffers.Contains(id))
            throw new GlException(function, $"cannot track renderbuffer size: renderbuffer {id} is not tracked.");
        renderbufferUsage += info.MemoryUsage - renderbufferSizes.GetValueOrDefault(id).MemoryUsage;
        renderbufferSizes[id] = info;
    }

    private void ReleaseBufferMemory(uint buffer) { if (bufferSizes.Remove(buffer, out var size)) bufferUsage -= size; }
    private void ReleaseTextureMemory(uint texture) { if (textureSizes.Remove(texture, out var info)) textureUsage -= info.MemoryUsage; }
    private void ReleaseRenderbufferMemory(uint id) { if (renderbufferSizes.Remove(id, out var info)) renderbufferUsage -= info.MemoryUsage; }
}
