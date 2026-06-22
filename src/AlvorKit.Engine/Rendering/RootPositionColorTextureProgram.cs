namespace AlvorKit.Engine;

/// <summary>Root-owned simple position-color-texture shader program.</summary>
[Root]
[ExcludeFromCodeCoverage(Justification = "Compiles and links OpenGL shaders through the live graphics backend.")]
public sealed class RootPositionColorTextureProgram : RenderProgram<PositionColorTextureVertex>, ITextureProgram
{
    /// <summary>Vertex shader source for textured color rendering.</summary>
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;
        layout (location = 2) in vec2 inTexCoord;

        out vec3 fragColor;
        out vec2 fragTexCoord;

        void main()
        {
            gl_Position = vec4(inPosition, 1.0);
            fragColor = inColor;
            fragTexCoord = inTexCoord;
        }
        """;

    /// <summary>Fragment shader source for textured color rendering.</summary>
    internal const string Frag =
        """
        #version 330 core

        in vec3 fragColor;
        in vec2 fragTexCoord;

        layout (location = 0) out vec4 outColor;

        uniform sampler2D samplerTexture;

        void main()
        {
            outColor = texture(samplerTexture, fragTexCoord) * vec4(fragColor, 1.0);
        }
        """;

    private readonly GlTextureUnit samplerTexture = GlTextureUnit.Texture0;

    /// <summary>Creates the shader program and assigns its sampler uniform.</summary>
    public RootPositionColorTextureProgram(RootGl gl) : base(gl, Vert, Frag) =>
        gl.ProgramUniform1i(Id, gl.GetUniformLocation(Id, nameof(samplerTexture)), (int)samplerTexture);

    /// <inheritdoc />
    public GlTextureUnit SamplerTexture => samplerTexture;
}
