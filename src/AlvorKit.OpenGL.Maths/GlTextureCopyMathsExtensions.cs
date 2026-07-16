namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped texture copy overloads.</summary>
public static class GlTextureCopyMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTexImage1D</c>.</summary>
    public static void CopyTexImage1D(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat,
        Vec2i sourceOrigin, uint width, int border) =>
        gl.CopyTexImage1D(target, level, internalFormat, sourceOrigin.X, sourceOrigin.Y,
            GlMathsConversions.ToSize(width), border);

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTexImage2D</c>.</summary>
    public static void CopyTexImage2D(this Gl gl, GlTextureTarget target, int level, GlInternalFormat internalFormat,
        Vec2i sourceOrigin, Vec2u size, int border)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CopyTexImage2D(target, level, internalFormat, sourceOrigin.X, sourceOrigin.Y, s.X, s.Y, border);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTexSubImage1D</c>.</summary>
    public static void CopyTexSubImage1D(this Gl gl, GlTextureTarget target, int level, int destinationOffset,
        Vec2i sourceOrigin, uint width) =>
        gl.CopyTexSubImage1D(target, level, destinationOffset, sourceOrigin.X, sourceOrigin.Y,
            GlMathsConversions.ToSize(width));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTexSubImage2D</c>.</summary>
    public static void CopyTexSubImage2D(this Gl gl, GlTextureTarget target, int level, Vec2i destinationOffset,
        Vec2i sourceOrigin, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CopyTexSubImage2D(target, level, destinationOffset.X, destinationOffset.Y, sourceOrigin.X, sourceOrigin.Y, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTexSubImage3D</c>.</summary>
    public static void CopyTexSubImage3D(this Gl gl, GlTextureTarget target, int level, Vec3i destinationOffset,
        Vec2i sourceOrigin, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CopyTexSubImage3D(target, level, destinationOffset.X, destinationOffset.Y, destinationOffset.Z,
            sourceOrigin.X, sourceOrigin.Y, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTextureSubImage1D</c>.</summary>
    public static void CopyTextureSubImage1D(this Gl gl, GlTextureHandle texture, int level, int destinationOffset,
        Vec2i sourceOrigin, uint width) =>
        gl.CopyTextureSubImage1D(texture, level, destinationOffset, sourceOrigin.X, sourceOrigin.Y,
            GlMathsConversions.ToSize(width));

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTextureSubImage2D</c>.</summary>
    public static void CopyTextureSubImage2D(this Gl gl, GlTextureHandle texture, int level, Vec2i destinationOffset,
        Vec2i sourceOrigin, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CopyTextureSubImage2D(texture, level, destinationOffset.X, destinationOffset.Y, sourceOrigin.X, sourceOrigin.Y, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyTextureSubImage3D</c>.</summary>
    public static void CopyTextureSubImage3D(this Gl gl, GlTextureHandle texture, int level, Vec3i destinationOffset,
        Vec2i sourceOrigin, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CopyTextureSubImage3D(texture, level, destinationOffset.X, destinationOffset.Y, destinationOffset.Z,
            sourceOrigin.X, sourceOrigin.Y, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glCopyImageSubData</c>.</summary>
    public static void CopyImageSubData(this Gl gl, uint sourceName, GlCopyImageSubDataTarget sourceTarget, int sourceLevel,
        Vec3i sourceOffset, uint destinationName, GlCopyImageSubDataTarget destinationTarget, int destinationLevel,
        Vec3i destinationOffset, Vec3u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.CopyImageSubData(sourceName, sourceTarget, sourceLevel, sourceOffset.X, sourceOffset.Y, sourceOffset.Z,
            destinationName, destinationTarget, destinationLevel, destinationOffset.X, destinationOffset.Y,
            destinationOffset.Z, s.X, s.Y, s.Z);
    }
}
