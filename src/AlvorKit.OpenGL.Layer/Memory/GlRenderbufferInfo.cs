namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// The shape of a renderbuffer, used to estimate its GPU memory. Reuses <see cref="GlTextureInfo"/>'s
/// per-format byte estimate (renderbuffer formats are a subset of the texture internal formats).
/// </summary>
/// <param name="InternalFormat">The internal storage format requested for the renderbuffer.</param>
/// <param name="Width">The renderbuffer width in pixels.</param>
/// <param name="Height">The renderbuffer height in pixels.</param>
/// <param name="Samples">The multisample count, or one for non-multisampled storage.</param>
public readonly record struct GlRenderbufferInfo(
    GlInternalFormat InternalFormat,
    int Width,
    int Height,
    int Samples)
{
    /// <summary>The estimated byte size of this renderbuffer.</summary>
    public long MemoryUsage => new GlTextureInfo(InternalFormat, (Width, Height, 1), default, default, Samples).MemoryUsage;
}
