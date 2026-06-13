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
    private readonly uint texture;
    private readonly uint program;
    private readonly uint vertexArray;
    private readonly uint vertexBuffer;
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
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.BindVertexArray(vertexArray);

        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.DrawArrays(PrimitiveType.TriangleStrip, 0, VertexCount);

        gl.UnbindVertexArray();
        gl.UnbindTexture(TextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.UnuseProgram();
        gl.ResetClearColor();
        gl.ResetViewport();
    }

    private uint CreateTexture(GlyphBitmap glyph)
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
        gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.R8, glyph.Width, glyph.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, glyph.Pixels);
        gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        gl.ResetPixelStore(PixelStoreParameter.UnpackAlignment);
        gl.UnbindTexture(TextureTarget.Texture2D);
        gl.ResetActiveTexture();
        return texture;
    }

    private uint CreateProgram()
    {
        var program = gl.CreateProgram();
        gl.AttachShader(program, Compile(ShaderType.VertexShader, VertexSource));
        gl.AttachShader(program, Compile(ShaderType.FragmentShader, FragmentSource));
        gl.LinkProgram(program);

        gl.GetProgramiv(program, ProgramProperty.LinkStatus, out var linkStatus);
        if (linkStatus == 0)
            throw new InvalidOperationException($"Program link failed: {gl.GetProgramInfoLog(program)}");

        gl.UseProgram(program);
        gl.Uniform1i(gl.GetUniformLocation(program, "glyph"), 0);
        gl.UnuseProgram();
        return program;
    }

    private (uint VertexArray, uint VertexBuffer) CreateGeometry()
    {
        var vertexArray = gl.GenVertexArray();
        var vertexBuffer = gl.GenBuffer();

        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData<float>(BufferTarget.ArrayBuffer, UnitQuad, BufferUsage.StaticDraw);
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VertexStrideBytes, PositionOffsetBytes);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VertexStrideBytes, UvOffsetBytes);
        gl.EnableVertexAttribArray(1);
        gl.UnbindBuffer(BufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();
        return (vertexArray, vertexBuffer);
    }

    private uint Compile(ShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShaderiv(shader, ShaderParameterName.CompileStatus, out var compileStatus);
        if (compileStatus == 0)
            throw new InvalidOperationException($"Shader compilation failed: {gl.GetShaderInfoLog(shader)}");

        return shader;
    }
}
