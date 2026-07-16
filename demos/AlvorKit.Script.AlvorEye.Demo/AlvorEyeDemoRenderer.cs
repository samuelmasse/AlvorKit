namespace AlvorKit.Script.AlvorEye.Demo;

/// <summary>Renders the simple colored game board used by the AlvorEye demo.</summary>
public sealed class AlvorEyeDemoRenderer : IDisposable
{
    /// <summary>Maximum number of colored rectangles emitted by one frame.</summary>
    private const int MaxRectangles = 48;

    /// <summary>Number of vertices emitted for one rectangle.</summary>
    private const int VerticesPerRectangle = 6;

    /// <summary>Number of floats in one position/color vertex.</summary>
    private const int FloatsPerVertex = 5;

    /// <summary>Stride between adjacent vertices in bytes.</summary>
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);

    /// <summary>Vertex shader source used by the rectangle renderer.</summary>
    private static readonly string VertexShaderSource = ReadShader("alvoreye-demo.vert.glsl");

    /// <summary>Fragment shader source used by the rectangle renderer.</summary>
    private static readonly string FragmentShaderSource = ReadShader("alvoreye-demo.frag.glsl");

    /// <summary>The OpenGL command surface used by this renderer.</summary>
    private readonly GlLayer gl;

    /// <summary>Linked shader program for colored rectangles.</summary>
    private readonly GlProgramHandle program;

    /// <summary>Vertex array that records the rectangle vertex layout.</summary>
    private readonly GlVertexArrayHandle vertexArray;

    /// <summary>Dynamic buffer receiving each frame's rectangles.</summary>
    private readonly GlBufferHandle vertexBuffer;

    /// <summary>Captures the OpenGL resources owned by the renderer.</summary>
    private AlvorEyeDemoRenderer(GlLayer gl, GlProgramHandle program, GlVertexArrayHandle vertexArray, GlBufferHandle vertexBuffer)
    {
        this.gl = gl;
        this.program = program;
        this.vertexArray = vertexArray;
        this.vertexBuffer = vertexBuffer;
    }

    /// <summary>Compiles shaders, creates the dynamic vertex buffer, and returns the renderer.</summary>
    public static AlvorEyeDemoRenderer Load(GlLayer gl)
    {
        var program = CreateProgram(gl);
        var vertexArray = gl.GenVertexArray();
        var vertexBuffer = gl.GenBuffer();
        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, MaxRectangles * VerticesPerRectangle * VertexStrideBytes, 0, GlBufferUsage.DynamicDraw);
        gl.VertexAttribPointer<Vec2>(0, false, VertexStrideBytes, 0);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer<Vec3>(1, false, VertexStrideBytes, 2 * sizeof(float));
        gl.EnableVertexAttribArray(1);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
        return new(gl, program, vertexArray, vertexBuffer);
    }

    /// <summary>Draws the whole game board for the current frame.</summary>
    public void Render(AlvorEyeDemoState state, float elapsedSeconds, int framebufferWidth, int framebufferHeight)
    {
        Span<float> vertices = stackalloc float[MaxRectangles * VerticesPerRectangle * FloatsPerVertex];
        var cursor = 0;
        AddBoard(vertices, ref cursor, state, elapsedSeconds);

        gl.UseProgram(program);
        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices[..cursor], GlBufferUsage.DynamicDraw);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, cursor / FloatsPerVertex);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
    }

    /// <summary>Deletes GPU resources owned by the renderer.</summary>
    public void Dispose()
    {
        gl.DeleteBuffer(vertexBuffer);
        gl.DeleteVertexArray(vertexArray);
        gl.DeleteProgram(program);
    }

    /// <summary>Adds every visible game object to the frame vertex buffer.</summary>
    private static void AddBoard(Span<float> vertices, ref int cursor, AlvorEyeDemoState state, float elapsedSeconds)
    {
        AddRect(vertices, ref cursor, 24f, 56f, 580f, 550f, 0.105f, 0.125f, 0.145f);
        AddRect(vertices, ref cursor, 632f, 56f, 244f, 550f, 0.095f, 0.095f, 0.115f);
        AddRect(vertices, ref cursor, 40f + elapsedSeconds % 3.2f / 3.2f * 520f, 70f, 42f, 8f, 0.5f, 0.95f, 1f);
        AddRect(vertices, ref cursor, 116f, 96f, 56f, 56f, state.HasKey ? 0.15f : 0.03f, state.HasKey ? 0.9f : 0.35f, 0.18f);
        AddRect(vertices, ref cursor, 514f, 84f, 64f, 96f, state.AllLocksComplete ? 0.95f : 0.35f, 0.95f, 0.95f);
        AddRect(vertices, ref cursor, state.PlayerX, state.PlayerY, AlvorEyeDemoState.PlayerSize, AlvorEyeDemoState.PlayerSize, 0.18f, 0.48f, 1f);
        AddPanel(vertices, ref cursor, state);
        if (state.Won)
            AddRect(vertices, ref cursor, 202f, 264f, 224f, 100f, 0.18f, 0.9f, 0.36f);
    }

    /// <summary>Adds the right-side interaction panel and progress indicators.</summary>
    private static void AddPanel(Span<float> vertices, ref int cursor, AlvorEyeDemoState state)
    {
        AddProgress(vertices, ref cursor, 666f, 86f, state.HasKey);
        AddProgress(vertices, ref cursor, 716f, 86f, state.ButtonPressed);
        AddProgress(vertices, ref cursor, 766f, 86f, state.SliderComplete);
        AddProgress(vertices, ref cursor, 816f, 86f, state.CodeProgress == 3);
        AddRect(vertices, ref cursor, 696f, 126f, 110f, 70f, state.ButtonPressed ? 1f : 0.85f, state.ButtonPressed ? 0.92f : 0.62f, 0.08f);
        AddRect(vertices, ref cursor, 674f, 298f, 202f, 18f, 0.75f, 0.16f, 0.86f);
        AddRect(vertices, ref cursor, 670f + state.SliderValue * 166f, 280f, 38f, 54f, 0.1f, 0.85f, 0.95f);
        for (var index = 0; index < 3; index++)
            AddRect(vertices, ref cursor, 690f + index * 54f, 398f, 38f, 38f, index < state.CodeProgress ? 0.95f : 0.22f, 0.46f, 0.15f);
        AddRect(vertices, ref cursor, state.MouseX - 5f, state.MouseY - 5f, 10f, 10f, 1f, 1f, 1f);
    }

    /// <summary>Adds one progress light.</summary>
    private static void AddProgress(Span<float> vertices, ref int cursor, float x, float y, bool complete) =>
        AddRect(vertices, ref cursor, x, y, 32f, 32f, complete ? 0.18f : 0.18f, complete ? 0.95f : 0.18f, complete ? 0.28f : 0.22f);

    /// <summary>Adds one screen-space rectangle to the dynamic vertex buffer.</summary>
    private static void AddRect(Span<float> vertices, ref int cursor, float x, float y, float width, float height, float r, float g, float b)
    {
        var left = ToNdcX(x);
        var right = ToNdcX(x + width);
        var top = ToNdcY(y);
        var bottom = ToNdcY(y + height);
        AddVertex(vertices, ref cursor, left, top, r, g, b);
        AddVertex(vertices, ref cursor, right, top, r, g, b);
        AddVertex(vertices, ref cursor, right, bottom, r, g, b);
        AddVertex(vertices, ref cursor, left, top, r, g, b);
        AddVertex(vertices, ref cursor, right, bottom, r, g, b);
        AddVertex(vertices, ref cursor, left, bottom, r, g, b);
    }

    /// <summary>Adds one position/color vertex to the frame buffer.</summary>
    private static void AddVertex(Span<float> vertices, ref int cursor, float x, float y, float r, float g, float b)
    {
        vertices[cursor++] = x;
        vertices[cursor++] = y;
        vertices[cursor++] = r;
        vertices[cursor++] = g;
        vertices[cursor++] = b;
    }

    /// <summary>Converts a client x coordinate into normalized device coordinates.</summary>
    private static float ToNdcX(float x) => x / AlvorEyeDemoState.GameWidth * 2f - 1f;

    /// <summary>Converts a client y coordinate into normalized device coordinates.</summary>
    private static float ToNdcY(float y) => 1f - y / AlvorEyeDemoState.GameHeight * 2f;

    /// <summary>Compiles both shaders, links them, and deletes temporary shader objects.</summary>
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

    /// <summary>Compiles one shader and reports the driver log when compilation fails.</summary>
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

    /// <summary>Reads a shader from the repository resource directory.</summary>
    private static string ReadShader(string name)
    {
        var path = Path.Combine(ProjectRoot.ResDirectory(typeof(AlvorEyeDemoRenderer)), "shaders", "AlvorKit.Script.AlvorEye.Demo", name);
        return File.ReadAllText(path, Encoding.UTF8);
    }
}
