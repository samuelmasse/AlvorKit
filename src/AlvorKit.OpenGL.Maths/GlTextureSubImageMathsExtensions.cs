namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped bound and direct-state-access texture subimage overloads.</summary>
public static class GlTextureSubImageMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexSubImage2D</c>.</summary>
    public static void TexSubImage2D(this Gl gl, GlTextureTarget target, int level, Vec2i offset, Vec2u size,
        GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexSubImage2D(target, level, offset.X, offset.Y, s.X, s.Y, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glTexSubImage2D</c>.</summary>
    public static void TexSubImage2D<T>(this Gl gl, GlTextureTarget target, int level, Vec2i offset, Vec2u size,
        GlPixelFormat format, GlPixelType type, ReadOnlySpan<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexSubImage2D(target, level, offset.X, offset.Y, s.X, s.Y, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTexSubImage3D</c>.</summary>
    public static void TexSubImage3D(this Gl gl, GlTextureTarget target, int level, Vec3i offset, Vec3u size,
        GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexSubImage3D(target, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glTexSubImage3D</c>.</summary>
    public static void TexSubImage3D<T>(this Gl gl, GlTextureTarget target, int level, Vec3i offset, Vec3u size,
        GlPixelFormat format, GlPixelType type, ReadOnlySpan<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TexSubImage3D(target, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCompressedTexSubImage2D</c>.</summary>
    public static void CompressedTexSubImage2D(this Gl gl, GlTextureTarget target, int level, Vec2i offset, Vec2u size,
        GlInternalFormat format, int imageSize, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CompressedTexSubImage2D(target, level, offset.X, offset.Y, s.X, s.Y, format, imageSize, data);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCompressedTexSubImage3D</c>.</summary>
    public static void CompressedTexSubImage3D(this Gl gl, GlTextureTarget target, int level, Vec3i offset, Vec3u size,
        GlInternalFormat format, int imageSize, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CompressedTexSubImage3D(target, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, imageSize, data);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTextureSubImage2D</c>.</summary>
    public static void TextureSubImage2D(this Gl gl, GlTextureHandle texture, int level, Vec2i offset, Vec2u size,
        GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureSubImage2D(texture, level, offset.X, offset.Y, s.X, s.Y, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glTextureSubImage2D</c>.</summary>
    public static void TextureSubImage2D<T>(this Gl gl, GlTextureHandle texture, int level, Vec2i offset, Vec2u size,
        GlPixelFormat format, GlPixelType type, ReadOnlySpan<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureSubImage2D(texture, level, offset.X, offset.Y, s.X, s.Y, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glTextureSubImage3D</c>.</summary>
    public static void TextureSubImage3D(this Gl gl, GlTextureHandle texture, int level, Vec3i offset, Vec3u size,
        GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureSubImage3D(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glTextureSubImage3D</c>.</summary>
    public static void TextureSubImage3D<T>(this Gl gl, GlTextureHandle texture, int level, Vec3i offset, Vec3u size,
        GlPixelFormat format, GlPixelType type, ReadOnlySpan<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.TextureSubImage3D(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCompressedTextureSubImage2D</c>.</summary>
    public static void CompressedTextureSubImage2D(this Gl gl, GlTextureHandle texture, int level, Vec2i offset,
        Vec2u size, GlInternalFormat format, int imageSize, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CompressedTextureSubImage2D(texture, level, offset.X, offset.Y, s.X, s.Y, format, imageSize, data);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCompressedTextureSubImage3D</c>.</summary>
    public static void CompressedTextureSubImage3D(this Gl gl, GlTextureHandle texture, int level, Vec3i offset,
        Vec3u size, GlInternalFormat format, int imageSize, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CompressedTextureSubImage3D(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, imageSize, data);
    }
}
