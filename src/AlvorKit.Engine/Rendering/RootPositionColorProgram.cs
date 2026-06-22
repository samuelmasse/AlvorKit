namespace AlvorKit.Engine;

/// <summary>Root-owned simple position-color shader program.</summary>
[Root]
[ExcludeFromCodeCoverage(Justification = "Compiles and links OpenGL shaders through the live graphics backend.")]
public sealed class RootPositionColorProgram(RootGl gl) : RenderProgram<PositionColorVertex>(gl, Vert, Frag)
{
    /// <summary>Vertex shader source for non-textured color rendering.</summary>
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;

        out vec3 fragColor;

        void main()
        {
            gl_Position = vec4(inPosition, 1.0);
            fragColor = inColor;
        }
        """;

    /// <summary>Fragment shader source for non-textured color rendering.</summary>
    internal const string Frag =
        """
        #version 330 core

        in vec3 fragColor;

        layout (location = 0) out vec4 outColor;

        void main()
        {
            outColor = vec4(fragColor, 1.0f);
        }
        """;
}
