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
        activeTexture.RequireCanSet(nameof(ActiveTexture), texture);
        base.ActiveTexture(texture);
        activeTexture.SetKnownUnset(texture);
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
            RequireTextureTargetCompatible(nameof(BindTexture), texture, target);
        var key = (GetActiveTextureIndex(nameof(BindTexture)), target);
        textureBinds.RequireCanBind(nameof(BindTexture), key, id);
        base.BindTexture(target, texture);
        if (id != 0)
            CommitTextureTarget(texture, target);
        textureBinds.BindKnownFree(key, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTextureUnit"/> for the same unit.</remarks>
    public override void BindTextureUnit(uint unit, GlTextureHandle texture)
    {
        var target = GetTextureTarget(nameof(BindTextureUnit), texture);
        textureBinds.RequireCanBind(nameof(BindTextureUnit), (unit, target), (uint)texture);
        base.BindTextureUnit(unit, texture);
        textureBinds.BindKnownFree((unit, target), (uint)texture);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: Binds each texture to units <c>[first, first + count)</c>.
    /// Must be paired with exactly one later call to <see cref="UnbindTextures"/> for the same range.
    /// </remarks>
    public override void BindTextures(uint first, int count, nint textures)
    {
        var ids = (uint*)textures;
        Span<GlTextureTarget> targets = stackalloc GlTextureTarget[count];
        for (var i = 0; i < count; i++)
        {
            var texture = textures == 0 ? (GlTextureHandle)0u : (GlTextureHandle)ids[i];
            targets[i] = GetTextureTarget(nameof(BindTextures), texture);
            textureBinds.RequireCanBind(nameof(BindTextures), (first + (uint)i, targets[i]), (uint)texture);
        }
        base.BindTextures(first, count, textures);
        for (var i = 0; i < count; i++)
        {
            var texture = textures == 0 ? (GlTextureHandle)0u : (GlTextureHandle)ids[i];
            textureBinds.BindKnownFree((first + (uint)i, targets[i]), (uint)texture);
        }
    }

    /// <summary>
    /// Layer: Unbinds <c>glBindTexture</c> for <paramref name="target"/> on the active unit.
    /// Must be paired with exactly one earlier call to <c>glBindTexture</c> for the same target.
    /// </summary>
    public void UnbindTexture(GlTextureTarget target)
    {
        var key = (GetActiveTextureIndex(nameof(BindTexture)), target);
        textureBinds.RequireCanUnbind(nameof(BindTexture), key);
        base.BindTexture(target, (GlTextureHandle)0u);
        textureBinds.UnbindKnownBound(key);
    }

    /// <summary>
    /// Layer: Unbinds the texture at unit <paramref name="unit"/>.
    /// Must be paired with exactly one earlier call to <c>glBindTextureUnit</c> for the same unit.
    /// </summary>
    public void UnbindTextureUnit(uint unit)
    {
        RequireAnyTextureUnitBinding(nameof(BindTextureUnit), unit);
        base.BindTextureUnit(unit, (GlTextureHandle)0u);
        ResetTextureUnitBindingsKnownBound(unit);
    }

    /// <summary>
    /// Layer: Unbinds the range of textures bound by <see cref="BindTextures(uint, int, nint)"/>.
    /// Must be paired with exactly one earlier call to <see cref="BindTextures(uint, int, nint)"/> for the same range.
    /// </summary>
    public void UnbindTextures(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            RequireAnyTextureUnitBinding(nameof(BindTextures), first + (uint)i);
        Span<uint> textures = stackalloc uint[count];
        textures.Clear();
        fixed (uint* p = textures)
            base.BindTextures(first, count, (nint)p);
        for (var i = 0; i < count; i++)
            ResetTextureUnitBindingsKnownBound(first + (uint)i);
    }

    /// <summary>
    /// Layer: Restores <c>glActiveTexture</c> to <see cref="DefaultActiveTexture"/>.
    /// Must be paired with exactly one earlier call to <c>glActiveTexture</c>.
    /// </summary>
    public void ResetActiveTexture()
    {
        activeTexture.RequireCanReset(nameof(ActiveTexture));
        base.ActiveTexture(DefaultActiveTexture);
        activeTexture.ResetKnownSet();
    }

    /// <summary>
    /// Records the single target family associated with a texture handle.
    /// </summary>
    /// <param name="function">The GL function that observed the target.</param>
    /// <param name="texture">The texture handle to associate.</param>
    /// <param name="target">The texture target family used by the handle.</param>
    private void TrackTextureTarget(string function, GlTextureHandle texture, GlTextureTarget target)
    {
        RequireTextureTargetCompatible(function, texture, target);
        CommitTextureTarget(texture, target);
    }

    /// <summary>
    /// Ensures a texture handle can be associated with the requested target family.
    /// </summary>
    /// <param name="function">The GL function that observed the target.</param>
    /// <param name="texture">The texture handle to associate.</param>
    /// <param name="target">The texture target family used by the handle.</param>
    private void RequireTextureTargetCompatible(string function, GlTextureHandle texture, GlTextureTarget target)
    {
        if ((uint)texture == 0)
            return;
        if (textureTargets.TryGetValue(texture, out var existing) && existing != target)
            throw new GlBindConflictException(function, $"texture {texture} is already used as {existing}, cannot use it as {target}.");
    }

    /// <summary>
    /// Records a validated texture target association.
    /// </summary>
    /// <param name="texture">The texture handle to associate.</param>
    /// <param name="target">The texture target family used by the handle.</param>
    private void CommitTextureTarget(GlTextureHandle texture, GlTextureTarget target)
    {
        if ((uint)texture == 0)
            return;
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

}
