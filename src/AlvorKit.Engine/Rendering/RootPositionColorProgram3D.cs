namespace AlvorKit.Engine;

/// <summary>Root-owned 3D position-color shader program.</summary>
[Root]
[ExcludeFromCodeCoverage(Justification = "Compiles and links OpenGL shaders through the live graphics backend.")]
public sealed class RootPositionColorProgram3D(RootGl gl) :
    RenderProgram3D<PositionColorVertex>(gl, Vert, RootPositionColorProgram.Frag)
{
    /// <summary>Vertex shader source for 3D non-textured color rendering.</summary>
    internal const string Vert =
        """
        #version 330 core

        layout (location = 0) in vec3 inPosition;
        layout (location = 1) in vec3 inColor;

        out vec3 fragColor;

        uniform mat4 matView;
        uniform mat4 matProjection;

        void main()
        {
            gl_Position = matProjection * matView * vec4(inPosition, 1.0);
            fragColor = inColor;
        }
        """;
}
