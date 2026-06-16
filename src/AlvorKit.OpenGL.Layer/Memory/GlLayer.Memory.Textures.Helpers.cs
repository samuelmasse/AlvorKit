namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Tracks whole-texture storage bytes for the texture currently bound to a target on the active texture unit.
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
    /// Returns how many dimensions should shrink when estimating mip chains for a target.
    /// </summary>
    /// <param name="target">The texture target being allocated.</param>
    /// <returns>The number of mip-reduced dimensions.</returns>
    private static int MipmapDimensionsFor(GlTextureTarget target) => target switch
    {
        GlTextureTarget.Texture1D or GlTextureTarget.ProxyTexture1D or GlTextureTarget.Texture1DArray or GlTextureTarget.ProxyTexture1DArray => 1,
        GlTextureTarget.Texture3D or GlTextureTarget.ProxyTexture3D => 3,
        _ => 2,
    };

    /// <summary>
    /// Returns the implicit depth for storage targets whose API shape omits it.
    /// </summary>
    /// <param name="target">The texture storage target.</param>
    /// <returns>The implicit storage depth.</returns>
    private static int DepthForStorageTarget(GlTextureTarget target) =>
        target is GlTextureTarget.TextureCubeMap or GlTextureTarget.ProxyTextureCubeMap ? 6 : 1;
}
