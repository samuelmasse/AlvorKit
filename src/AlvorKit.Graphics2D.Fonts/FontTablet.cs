namespace AlvorKit;

/// <summary>Owns one square glyph atlas texture.</summary>
internal class FontTablet : IDisposable
{
    /// <summary>The square atlas edge length in pixels.</summary>
    internal const int DefaultSize = 2048;

    /// <summary>The strict OpenGL layer that owns the atlas texture.</summary>
    private readonly GlLayer gl;

    /// <summary>The atlas texture.</summary>
    private readonly Texture texture;

    private GlTextureMagFilter magFilter;

    /// <summary>Creates an empty RGBA atlas texture.</summary>
    internal FontTablet(GlLayer gl, GlTextureMagFilter magFilter = GlTextureMagFilter.Linear)
    {
        this.gl = gl;
        this.magFilter = magFilter;
        texture = new Texture(gl, (DefaultSize, DefaultSize), GlTextureTarget.Texture2D);
        AllocateTexture();
    }

    /// <summary>Gets the square atlas edge length in pixels.</summary>
    internal int Size => DefaultSize;

    /// <summary>Gets the atlas texture.</summary>
    internal Texture Texture => texture;

    internal GlTextureMagFilter MagFilter => magFilter;

    /// <summary>Applies the owning font's filter when the scratch tablet becomes its atlas.</summary>
    internal void SetMagFilter(GlTextureMagFilter value)
    {
        if (value == magFilter)
            return;

        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture.Id);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMagFilter, (int)value);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        magFilter = value;
    }

    /// <summary>Deletes the atlas texture.</summary>
    public void Dispose() => texture.Dispose();

    /// <summary>Allocates texture storage and applies sampling parameters.</summary>
    private void AllocateTexture()
    {
        ReadOnlySpan<byte> empty = [];
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture.Id);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMinFilter, (int)GlTextureMinFilter.Linear);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMagFilter, (int)magFilter);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapS, (int)GlTextureWrapMode.ClampToEdge);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapT, (int)GlTextureWrapMode.ClampToEdge);
        gl.TexImage2D(
            GlTextureTarget.Texture2D,
            0,
            GlInternalFormat.Rgba8,
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
