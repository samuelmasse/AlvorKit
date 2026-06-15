namespace AlvorKit.Demo;

/// <summary>
/// Draws a glyph as a textured quad. Resources are created once; every per-frame bind and state-set is
/// paired with its unbind/reset inside <see cref="Draw"/>, so the strict layer sees a clean slate each
/// frame and nothing lingers between frames.
/// </summary>
public sealed class GlyphRenderer
{
    /// <summary>The number of vertices in the triangle-strip quad.</summary>
    private const int VertexCount = 4;

    /// <summary>The number of floating-point values in each interleaved vertex.</summary>
    private const int FloatsPerVertex = 4;

    /// <summary>The byte stride between adjacent interleaved vertices.</summary>
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);

    /// <summary>The byte offset of the clip-space position inside each vertex.</summary>
    private const int PositionOffsetBytes = 0;

    /// <summary>The byte offset of the texture coordinate inside each vertex.</summary>
    private const int UvOffsetBytes = 2 * sizeof(float);

    /// <summary>A full-size quad with clip-space coordinates followed by glyph texture coordinates.</summary>
    private static readonly float[] UnitQuad =
    [
        -1f,  1f, 0f, 0f,
        -1f, -1f, 0f, 1f,
         1f,  1f, 1f, 0f,
         1f, -1f, 1f, 1f,
    ];

    /// <summary>The vertex shader source read during startup, before the render loop begins.</summary>
    private static readonly string VertexSource = ReadShader("glyph.vert.glsl");

    /// <summary>The fragment shader source read during startup, before the render loop begins.</summary>
    private static readonly string FragmentSource = ReadShader("glyph.frag.glsl");

    /// <summary>The strict OpenGL layer that tracks state and resource lifetime for the demo.</summary>
    private readonly GlLayer gl;

    /// <summary>The managed glyph bitmap used to compute the on-screen scale each frame.</summary>
    private readonly GlyphBitmap glyph;

    /// <summary>The display scale applied to the glyph bitmap dimensions.</summary>
    private readonly int scale;

    /// <summary>The OpenGL texture containing the grayscale glyph alpha mask.</summary>
    private readonly GlTextureHandle texture;

    /// <summary>The linked shader program used for the textured quad.</summary>
    private readonly GlProgramHandle program;

    /// <summary>The vertex array that records the glyph quad attribute layout.</summary>
    private readonly GlVertexArrayHandle vertexArray;

    /// <summary>The static vertex buffer containing the glyph quad.</summary>
    private readonly GlBufferHandle vertexBuffer;

    /// <summary>The cached uniform location for the normalized glyph scale.</summary>
    private readonly int scaleLocation;

    /// <summary>Uploads the glyph texture and creates the fixed shader and quad resources.</summary>
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
    private GlTextureHandle CreateTexture(GlyphBitmap glyph)
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 1);
        gl.TexImage2D<byte>(
            GlTextureTarget.Texture2D,
            0,
            GlInternalFormat.R8,
            glyph.Width,
            glyph.Height,
            0,
            GlPixelFormat.Red,
            GlPixelType.UnsignedByte,
            glyph.Pixels);
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
    private GlShaderHandle Compile(GlShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        return shader;
    }

    /// <summary>Reads a GLSL shader from the repository root res directory.</summary>
    private static string ReadShader(string name)
    {
        var path = Path.Combine(ProjectRoot.ResDirectory(typeof(GlyphRenderer)), "shaders", "AlvorKit.Demo", name);
        return File.ReadAllText(path, Encoding.UTF8);
    }
}
