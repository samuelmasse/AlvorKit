namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped texture subregion overloads.</summary>
public static class GlTextureRegionMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glClearTexSubImage</c>.</summary>
    public static void ClearTexSubImage(this Gl gl, GlTextureHandle texture, int level, Vec3i offset, Vec3u size,
        GlPixelFormat format, GlPixelType type, nint data)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.ClearTexSubImage(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, type, data);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glInvalidateTexSubImage</c>.</summary>
    public static void InvalidateTexSubImage(this Gl gl, GlTextureHandle texture, int level, Vec3i offset, Vec3u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.InvalidateTexSubImage(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glGetTextureSubImage</c>.</summary>
    public static void GetTextureSubImage(this Gl gl, GlTextureHandle texture, int level, Vec3i offset, Vec3u size,
        GlPixelFormat format, GlPixelType type, int bufferSize, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.GetTextureSubImage(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, format, type, bufferSize, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glGetCompressedTextureSubImage</c>.</summary>
    public static void GetCompressedTextureSubImage(this Gl gl, GlTextureHandle texture, int level, Vec3i offset,
        Vec3u size, int bufferSize, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.GetCompressedTextureSubImage(texture, level, offset.X, offset.Y, offset.Z, s.X, s.Y, s.Z, bufferSize, pixels);
    }
}
