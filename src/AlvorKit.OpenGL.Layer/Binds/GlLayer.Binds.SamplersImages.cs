namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindSampler"/> for the same unit.</remarks>
    public override void BindSampler(uint unit, GlSamplerHandle sampler)
    {
        var id = (uint)sampler;
        state.samplerBinds.RequireCanBind(nameof(BindSampler), unit, id);
        base.BindSampler(unit, sampler);
        state.samplerBinds.BindKnownFree(unit, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindImageTexture"/> for the same unit.</remarks>
    public override void BindImageTexture(uint unit, GlTextureHandle texture, int level, bool layered, int layer, GlBufferAccess access, GlInternalFormat format)
    {
        var id = (uint)texture;
        state.imageTextureBinds.RequireCanBind(nameof(BindImageTexture), unit, id);
        base.BindImageTexture(unit, texture, level, layered, layer, access, format);
        state.imageTextureBinds.BindKnownFree(unit, id);
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
            state.samplerBinds.RequireCanBind(nameof(BindSamplers), first + (uint)i, samplers == 0 ? 0u : ids[i]);
        base.BindSamplers(first, count, samplers);
        for (var i = 0; i < count; i++)
            state.samplerBinds.BindKnownFree(first + (uint)i, samplers == 0 ? 0u : ids[i]);
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
            state.imageTextureBinds.RequireCanBind(nameof(BindImageTextures), first + (uint)i, textures == 0 ? 0u : ids[i]);
        base.BindImageTextures(first, count, textures);
        for (var i = 0; i < count; i++)
            state.imageTextureBinds.BindKnownFree(first + (uint)i, textures == 0 ? 0u : ids[i]);
    }

    /// <summary>
    /// Layer: Unbinds the sampler at unit <paramref name="unit"/>.
    /// Must be paired with exactly one earlier call to <c>glBindSampler</c> for the same unit.
    /// </summary>
    public void UnbindSampler(uint unit)
    {
        state.samplerBinds.RequireCanUnbind(nameof(BindSampler), unit);
        base.BindSampler(unit, (GlSamplerHandle)0u);
        state.samplerBinds.UnbindKnownBound(unit);
    }

    /// <summary>
    /// Layer: Unbinds the image texture at unit <paramref name="unit"/>.
    /// Must be paired with exactly one earlier call to <c>glBindImageTexture</c> for the same unit.
    /// </summary>
    public void UnbindImageTexture(uint unit)
    {
        state.imageTextureBinds.RequireCanUnbind(nameof(BindImageTexture), unit);
        base.BindImageTexture(unit, (GlTextureHandle)0u, 0, false, 0, default, default);
        state.imageTextureBinds.UnbindKnownBound(unit);
    }

    /// <summary>
    /// Layer: Unbinds the range of samplers bound by <see cref="BindSamplers(uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindSamplers(uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindSamplers(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            state.samplerBinds.RequireCanUnbind(nameof(BindSamplers), first + (uint)i);
        Span<uint> samplers = stackalloc uint[count];
        samplers.Clear();
        fixed (uint* p = samplers)
            base.BindSamplers(first, count, (nint)p);
        for (var i = 0; i < count; i++)
            state.samplerBinds.UnbindKnownBound(first + (uint)i);
    }

    /// <summary>
    /// Layer: Unbinds the range of image textures bound by <see cref="BindImageTextures(uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindImageTextures(uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindImageTextures(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            state.imageTextureBinds.RequireCanUnbind(nameof(BindImageTextures), first + (uint)i);
        Span<uint> textures = stackalloc uint[count];
        textures.Clear();
        fixed (uint* p = textures)
            base.BindImageTextures(first, count, (nint)p);
        for (var i = 0; i < count; i++)
            state.imageTextureBinds.UnbindKnownBound(first + (uint)i);
    }
}
