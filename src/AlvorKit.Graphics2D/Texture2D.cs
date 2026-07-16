namespace AlvorKit.Graphics2D;

/// <summary>Owns a two-dimensional RGBA texture and uploads pixel spans into level zero.</summary>
public class Texture2D : Texture
{
    /// <summary>Creates a texture using <see cref="GlTextureTarget.Texture2D"/>.</summary>
    public Texture2D(GlLayer gl, Vec2u size) : base(gl, size, GlTextureTarget.Texture2D) { }

    /// <summary>Creates a texture using the supplied two-dimensional target.</summary>
    public Texture2D(GlLayer gl, Vec2u size, GlTextureTarget target) : base(gl, size, target) { }

    /// <summary>Uploads RGBA8 pixels into texture level zero.</summary>
    public ReadOnlySpan<Vec4u8> Pixels { set => TexImage2D(value); }

    /// <summary>Uploads RGBA8 pixels into texture level zero and regenerates mipmaps.</summary>
    public ReadOnlySpan<Vec4u8> PixelsMipmap
    {
        set
        {
            TexImage2D(value);
            GenerateMipmap();
        }
    }

    /// <summary>Uploads unmanaged pixel data into texture level zero as tightly packed RGBA unsigned bytes.</summary>
    /// <typeparam name="T">The unmanaged pixel element type whose byte layout must match RGBA unsigned-byte data.</typeparam>
    /// <param name="pixels">The caller-owned pixel span to upload for the duration of the call.</param>
    /// <exception cref="ArgumentException">Thrown when the span byte count does not match the texture size.</exception>
    public void TexImage2D<T>(ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        const int bytesPerPixel = 4;

        var width = checked((int)size.X);
        var height = checked((int)size.Y);
        var pixelBytes = MemoryMarshal.AsBytes(pixels);
        var expectedBytes = (long)width * height * bytesPerPixel;
        if (pixelBytes.Length != expectedBytes)
            throw new ArgumentException("Pixel byte count does not match the texture size.", nameof(pixels));

        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(target, id);
        gl.TexImage2D(
            target,
            0,
            GlInternalFormat.Rgba8,
            size,
            0,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            pixelBytes);
        gl.UnbindTexture(target);
        gl.ResetActiveTexture();
    }
}
