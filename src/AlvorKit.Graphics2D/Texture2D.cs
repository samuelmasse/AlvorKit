namespace AlvorKit.Graphics2D;

/// <summary>Owns a two-dimensional RGBA texture and uploads pixel spans into level zero.</summary>
public class Texture2D : Texture
{
    /// <summary>Creates an unlabeled texture using <see cref="GlTextureTarget.Texture2D"/>.</summary>
    public Texture2D(GlLayer gl, Vector2 size) : base(gl, null, size, GlTextureTarget.Texture2D) { }

    /// <summary>Creates an unlabeled texture using the supplied two-dimensional target.</summary>
    public Texture2D(GlLayer gl, Vector2 size, GlTextureTarget target) : base(gl, null, size, target) { }

    /// <summary>Creates a labeled texture using <see cref="GlTextureTarget.Texture2D"/>.</summary>
    public Texture2D(GlLayer gl, string? label, Vector2 size) : base(gl, label, size, GlTextureTarget.Texture2D) { }

    /// <summary>Creates a labeled texture using the supplied two-dimensional target.</summary>
    public Texture2D(GlLayer gl, string? label, Vector2 size, GlTextureTarget target) : base(gl, label, size, target) { }

    /// <summary>Uploads RGBA byte pixels into texture level zero.</summary>
    public ReadOnlySpan<(byte Red, byte Green, byte Blue, byte Alpha)> Pixels { set => TexImage2D(value); }

    /// <summary>Uploads RGBA byte pixels into texture level zero and regenerates mipmaps.</summary>
    public ReadOnlySpan<(byte Red, byte Green, byte Blue, byte Alpha)> PixelsMipmap
    {
        set
        {
            TexImage2D(value);
            GenerateMipmap();
        }
    }

    /// <summary>Uploads unmanaged pixel data into texture level zero as RGBA unsigned bytes.</summary>
    /// <typeparam name="T">The unmanaged pixel element type.</typeparam>
    /// <param name="pixels">The caller-owned pixel span to upload for the duration of the call.</param>
    public void TexImage2D<T>(ReadOnlySpan<T> pixels)
        where T : unmanaged
    {
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(target, id);
        gl.TexImage2D(
            target,
            0,
            GlInternalFormat.Rgba,
            (int)size.X,
            (int)size.Y,
            0,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            pixels);
        gl.UnbindTexture(target);
        gl.ResetActiveTexture();
    }
}
