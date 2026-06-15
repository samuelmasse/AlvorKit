namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="ResetActiveTexture"/>.
    /// Set-once: switching to another unit requires resetting first.
    /// </remarks>
    public override void ActiveTexture(GlTextureUnit texture)
    {
        activeTexture.Set(nameof(ActiveTexture), texture);
        base.ActiveTexture(texture);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Must be paired with exactly one later call to <see cref="UnbindTexture"/> for the same target
    /// on the active unit. A texture cannot be used as more than one target.
    /// </remarks>
    public override void BindTexture(GlTextureTarget target, GlTextureHandle texture)
    {
        var id = (uint)texture;
        if (id != 0)
            TrackTextureTarget(nameof(BindTexture), texture, target);
        textureBinds.Bind(nameof(BindTexture), (GetActiveTextureIndex(nameof(BindTexture)), target), id);
        base.BindTexture(target, texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTextureUnit"/> for the same unit.</remarks>
    public override void BindTextureUnit(uint unit, GlTextureHandle texture)
    {
        var target = GetTextureTarget(nameof(BindTextureUnit), texture);
        textureBinds.Bind(nameof(BindTextureUnit), (unit, target), (uint)texture);
        base.BindTextureUnit(unit, texture);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each texture to units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindTextures"/> for the same range.
    /// </remarks>
    public override void BindTextures(uint first, int count, nint textures)
    {
        var ids = (uint*)textures;
        for (var i = 0; i < count; i++)
        {
            var texture = textures == 0 ? (GlTextureHandle)0u : (GlTextureHandle)ids[i];
            var target = GetTextureTarget(nameof(BindTextures), texture);
            textureBinds.Bind(nameof(BindTextures), (first + (uint)i, target), (uint)texture);
        }
        base.BindTextures(first, count, textures);
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindTexture</c> for <paramref name="target"/> on the active unit.
    /// Must be paired with exactly one earlier call to <c>glBindTexture</c> for the same target.
    /// </summary>
    public void UnbindTexture(GlTextureTarget target)
    {
        textureBinds.Unbind(nameof(BindTexture), (GetActiveTextureIndex(nameof(BindTexture)), target));
        base.BindTexture(target, (GlTextureHandle)0u);
    }

    /// <summary>
    /// Layer: Unbinds the texture at unit <paramref name="unit"/>.
    /// Must be paired with exactly one earlier call to <c>glBindTextureUnit</c> for the same unit.
    /// </summary>
    public void UnbindTextureUnit(uint unit) { ResetTextureUnitBindings(nameof(BindTextureUnit), unit); base.BindTextureUnit(unit, (GlTextureHandle)0u); }

    /// <summary>
    /// Layer: Unbinds the range of textures bound by <see cref="BindTextures"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindTextures"/> for the same range.
    /// </summary>
    public void UnbindTextures(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            ResetTextureUnitBindings(nameof(BindTextures), first + (uint)i);
        uint* textures = stackalloc uint[count];
        base.BindTextures(first, count, (nint)textures);
    }

    /// <summary>
    /// Layer: Restores <c>glActiveTexture</c> to <see cref="DefaultActiveTexture"/>.
    /// Must be paired with exactly one earlier call to <c>glActiveTexture</c>.
    /// </summary>
    public void ResetActiveTexture()
    {
        activeTexture.Reset(nameof(ActiveTexture));
        base.ActiveTexture(DefaultActiveTexture);
    }

    /// <summary>
    /// Records the single target family associated with a texture handle.
    /// </summary>
    /// <param name="function">The GL function that observed the target.</param>
    /// <param name="texture">The texture handle to associate.</param>
    /// <param name="target">The texture target family used by the handle.</param>
    private void TrackTextureTarget(string function, GlTextureHandle texture, GlTextureTarget target)
    {
        if ((uint)texture == 0)
            return;
        if (textureTargets.TryGetValue(texture, out var existing) && existing != target)
            throw new GlBindConflictException(function, $"texture {texture} is already used as {existing}, cannot use it as {target}.");
        textureTargets[texture] = target;
    }

    /// <summary>
    /// Gets the known texture target for strict direct-state texture binding calls.
    /// </summary>
    /// <param name="function">The GL function that needs the texture target.</param>
    /// <param name="texture">The texture handle to inspect.</param>
    /// <returns>The target family associated with the texture handle.</returns>
    private GlTextureTarget GetTextureTarget(string function, GlTextureHandle texture)
    {
        if ((uint)texture == 0)
            throw new GlException(function, "cannot bind texture 0 through strict bind APIs; use the matching Unbind* method.");
        if (textureTargets.TryGetValue(texture, out var target))
            return target;
        throw new GlException(function, $"texture {texture} has no known target; bind it with glBindTexture or create it with glCreateTextures first.");
    }

    /// <summary>
    /// Clears every texture target binding for a texture unit.
    /// </summary>
    /// <param name="function">The GL function that requested the unbind.</param>
    /// <param name="unit">The texture unit whose target bindings should be cleared.</param>
    private void ResetTextureUnitBindings(string function, uint unit) =>
        textureBinds.UnbindWhere(function, key => key.Item1 == unit);
}
