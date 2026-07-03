namespace AlvorKit.Engine;

/// <summary>Root-owned 3D position-color-texture shader program.</summary>
[Root]
[ExcludeFromCodeCoverage(Justification = "Compiles and links OpenGL shaders through the live graphics backend.")]
public class RootPositionColorTextureProgram3D : RenderProgram3D<PositionColorTextureVertex>, ITextureProgram
{
    /// <summary>Vertex shader source for 3D textured color rendering.</summary>
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;
        layout (location = 2) in vec2 inTexCoord;

        out vec3 fragColor;
        out vec2 fragTexCoord;

        uniform mat4 matView;
        uniform mat4 matProjection;

        void main()
        {
            gl_Position = vec4(inPosition, 1.0) * matView * matProjection;
            fragColor = inColor;
            fragTexCoord = inTexCoord;
        }
        """;

    private readonly GlTextureUnit samplerTexture = GlTextureUnit.Texture0;

    /// <summary>Creates the shader program and assigns its sampler uniform.</summary>
    public RootPositionColorTextureProgram3D(RootGl gl) : base(gl, Vert, RootPositionColorTextureProgram.Frag) =>
        gl.ProgramUniform1i(Id, gl.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);

    /// <inheritdoc />
    public GlTextureUnit SamplerTexture => samplerTexture;
}
