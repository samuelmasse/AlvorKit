namespace AlvorKit.Graphics2D;

/// <summary>Owns a tracked OpenGL texture and exposes strict bind helpers for a single target.</summary>
/// <param name="gl">The strict OpenGL layer that owns the texture handle.</param>
/// <param name="label">Optional diagnostic label used by callers when reporting texture ownership.</param>
/// <param name="size">The logical pixel size associated with the texture.</param>
/// <param name="target">The OpenGL texture target used by this texture handle.</param>
public class Texture(GlLayer gl, string? label, Vector2 size, GlTextureTarget target) : IDisposable
{
    /// <summary>The strict OpenGL command surface that owns this texture.</summary>
    protected readonly GlLayer gl = gl;

    /// <summary>The texture handle tracked by <see cref="GlLayer"/>.</summary>
    protected readonly GlTextureHandle id = gl.GenTexture();

    /// <summary>The texture target used for binds, uploads, parameters, and mipmap generation.</summary>
    protected readonly GlTextureTarget target = target;

    /// <summary>The logical pixel size associated with this texture.</summary>
    protected Vector2 size = size;

    /// <summary>Gets the optional diagnostic label supplied by the caller.</summary>
    public string? Label => label;

    /// <summary>Gets the OpenGL texture handle.</summary>
    public GlTextureHandle Id => id;

    /// <summary>Gets the logical pixel size associated with the texture.</summary>
    public Vector2 Size => size;

    /// <summary>Gets the OpenGL target used for this texture handle.</summary>
    public GlTextureTarget Target => target;

    /// <summary>Sets the minification filter for this texture.</summary>
    public GlTextureMinFilter MinFilter { set => TexParameter(GlTextureParameterName.TextureMinFilter, (int)value); }

    /// <summary>Sets the magnification filter for this texture.</summary>
    public GlTextureMagFilter MagFilter { set => TexParameter(GlTextureParameterName.TextureMagFilter, (int)value); }

    /// <summary>Sets the horizontal wrap mode for this texture.</summary>
    public GlTextureWrapMode WrapS { set => TexParameter(GlTextureParameterName.TextureWrapS, (int)value); }

    /// <summary>Sets the vertical wrap mode for this texture.</summary>
    public GlTextureWrapMode WrapT { set => TexParameter(GlTextureParameterName.TextureWrapT, (int)value); }

    /// <summary>Creates an unlabeled texture for the supplied target.</summary>
    /// <param name="gl">The strict OpenGL layer that owns the texture handle.</param>
    /// <param name="size">The logical pixel size associated with the texture.</param>
    /// <param name="target">The OpenGL texture target used by this texture handle.</param>
    public Texture(GlLayer gl, Vector2 size, GlTextureTarget target) : this(gl, null, size, target) { }

    /// <summary>Binds this texture to the requested texture unit.</summary>
    /// <param name="unit">The texture unit that should receive the texture binding.</param>
    public void Bind(GlTextureUnit unit)
    {
        gl.ActiveTexture(unit);
        gl.BindTexture(target, id);
        gl.ResetActiveTexture();
    }

    /// <summary>Binds this texture to texture unit zero.</summary>
    public void Bind() => Bind(GlTextureUnit.Texture0);

    /// <summary>Unbinds this texture from the requested texture unit.</summary>
    /// <param name="unit">The texture unit whose binding should be released.</param>
    public void Unbind(GlTextureUnit unit)
    {
        gl.ActiveTexture(unit);
        gl.UnbindTexture(target);
        gl.ResetActiveTexture();
    }

    /// <summary>Unbinds this texture from texture unit zero.</summary>
    public void Unbind() => Unbind(GlTextureUnit.Texture0);

    /// <summary>Sets one integer texture parameter while preserving the layer's strict binding discipline.</summary>
    /// <param name="name">The texture parameter to set.</param>
    /// <param name="value">The raw OpenGL parameter value.</param>
    public void TexParameter(GlTextureParameterName name, int value)
    {
        Bind();
        gl.TexParameteri(target, name, value);
        Unbind();
    }

    /// <summary>Generates mipmaps for the texture while preserving strict texture binding state.</summary>
    public void GenerateMipmap()
    {
        Bind();
        gl.GenerateMipmap(target);
        Unbind();
    }

    /// <summary>Deletes the tracked texture handle.</summary>
    public void Dispose() => gl.DeleteTexture(id);
}
