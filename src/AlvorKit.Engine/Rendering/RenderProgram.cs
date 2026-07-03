namespace AlvorKit.Engine;

/// <summary>Owns a linked OpenGL shader program and its vertex-layout contract.</summary>
[ExcludeFromCodeCoverage(Justification = "Compiles and links OpenGL shaders through the live graphics backend.")]
public abstract class RenderProgram<T> : IRenderProgram where T : IVertex
{
    /// <summary>The strict OpenGL layer used by this program.</summary>
    protected readonly GlLayer gl;

    private readonly ShaderProgram program;

    /// <summary>Creates a program from vertex and fragment shader source.</summary>
    protected RenderProgram(GlLayer gl, string vertCode, string fragCode)
    {
        this.gl = gl;
        using var vert = new ShaderStage(gl, vertCode, GlShaderType.VertexShader);
        using var frag = new ShaderStage(gl, fragCode, GlShaderType.FragmentShader);
        program = new(gl, [vert, frag]);
    }

    /// <inheritdoc />
    public GlProgramHandle Id => program.Id;

    /// <inheritdoc />
    public void SetAttributes() => T.SetAttributes(gl);
}
