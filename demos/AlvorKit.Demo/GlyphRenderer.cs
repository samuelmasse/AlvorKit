using AlvorKit.OpenGL;

namespace AlvorKit.Demo;

/// <summary>Draws a glyph as a textured quad centered in the window.</summary>
public class GlyphRenderer
{
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

    public GlyphRenderer(Glyph glyph, int windowWidth, int windowHeight, int scale)
    {
        Gl.BindTexture(Gl.Texture2D, Gl.GenTexture());
        Gl.PixelStorei(Gl.UnpackAlignment, 1);
        Gl.TexImage2D(Gl.Texture2D, 0, Gl.R8, glyph.Width, glyph.Height, Gl.Red, Gl.UnsignedByte, glyph.Pixels);
        Gl.TexParameteri(Gl.Texture2D, Gl.TextureMinFilter, (int)Gl.Linear);
        Gl.TexParameteri(Gl.Texture2D, Gl.TextureMagFilter, (int)Gl.Linear);

        var program = Gl.CreateProgram();
        Gl.AttachShader(program, Compile(Gl.VertexShader, VertexSource));
        Gl.AttachShader(program, Compile(Gl.FragmentShader, FragmentSource));
        Gl.LinkProgram(program);
        if (Gl.GetProgram(program, Gl.LinkStatus) == 0)
            throw new InvalidOperationException($"Program link failed: {Gl.GetProgramInfoLog(program)}");
        Gl.UseProgram(program);
        Gl.ActiveTexture(Gl.Texture0);
        Gl.Uniform1i(Gl.GetUniformLocation(program, "glyph"), 0);

        // A centered quad as a triangle strip of (position, uv).
        var halfWidth = (float)glyph.Width * scale / windowWidth;
        var halfHeight = (float)glyph.Height * scale / windowHeight;
        Gl.BindVertexArray(Gl.GenVertexArray());
        Gl.BindBuffer(Gl.ArrayBuffer, Gl.GenBuffer());
        Gl.BufferData(Gl.ArrayBuffer, [
            -halfWidth, halfHeight, 0, 0,
            -halfWidth, -halfHeight, 0, 1,
            halfWidth, halfHeight, 1, 0,
            halfWidth, -halfHeight, 1, 1
        ], Gl.StaticDraw);
        Gl.VertexAttribPointer(0, 2, Gl.Float, false, 4 * sizeof(float), 0);
        Gl.EnableVertexAttribArray(0);
        Gl.VertexAttribPointer(1, 2, Gl.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        Gl.EnableVertexAttribArray(1);

        Gl.ClearColor(0.09f, 0.09f, 0.11f, 1);
    }

    public void Draw()
    {
        Gl.Clear(Gl.ColorBufferBit);
        Gl.DrawArrays(Gl.TriangleStrip, 0, 4);
    }

    public void Resize(int width, int height) => Gl.Viewport(0, 0, width, height);

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
