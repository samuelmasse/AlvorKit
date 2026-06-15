using AlvorKit.OpenGL;
using AlvorKit.OpenGL.Layer;

namespace AlvorKit.Demo;

/// <summary>
/// Draws a glyph as a textured quad. Resources are created once; every per-frame bind and state-set is
/// paired with its unbind/reset inside <see cref="Draw"/>, so the strict layer sees a clean slate each
/// frame and nothing lingers between frames.
/// </summary>
public sealed class GlyphRenderer
{
    private const int VertexCount = 4;
    private const int FloatsPerVertex = 4;
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);
    private const int PositionOffsetBytes = 0;
    private const int UvOffsetBytes = 2 * sizeof(float);

    private static readonly float[] UnitQuad =
    [
        -1f,  1f, 0f, 0f,
        -1f, -1f, 0f, 1f,
         1f,  1f, 1f, 0f,
         1f, -1f, 1f, 1f,
    ];

    private static readonly string VertexSource = ReadShader("glyph.vert.glsl");
    private static readonly string FragmentSource = ReadShader("glyph.frag.glsl");

    private readonly GlLayer gl;
    private readonly GlyphBitmap glyph;
    private readonly int scale;
    private readonly GlTextureHandle texture;
    private readonly GlProgramHandle program;
    private readonly GlVertexArrayHandle vertexArray;
    private readonly GlBufferHandle vertexBuffer;
    private readonly int scaleLocation;

    public GlyphRenderer(GlLayer gl, GlyphBitmap glyph, int scale)
    {
        this.gl = gl;
        this.glyph = glyph;
        this.scale = scale;

        texture = CreateTexture(glyph);
        program = CreateProgram();
        scaleLocation = gl.GetUniformLocation(program, "uScale");
        (vertexArray, vertexBuffer) = CreateGeometry();
    }

    /// <summary>Draws one frame of the glyph without allocating in the render path.</summary>
    /// <param name="width">The current framebuffer width.</param>
    /// <param name="height">The current framebuffer height.</param>
    public void Draw(int width, int height)
    {
        gl.Viewport(0, 0, width, height);
        gl.ClearColor(0.09f, 0.09f, 0.11f, 1f);
        gl.UseProgram(program);
        gl.Uniform2f(scaleLocation, (float)glyph.Width * scale / width, (float)glyph.Height * scale / height);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.BindVertexArray(vertexArray);

        gl.Clear(GlClearBufferMask.ColorBufferBit);
        gl.DrawArrays(GlPrimitiveType.TriangleStrip, 0, VertexCount);

        gl.UnbindVertexArray();
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.UnuseProgram();
        gl.ResetClearColor();
        gl.ResetViewport();
    }

    /// <summary>Uploads the grayscale glyph to a single-channel OpenGL texture.</summary>
    /// <param name="glyph">The glyph bitmap copied from FreeType during startup.</param>
    private GlTextureHandle CreateTexture(GlyphBitmap glyph)
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 1);
        gl.TexImage2D<byte>(GlTextureTarget.Texture2D, 0, GlInternalFormat.R8, glyph.Width, glyph.Height, 0, GlPixelFormat.Red, GlPixelType.UnsignedByte, glyph.Pixels);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMinFilter, (int)GlTextureMinFilter.Linear);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMagFilter, (int)GlTextureMagFilter.Linear);
        gl.ResetPixelStore(GlPixelStoreParameter.UnpackAlignment);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        return texture;
    }

    /// <summary>Creates the shader program used to draw the textured glyph quad.</summary>
    private GlProgramHandle CreateProgram()
    {
        var program = gl.CreateProgram();
        gl.AttachShader(program, Compile(GlShaderType.VertexShader, VertexSource));
        gl.AttachShader(program, Compile(GlShaderType.FragmentShader, FragmentSource));
        gl.LinkProgram(program);
        gl.UseProgram(program);
        gl.Uniform1i(gl.GetUniformLocation(program, "glyph"), 0);
        gl.UnuseProgram();
        return program;
    }

    /// <summary>Creates the vertex array and buffer for a full-size triangle-strip quad.</summary>
    private (GlVertexArrayHandle VertexArray, GlBufferHandle VertexBuffer) CreateGeometry()
    {
        var vertexArray = gl.GenVertexArray();
        var vertexBuffer = gl.GenBuffer();

        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData<float>(GlBufferTarget.ArrayBuffer, UnitQuad, GlBufferUsage.StaticDraw);
        gl.VertexAttribPointer(0, 2, GlVertexAttribPointerType.Float, false, VertexStrideBytes, PositionOffsetBytes);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, GlVertexAttribPointerType.Float, false, VertexStrideBytes, UvOffsetBytes);
        gl.EnableVertexAttribArray(1);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
        return (vertexArray, vertexBuffer);
    }

    /// <summary>Compiles a shader from demo source.</summary>
    /// <param name="type">The shader stage to compile.</param>
    /// <param name="source">The GLSL source read from the repository root res directory.</param>
    private GlShaderHandle Compile(GlShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        return shader;
    }

    /// <summary>Reads a GLSL shader from the repository root res directory.</summary>
    /// <param name="name">The shader file name under <c>res/shaders/AlvorKit.Demo</c>.</param>
    private static string ReadShader(string name)
    {
        var path = Path.Combine(FindRepositoryRoot(), "res", "shaders", "AlvorKit.Demo", name);
        return File.ReadAllText(path, Encoding.UTF8);
    }

    /// <summary>Finds the nearest repository root by walking upward from likely demo execution directories.</summary>
    private static string FindRepositoryRoot()
    {
        foreach (var start in CandidateDirectories())
        {
            for (var current = start; current is not null; current = Directory.GetParent(current)?.FullName)
            {
                if (File.Exists(Path.Combine(current, "AlvorKit.slnx")) && Directory.Exists(Path.Combine(current, "res")))
                    return current;
            }
        }

        throw new InvalidOperationException("Could not find the AlvorKit repository root containing the res directory.");
    }

    /// <summary>Returns likely starting directories for repository root discovery.</summary>
    private static IEnumerable<string> CandidateDirectories()
    {
        if (!string.IsNullOrWhiteSpace(Environment.CurrentDirectory))
            yield return Environment.CurrentDirectory;

        if (!string.IsNullOrWhiteSpace(AppContext.BaseDirectory))
            yield return AppContext.BaseDirectory;
    }
}
