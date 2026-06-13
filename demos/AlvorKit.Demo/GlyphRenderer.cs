using AlvorKit.OpenGL;

namespace AlvorKit.Demo;

/// <summary>Draws a glyph as a textured quad centered in the window.</summary>
public sealed class GlyphRenderer
{
    private const int VertexCount = 4;
    private const int FloatsPerVertex = 4;
    private const int VertexFloatCount = VertexCount * FloatsPerVertex;
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);
    private const int PositionOffsetBytes = 0;
    private const int UvOffsetBytes = 2 * sizeof(float);

    private const string VertexSource = """
        #version 330 core
        layout(location = 0) in vec2 position;
        layout(location = 1) in vec2 uv;
        out vec2 vertexUv;
        void main() { vertexUv = uv; gl_Position = vec4(position, 0.0, 1.0); }
        """;

    private const string FragmentSource = """
        #version 330 core
        in vec2 vertexUv;
        out vec4 color;
        uniform sampler2D glyph;
        void main() { color = vec4(vec3(texture(glyph, vertexUv).r), 1.0); }
        """;

    private readonly Gl gl;
    private readonly GlyphBitmap glyph;
    private readonly int scale;
    private readonly float[] vertices = new float[VertexFloatCount];
    private readonly uint vertexBuffer;

    public GlyphRenderer(Gl gl, GlyphBitmap glyph, int windowWidth, int windowHeight, int scale)
    {
        this.gl = gl;
        this.glyph = glyph;
        this.scale = scale;

        UploadTexture(glyph);

        var program = CreateProgram();
        gl.UseProgram(program);
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.Uniform1i(gl.GetUniformLocation(program, "glyph"), 0);

        gl.BindVertexArray(gl.GenVertexArray());
        vertexBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
        WriteQuad(windowWidth, windowHeight);
        gl.BufferData<float>(BufferTarget.ArrayBuffer, vertices, BufferUsage.StaticDraw);

        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VertexStrideBytes, PositionOffsetBytes);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VertexStrideBytes, UvOffsetBytes);
        gl.EnableVertexAttribArray(1);

        gl.ClearColor(0.09f, 0.09f, 0.11f, 1);
        gl.Viewport(0, 0, windowWidth, windowHeight);
    }

    public void Draw()
    {
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.DrawArrays(PrimitiveType.TriangleStrip, 0, VertexCount);
    }

    public void Resize(int width, int height)
    {
        gl.Viewport(0, 0, width, height);
        gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
        WriteQuad(width, height);
        gl.BufferData<float>(BufferTarget.ArrayBuffer, vertices, BufferUsage.StaticDraw);
    }

    private void UploadTexture(GlyphBitmap glyph)
    {
        gl.BindTexture(TextureTarget.Texture2D, gl.GenTexture());
        gl.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
        gl.TexImage2D<byte>(TextureTarget.Texture2D, 0, InternalFormat.R8, glyph.Width, glyph.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, glyph.Pixels);
        gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameteri(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
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

        return program;
    }

    private void WriteQuad(int windowWidth, int windowHeight)
    {
        var halfWidth = (float)glyph.Width * scale / windowWidth;
        var halfHeight = (float)glyph.Height * scale / windowHeight;

        vertices[0] = -halfWidth;
        vertices[1] = halfHeight;
        vertices[2] = 0;
        vertices[3] = 0;

        vertices[4] = -halfWidth;
        vertices[5] = -halfHeight;
        vertices[6] = 0;
        vertices[7] = 1;

        vertices[8] = halfWidth;
        vertices[9] = halfHeight;
        vertices[10] = 1;
        vertices[11] = 0;

        vertices[12] = halfWidth;
        vertices[13] = -halfHeight;
        vertices[14] = 1;
        vertices[15] = 1;
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
