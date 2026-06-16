namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindSampler"/> for the same unit.</remarks>
    public override void BindSampler(uint unit, GlSamplerHandle sampler)
    {
        samplerBinds.Bind(nameof(BindSampler), unit, (uint)sampler);
        base.BindSampler(unit, sampler);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindImageTexture"/> for the same unit.</remarks>
    public override void BindImageTexture(uint unit, GlTextureHandle texture, int level, bool layered, int layer, GlBufferAccess access, GlInternalFormat format)
    {
        imageTextureBinds.Bind(nameof(BindImageTexture), unit, (uint)texture);
        base.BindImageTexture(unit, texture, level, layered, layer, access, format);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each sampler to texture units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindSamplers"/> for the same range.
    /// </remarks>
    public override void BindSamplers(uint first, int count, nint samplers)
    {
        var ids = (uint*)samplers;
        for (var i = 0; i < count; i++)
            samplerBinds.Bind(nameof(BindSamplers), first + (uint)i, samplers == 0 ? 0u : ids[i]);
        base.BindSamplers(first, count, samplers);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each texture to image units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindImageTextures"/> for the same range.
    /// </remarks>
    public override void BindImageTextures(uint first, int count, nint textures)
    {
        var ids = (uint*)textures;
        for (var i = 0; i < count; i++)
            imageTextureBinds.Bind(nameof(BindImageTextures), first + (uint)i, textures == 0 ? 0u : ids[i]);
        base.BindImageTextures(first, count, textures);
    }

    /// <summary>
    /// Layer: Unbinds the sampler at unit <paramref name="unit"/>.
    /// Must be paired with exactly one earlier call to <c>glBindSampler</c> for the same unit.
    /// </summary>
    public void UnbindSampler(uint unit) { samplerBinds.Unbind(nameof(BindSampler), unit); base.BindSampler(unit, (GlSamplerHandle)0u); }

    /// <summary>
    /// Layer: Unbinds the image texture at unit <paramref name="unit"/>.
    /// Must be paired with exactly one earlier call to <c>glBindImageTexture</c> for the same unit.
    /// </summary>
    public void UnbindImageTexture(uint unit)
    {
        imageTextureBinds.Unbind(nameof(BindImageTexture), unit);
        base.BindImageTexture(unit, (GlTextureHandle)0u, 0, false, 0, default, default);
    }

    /// <summary>
    /// Layer: Unbinds the range of samplers bound by <see cref="BindSamplers(uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindSamplers(uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindSamplers(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            samplerBinds.Unbind(nameof(BindSamplers), first + (uint)i);
        uint* samplers = stackalloc uint[count];
        base.BindSamplers(first, count, (nint)samplers);
    }

    /// <summary>
    /// Layer: Unbinds the range of image textures bound by <see cref="BindImageTextures(uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindImageTextures(uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindImageTextures(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            imageTextureBinds.Unbind(nameof(BindImageTextures), first + (uint)i);
        uint* textures = stackalloc uint[count];
        base.BindImageTextures(first, count, (nint)textures);
    }
}
