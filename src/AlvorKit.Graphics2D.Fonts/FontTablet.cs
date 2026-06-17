namespace AlvorKit.Graphics2D.Fonts;

/// <summary>Owns one square glyph atlas texture.</summary>
internal sealed class FontTablet : IDisposable
{
    /// <summary>The square atlas edge length in pixels.</summary>
    internal const int DefaultSize = 2048;

    /// <summary>The strict OpenGL layer that owns the atlas texture.</summary>
    private readonly GlLayer gl;

    /// <summary>The atlas texture.</summary>
    private readonly Texture texture;

    /// <summary>Creates an empty RGBA atlas texture.</summary>
    internal FontTablet(GlLayer gl)
    {
        this.gl = gl;
        texture = new Texture(gl, nameof(FontTablet), new Vector2(DefaultSize), GlTextureTarget.Texture2D);
        AllocateTexture();
    }

    /// <summary>Gets the square atlas edge length in pixels.</summary>
    internal int Size => DefaultSize;

    /// <summary>Gets the atlas texture.</summary>
    internal Texture Texture => texture;

    /// <summary>Deletes the atlas texture.</summary>
    public void Dispose() => texture.Dispose();

    /// <summary>Allocates texture storage and applies sampling parameters.</summary>
    private void AllocateTexture()
    {
        ReadOnlySpan<byte> empty = [];
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture.Id);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMinFilter, (int)GlTextureMinFilter.Linear);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMagFilter, (int)GlTextureMagFilter.Linear);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapS, (int)GlTextureWrapMode.ClampToEdge);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapT, (int)GlTextureWrapMode.ClampToEdge);
        gl.TexImage2D(
            GlTextureTarget.Texture2D,
            0,
            GlInternalFormat.Rgba,
            Size,
            Size,
            0,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            empty);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
    }
}
