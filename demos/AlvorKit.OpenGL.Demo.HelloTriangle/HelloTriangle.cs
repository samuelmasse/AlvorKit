namespace AlvorKit.OpenGL.Demo.HelloTriangle;

/// <summary>The classic coloured triangle, driven directly through the raw <see cref="Gl"/> surface.</summary>
public sealed class HelloTriangle : IDisposable
{
    /// <summary>The number of vertices submitted by the triangle draw call.</summary>
    private const int VertexCount = 3;

    /// <summary>The number of floats in one interleaved position/color vertex.</summary>
    private const int FloatsPerVertex = 6;

    /// <summary>The byte stride between adjacent interleaved vertices.</summary>
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);

    /// <summary>The byte offset of the position attribute inside a vertex.</summary>
    private const int PositionOffsetBytes = 0;

    /// <summary>The byte offset of the color attribute inside a vertex.</summary>
    private const int ColorOffsetBytes = 3 * sizeof(float);

    /// <summary>Vertex shader source for transforming positions and passing per-vertex color through.</summary>
    private static readonly string VertexShaderSource = ReadShader("triangle.vert.glsl");

    /// <summary>Fragment shader source that writes the interpolated vertex color to the framebuffer.</summary>
    private static readonly string FragmentShaderSource = ReadShader("triangle.frag.glsl");

    /// <summary>The raw OpenGL command surface bound to the live GLFW context.</summary>
    private readonly Gl gl;

    /// <summary>The linked shader program used by every triangle frame.</summary>
    private readonly GlProgramHandle program;

    /// <summary>The vertex array object that captures the triangle attribute layout.</summary>
    private readonly GlVertexArrayHandle vertexArray;

    /// <summary>The immutable buffer containing the triangle's interleaved position and color data.</summary>
    private readonly GlBufferHandle vertexBuffer;

    /// <summary>Captures the OpenGL resources that make up a loaded triangle.</summary>
    private HelloTriangle(
        Gl gl,
        GlProgramHandle program,
        GlVertexArrayHandle vertexArray,
        GlBufferHandle vertexBuffer)
    {
        this.gl = gl;
        this.program = program;
        this.vertexArray = vertexArray;
        this.vertexBuffer = vertexBuffer;
    }

    /// <summary>Compiles the shaders, uploads the vertices, and returns the resource owner for the demo loop.</summary>
    public static HelloTriangle Load(Gl gl)
    {
        var program = CreateProgram(gl);
        var (vertexArray, vertexBuffer) = CreateGeometry(gl);
        return new HelloTriangle(gl, program, vertexArray, vertexBuffer);
    }

    /// <summary>Binds the loaded program and vertex array, draws the triangle, and restores the raw bindings.</summary>
    public void Render()
    {
        gl.UseProgram(program);
        gl.BindVertexArray(vertexArray);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, VertexCount);
        gl.BindVertexArray(default);
        gl.UseProgram(default);
    }

    /// <summary>Deletes GPU resources once the frame loop has stopped drawing the triangle.</summary>
    public void Dispose()
    {
        gl.DeleteBuffer(vertexBuffer);
        gl.DeleteVertexArray(vertexArray);
        gl.DeleteProgram(program);
    }

    /// <summary>Uploads the static triangle vertices and records their two vertex attributes in a VAO.</summary>
    private static (GlVertexArrayHandle VertexArray, GlBufferHandle VertexBuffer) CreateGeometry(Gl gl)
    {
        ReadOnlySpan<float> vertices =
        [
            0.5f, -0.5f, 0f, 1f, 0f, 0f,
            -0.5f, -0.5f, 0f, 0f, 1f, 0f,
            0f, 0.5f, 0f, 0f, 0f, 1f,
        ];

        var vertexArray = gl.GenVertexArray();
        var vertexBuffer = gl.GenBuffer();
        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices, GlBufferUsage.StaticDraw);
        gl.VertexAttribPointer(0, 3, GlVertexAttribPointerType.Float, false, VertexStrideBytes, PositionOffsetBytes);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, GlVertexAttribPointerType.Float, false, VertexStrideBytes, ColorOffsetBytes);
        gl.EnableVertexAttribArray(1);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, default);
        gl.BindVertexArray(default);
        return (vertexArray, vertexBuffer);
    }

    /// <summary>Compiles both shaders, links them, and deletes the temporary shader objects.</summary>
    private static GlProgramHandle CreateProgram(Gl gl)
    {
        var vertexShader = CompileShader(gl, GlShaderType.VertexShader, VertexShaderSource);
        var fragmentShader = CompileShader(gl, GlShaderType.FragmentShader, FragmentShaderSource);
        var program = gl.CreateProgram();

        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);
        gl.GetProgramiv(program, GlProgramProperty.LinkStatus, out var linked);

        var linkLog = linked == 0 ? gl.GetProgramInfoLog(program) : null;
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        if (linked != 0)
            return program;

        gl.DeleteProgram(program);
        throw new InvalidOperationException($"Program link failed: {linkLog}");
    }

    /// <summary>Compiles one shader object and deletes it before throwing if the driver rejects the source.</summary>
    private static GlShaderHandle CompileShader(Gl gl, GlShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        gl.GetShaderiv(shader, GlShaderParameterName.CompileStatus, out var compiled);
        if (compiled != 0)
            return shader;

        var log = gl.GetShaderInfoLog(shader);
        gl.DeleteShader(shader);
        throw new InvalidOperationException($"Shader compilation failed: {log}");
    }

    /// <summary>Reads a GLSL shader from the repository root res directory.</summary>
    /// <param name="name">The shader file name under <c>res/shaders/AlvorKit.OpenGL.Demo.HelloTriangle</c>.</param>
    private static string ReadShader(string name)
    {
        var path = Path.Combine(
            ProjectRoot.ResDirectory(typeof(HelloTriangle)),
            "shaders",
            "AlvorKit.OpenGL.Demo.HelloTriangle",
            name);
        return File.ReadAllText(path, Encoding.UTF8);
    }
}
