namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped texture storage overloads.</summary>
public static class GlTextureStorageMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexStorage2D</c>.</summary>
    public static void TexStorage2D(this Gl gl, GlTextureTarget target, int levels, GlSizedInternalFormat internalFormat, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexStorage2D(target, levels, internalFormat, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexStorage3D</c>.</summary>
    public static void TexStorage3D(this Gl gl, GlTextureTarget target, int levels, GlSizedInternalFormat internalFormat, Vec3u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexStorage3D(target, levels, internalFormat, s.X, s.Y, s.Z);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexStorage2DMultisample</c>.</summary>
    public static void TexStorage2DMultisample(this Gl gl, GlTextureTarget target, int samples,
        GlSizedInternalFormat internalFormat, Vec2u size, bool fixedSampleLocations)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexStorage2DMultisample(target, samples, internalFormat, s.X, s.Y, fixedSampleLocations);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexStorage3DMultisample</c>.</summary>
    public static void TexStorage3DMultisample(this Gl gl, GlTextureTarget target, int samples,
        GlSizedInternalFormat internalFormat, Vec3u size, bool fixedSampleLocations)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexStorage3DMultisample(target, samples, internalFormat, s.X, s.Y, s.Z, fixedSampleLocations);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTextureStorage2D</c>.</summary>
    public static void TextureStorage2D(this Gl gl, GlTextureHandle texture, int levels,
        GlSizedInternalFormat internalFormat, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureStorage2D(texture, levels, internalFormat, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTextureStorage3D</c>.</summary>
    public static void TextureStorage3D(this Gl gl, GlTextureHandle texture, int levels,
        GlSizedInternalFormat internalFormat, Vec3u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureStorage3D(texture, levels, internalFormat, s.X, s.Y, s.Z);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTextureStorage2DMultisample</c>.</summary>
    public static void TextureStorage2DMultisample(this Gl gl, GlTextureHandle texture, int samples,
        GlSizedInternalFormat internalFormat, Vec2u size, bool fixedSampleLocations)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureStorage2DMultisample(texture, samples, internalFormat, s.X, s.Y, fixedSampleLocations);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTextureStorage3DMultisample</c>.</summary>
    public static void TextureStorage3DMultisample(this Gl gl, GlTextureHandle texture, int samples,
        GlSizedInternalFormat internalFormat, Vec3u size, bool fixedSampleLocations)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureStorage3DMultisample(texture, samples, internalFormat, s.X, s.Y, s.Z, fixedSampleLocations);
    }
}
