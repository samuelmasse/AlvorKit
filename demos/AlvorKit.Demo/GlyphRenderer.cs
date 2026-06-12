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

    private readonly GlyphBitmap glyph;
    private readonly int scale;
    private readonly float[] vertices = new float[VertexFloatCount];
    private readonly uint vertexBuffer;

    public GlyphRenderer(GlyphBitmap glyph, int windowWidth, int windowHeight, int scale)
    {
        this.glyph = glyph;
        this.scale = scale;

        UploadTexture(glyph);

        var program = CreateProgram();
        Gl.UseProgram(program);
        Gl.ActiveTexture(Gl.Texture0);
        Gl.Uniform1i(Gl.GetUniformLocation(program, "glyph"), 0);

        Gl.BindVertexArray(Gl.GenVertexArray());
        vertexBuffer = Gl.GenBuffer();
        Gl.BindBuffer(Gl.ArrayBuffer, vertexBuffer);
        WriteQuad(windowWidth, windowHeight);
        Gl.BufferData(Gl.ArrayBuffer, vertices, Gl.StaticDraw);

        Gl.VertexAttribPointer(0, 2, Gl.Float, false, VertexStrideBytes, PositionOffsetBytes);
        Gl.EnableVertexAttribArray(0);
        Gl.VertexAttribPointer(1, 2, Gl.Float, false, VertexStrideBytes, UvOffsetBytes);
        Gl.EnableVertexAttribArray(1);

        Gl.ClearColor(0.09f, 0.09f, 0.11f, 1);
        Gl.Viewport(0, 0, windowWidth, windowHeight);
    }

    public void Draw()
    {
        Gl.Clear(Gl.ColorBufferBit);
        Gl.DrawArrays(Gl.TriangleStrip, 0, VertexCount);
    }

    public void Resize(int width, int height)
    {
        Gl.Viewport(0, 0, width, height);
        Gl.BindBuffer(Gl.ArrayBuffer, vertexBuffer);
        WriteQuad(width, height);
        Gl.BufferData(Gl.ArrayBuffer, vertices, Gl.StaticDraw);
    }

    private static void UploadTexture(GlyphBitmap glyph)
    {
        Gl.BindTexture(Gl.Texture2D, Gl.GenTexture());
        Gl.PixelStorei(Gl.UnpackAlignment, 1);
        Gl.TexImage2D(Gl.Texture2D, 0, Gl.R8, glyph.Width, glyph.Height, Gl.Red, Gl.UnsignedByte, glyph.Pixels);
        Gl.TexParameteri(Gl.Texture2D, Gl.TextureMinFilter, (int)Gl.Linear);
        Gl.TexParameteri(Gl.Texture2D, Gl.TextureMagFilter, (int)Gl.Linear);
    }

    private static uint CreateProgram()
    {
        var program = Gl.CreateProgram();
        Gl.AttachShader(program, Compile(Gl.VertexShader, VertexSource));
        Gl.AttachShader(program, Compile(Gl.FragmentShader, FragmentSource));
        Gl.LinkProgram(program);

        if (Gl.GetProgram(program, Gl.LinkStatus) == 0)
            throw new InvalidOperationException($"Program link failed: {Gl.GetProgramInfoLog(program)}");

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

    private static uint Compile(uint type, string source)
    {
        var shader = Gl.CreateShader(type);
        Gl.ShaderSource(shader, source);
        Gl.CompileShader(shader);

        if (Gl.GetShader(shader, Gl.CompileStatus) == 0)
            throw new InvalidOperationException($"Shader compilation failed: {Gl.GetShaderInfoLog(shader)}");

        return shader;
    }
}
