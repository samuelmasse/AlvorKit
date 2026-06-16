namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Tracks one level for the texture currently bound to a target on the active texture unit.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="target">The texture target whose bound texture receives the shape.</param>
    /// <param name="level">The mip level receiving the shape.</param>
    /// <param name="info">The texture level shape used for accounting.</param>
    private void TrackBoundTextureSize(string function, GlTextureTarget target, int level, GlTextureInfo info)
    {
        var unit = GetActiveTextureIndex(function);
        if (!textureBinds.TryGet((unit, target), out var texture) || texture == 0)
            throw new GlException(function, $"cannot track texture size: no texture is bound to {target} on unit {unit}.");
        TrackTextureLevelSize(function, (GlTextureHandle)texture, level, info);
    }

    /// <summary>
    /// Tracks whole-texture storage bytes for a specific live texture handle.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="texture">The texture handle receiving the shape.</param>
    /// <param name="info">The texture shape used for accounting.</param>
    private void TrackTextureSize(string function, GlTextureHandle texture, GlTextureInfo info)
    {
        if (!textures.Contains(texture))
            throw new GlException(function, $"cannot track texture size: texture {texture} is not tracked.");
        RemoveTextureLevels(texture);
        textureUsage += info.MemoryUsage - textureSizes.GetValueOrDefault(texture).MemoryUsage;
        textureLevelSizes[(texture, 0)] = info;
        textureSizes[texture] = info;
    }

    /// <summary>
    /// Tracks one texture level and keeps the texture's aggregate byte count up to date.
    /// </summary>
    /// <param name="function">The GL function that requested the accounting update.</param>
    /// <param name="texture">The texture handle receiving the level shape.</param>
    /// <param name="level">The mip level receiving the shape.</param>
    /// <param name="info">The texture level shape used for accounting.</param>
    private void TrackTextureLevelSize(string function, GlTextureHandle texture, int level, GlTextureInfo info)
    {
        if (!textures.Contains(texture))
            throw new GlException(function, $"cannot track texture size: texture {texture} is not tracked.");
        if (level < 0)
            throw new GlException(function, $"cannot track texture size: level {level} is negative.");
        var previous = textureSizes.GetValueOrDefault(texture).MemoryUsage;
        textureLevelSizes[(texture, level)] = info;
        var total = TotalTextureLevelUsage(texture);
        textureUsage += total - previous;
        textureSizes[texture] = info with { ByteSizeOverride = total };
    }

    /// <summary>
    /// Releases any tracked storage bytes for a texture that is being deleted.
    /// </summary>
    /// <param name="texture">The texture handle whose memory accounting should be released.</param>
    private void ReleaseTextureMemory(GlTextureHandle texture)
    {
        if (textureSizes.Remove(texture, out var info))
            textureUsage -= info.MemoryUsage;
        RemoveTextureLevels(texture);
    }

    /// <summary>
    /// Returns the total tracked bytes for all recorded levels of a texture.
    /// </summary>
    /// <param name="texture">The texture handle to total.</param>
    /// <returns>The total tracked bytes.</returns>
    private long TotalTextureLevelUsage(GlTextureHandle texture)
    {
        long total = 0;
        foreach (var (key, info) in textureLevelSizes)
            if (key.Texture == texture)
                total += info.MemoryUsage;
        return total;
    }

    /// <summary>
    /// Removes all per-level storage records for a texture.
    /// </summary>
    /// <param name="texture">The texture handle whose level records should be removed.</param>
    private void RemoveTextureLevels(GlTextureHandle texture)
    {
        while (TryFindTextureLevel(texture, out var key))
            textureLevelSizes.Remove(key);
    }

    /// <summary>
    /// Finds one recorded level for a texture without allocating during deletion cleanup.
    /// </summary>
    /// <param name="texture">The texture handle to find.</param>
    /// <param name="key">The matching level key, if found.</param>
    /// <returns><see langword="true"/> when a matching level was found.</returns>
    private bool TryFindTextureLevel(GlTextureHandle texture, out (GlTextureHandle Texture, int Level) key)
    {
        foreach (var candidate in textureLevelSizes.Keys)
        {
            if (candidate.Texture != texture)
                continue;
            key = candidate;
            return true;
        }
        key = default;
        return false;
    }
}
