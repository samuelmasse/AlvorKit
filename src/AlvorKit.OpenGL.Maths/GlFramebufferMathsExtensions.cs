namespace AlvorKit.OpenGL;

/// <summary>Provides maths-shaped framebuffer and renderbuffer overloads.</summary>
public static class GlFramebufferMathsExtensions
{
    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glRenderbufferStorage</c>.</summary>
    public static void RenderbufferStorage(this Gl gl, GlRenderbufferTarget target,
        GlInternalFormat internalFormat, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.RenderbufferStorage(target, internalFormat, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glRenderbufferStorageMultisample</c>.</summary>
    public static void RenderbufferStorageMultisample(this Gl gl, GlRenderbufferTarget target, int samples,
        GlInternalFormat internalFormat, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.RenderbufferStorageMultisample(target, samples, internalFormat, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glNamedRenderbufferStorage</c>.</summary>
    public static void NamedRenderbufferStorage(this Gl gl, GlRenderbufferHandle renderbuffer,
        GlInternalFormat internalFormat, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.NamedRenderbufferStorage(renderbuffer, internalFormat, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glNamedRenderbufferStorageMultisample</c>.</summary>
    public static void NamedRenderbufferStorageMultisample(this Gl gl, GlRenderbufferHandle renderbuffer, int samples,
        GlInternalFormat internalFormat, Vec2u size)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.NamedRenderbufferStorageMultisample(renderbuffer, samples, internalFormat, s.X, s.Y);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glReadPixels</c>.</summary>
    public static void ReadPixels(this Gl gl, Vec2i origin, Vec2u size,
        GlPixelFormat format, GlPixelType type, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.ReadPixels(origin.X, origin.Y, s.X, s.Y, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glReadPixels</c>.</summary>
    public static void ReadPixels<T>(this Gl gl, Vec2i origin, Vec2u size,
        GlPixelFormat format, GlPixelType type, Span<T> pixels) where T : unmanaged
    {
        var s = GlMathsConversions.ToSize(size);
        gl.ReadPixels(origin.X, origin.Y, s.X, s.Y, format, type, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glReadPixels</c> at the zero origin.</summary>
    public static void ReadPixels(this Gl gl, Vec2u size, GlPixelFormat format, GlPixelType type, nint pixels) =>
        gl.ReadPixels(default, size, format, type, pixels);

    /// <summary>Calls the raw <see cref="Gl"/> span member for <c>glReadPixels</c> at the zero origin.</summary>
    public static void ReadPixels<T>(this Gl gl, Vec2u size, GlPixelFormat format, GlPixelType type, Span<T> pixels)
        where T : unmanaged =>
        gl.ReadPixels(default, size, format, type, pixels);

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glReadnPixels</c>.</summary>
    public static void ReadnPixels(this Gl gl, Vec2i origin, Vec2u size,
        GlPixelFormat format, GlPixelType type, int bufferSize, nint pixels)
    {
        var s = GlMathsConversions.ToSize(size);
        gl.ReadnPixels(origin.X, origin.Y, s.X, s.Y, format, type, bufferSize, pixels);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glReadnPixels</c> at the zero origin.</summary>
    public static void ReadnPixels(this Gl gl, Vec2u size,
        GlPixelFormat format, GlPixelType type, int bufferSize, nint pixels) =>
        gl.ReadnPixels(default, size, format, type, bufferSize, pixels);

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glBlitFramebuffer</c> with origin-plus-size regions.</summary>
    public static void BlitFramebuffer(this Gl gl, Vec2i sourceOrigin, Vec2u sourceSize,
        Vec2i destinationOrigin, Vec2u destinationSize, GlClearBufferMask mask, GlBlitFramebufferFilter filter)
    {
        var sourceEnd = GlMathsConversions.ToEnd(sourceOrigin, sourceSize);
        var destinationEnd = GlMathsConversions.ToEnd(destinationOrigin, destinationSize);
        gl.BlitFramebuffer(sourceOrigin.X, sourceOrigin.Y, sourceEnd.X, sourceEnd.Y,
            destinationOrigin.X, destinationOrigin.Y, destinationEnd.X, destinationEnd.Y, mask, filter);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glBlitNamedFramebuffer</c> with origin-plus-size regions.</summary>
    public static void BlitNamedFramebuffer(this Gl gl, GlFramebufferHandle sourceFramebuffer,
        GlFramebufferHandle destinationFramebuffer, Vec2i sourceOrigin, Vec2u sourceSize,
        Vec2i destinationOrigin, Vec2u destinationSize, GlClearBufferMask mask, GlBlitFramebufferFilter filter)
    {
        var sourceEnd = GlMathsConversions.ToEnd(sourceOrigin, sourceSize);
        var destinationEnd = GlMathsConversions.ToEnd(destinationOrigin, destinationSize);
        gl.BlitNamedFramebuffer(sourceFramebuffer, destinationFramebuffer,
            sourceOrigin.X, sourceOrigin.Y, sourceEnd.X, sourceEnd.Y,
            destinationOrigin.X, destinationOrigin.Y, destinationEnd.X, destinationEnd.Y, mask, filter);
    }

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glBlitFramebuffer</c> with exact endpoint vectors.</summary>
    public static void BlitFramebuffer(this Gl gl, Vec4i sourceEndpoints, Vec4i destinationEndpoints,
        GlClearBufferMask mask, GlBlitFramebufferFilter filter) =>
        gl.BlitFramebuffer(sourceEndpoints.X, sourceEndpoints.Y, sourceEndpoints.Z, sourceEndpoints.W,
            destinationEndpoints.X, destinationEndpoints.Y, destinationEndpoints.Z, destinationEndpoints.W, mask, filter);

    /// <summary>Calls the raw <see cref="Gl"/> member for <c>glBlitNamedFramebuffer</c> with exact endpoint vectors.</summary>
    public static void BlitNamedFramebuffer(this Gl gl, GlFramebufferHandle sourceFramebuffer,
        GlFramebufferHandle destinationFramebuffer, Vec4i sourceEndpoints, Vec4i destinationEndpoints,
        GlClearBufferMask mask, GlBlitFramebufferFilter filter) =>
        gl.BlitNamedFramebuffer(sourceFramebuffer, destinationFramebuffer,
            sourceEndpoints.X, sourceEndpoints.Y, sourceEndpoints.Z, sourceEndpoints.W,
            destinationEndpoints.X, destinationEndpoints.Y, destinationEndpoints.Z, destinationEndpoints.W, mask, filter);
}
