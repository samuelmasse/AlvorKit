namespace AlvorKit.Engine;

/// <summary>Shader program API for programs that sample a texture.</summary>
public interface ITextureProgram : IRenderProgram
{
    /// <summary>Gets the texture unit used by the program's sampler.</summary>
    GlTextureUnit SamplerTexture { get; }
}
