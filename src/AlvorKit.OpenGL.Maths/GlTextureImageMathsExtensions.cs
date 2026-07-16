namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped texture image definition overloads.</summary>
public static class GlTextureImageMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexImage2D</c> with a two-dimensional extent.</summary>
    public static void TexImage2D(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat, Vec2u size,
        int border, GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexImage2D(target, level, internalFormat, s.X, s.Y, border, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glTexImage2D</c> with a two-dimensional extent.</summary>
    public static void TexImage2D<T>(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat, Vec2u size,
        int border, GlPixelFormat format, GlPixelType type, ReadOnlySpan<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexImage2D(target, level, internalFormat, s.X, s.Y, border, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexImage3D</c> with a three-dimensional extent.</summary>
    public static void TexImage3D(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat, Vec3u size,
        int border, GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexImage3D(target, level, internalFormat, s.X, s.Y, s.Z, border, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glTexImage3D</c> with a three-dimensional extent.</summary>
    public static void TexImage3D<T>(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat, Vec3u size,
        int border, GlPixelFormat format, GlPixelType type, ReadOnlySpan<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexImage3D(target, level, internalFormat, s.X, s.Y, s.Z, border, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCompressedTexImage2D</c> with a two-dimensional extent.</summary>
    public static void CompressedTexImage2D(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat,
        Vec2u size, int border, int imageSize, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CompressedTexImage2D(target, level, internalFormat, s.X, s.Y, border, imageSize, data);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCompressedTexImage3D</c> with a three-dimensional extent.</summary>
    public static void CompressedTexImage3D(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat,
        Vec3u size, int border, int imageSize, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CompressedTexImage3D(target, level, internalFormat, s.X, s.Y, s.Z, border, imageSize, data);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexImage2DMultisample</c> with a two-dimensional extent.</summary>
    public static void TexImage2DMultisample(this Gl gl, GlTextureTarget target, int samples, GlInternalFormat internalFormat,
        Vec2u size, bool fixedSampleLocations)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexImage2DMultisample(target, samples, internalFormat, s.X, s.Y, fixedSampleLocations);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexImage3DMultisample</c> with a three-dimensional extent.</summary>
    public static void TexImage3DMultisample(this Gl gl, GlTextureTarget target, int samples, GlInternalFormat internalFormat,
        Vec3u size, bool fixedSampleLocations)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexImage3DMultisample(target, samples, internalFormat, s.X, s.Y, s.Z, fixedSampleLocations);
    }
}
