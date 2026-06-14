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

    private const string VertexSource = """
        #version 330 core
        layout(location = 0) in vec2 position;
        layout(location = 1) in vec2 uv;
        out vec2 vertexUv;
        uniform vec2 uScale;
        void main() { vertexUv = uv; gl_Position = vec4(position * uScale, 0.0, 1.0); }
        """;

    private const string FragmentSource = """
        #version 330 core
        in vec2 vertexUv;
        out vec4 color;
        uniform sampler2D glyph;
        void main() { color = vec4(vec3(texture(glyph, vertexUv).r), 1.0); }
        """;

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

    private GlProgramHandle CreateProgram()
    {
        var program = gl.CreateProgram();
        gl.AttachShader(program, Compile(GlShaderType.VertexShader, VertexSource));
        gl.AttachShader(program, Compile(GlShaderType.FragmentShader, FragmentSource));
        gl.LinkProgram(program);

        gl.GetProgramiv(program, GlProgramProperty.LinkStatus, out var linkStatus);
        if (linkStatus == 0)
            throw new InvalidOperationException($"Program link failed: {gl.GetProgramInfoLog(program)}");

        gl.UseProgram(program);
        gl.Uniform1i(gl.GetUniformLocation(program, "glyph"), 0);
        gl.UnuseProgram();
        return program;
    }

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

    private GlShaderHandle Compile(GlShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShaderiv(shader, GlShaderParameterName.CompileStatus, out var compileStatus);
        if (compileStatus == 0)
            throw new InvalidOperationException($"Shader compilation failed: {gl.GetShaderInfoLog(shader)}");

        return shader;
    }
}
