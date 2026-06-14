namespace AlvorKit.OpenGL.Demo.HelloTriangle;

/// <summary>The classic coloured triangle, driven directly through the raw <see cref="Gl"/> surface.</summary>
public sealed class HelloTriangle(Gl gl)
{
    private const string VertexShaderSource =
        """
        #version 330 core
        layout (location = 0) in vec3 aPos;
        layout (location = 1) in vec3 aColor;
        out vec3 ourColor;
        void main()
        {
            gl_Position = vec4(aPos, 1.0);
            ourColor = aColor;
        }
        """;

    private const string FragmentShaderSource =
        """
        #version 330 core
        in vec3 ourColor;
        out vec4 FragColor;
        void main()
        {
            FragColor = vec4(ourColor, 1.0);
        }
        """;

    private GlProgramHandle program;
    private GlVertexArrayHandle vertexArray;

    public void Load()
    {
        var vertexShader = Compile(GlShaderType.VertexShader, VertexShaderSource);
        var fragmentShader = Compile(GlShaderType.FragmentShader, FragmentShaderSource);

        program = gl.CreateProgram();
        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);
        gl.LinkProgram(program);
        gl.GetProgramiv(program, GlProgramProperty.LinkStatus, out var linked);
        if (linked == 0)
            throw new InvalidOperationException(gl.GetProgramInfoLog(program));

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        ReadOnlySpan<float> vertices =
        [
            0.5f, -0.5f, 0f, 1f, 0f, 0f,
            -0.5f, -0.5f, 0f, 0f, 1f, 0f,
            0f, 0.5f, 0f, 0f, 0f, 1f,
        ];

        var vertexBuffer = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices, GlBufferUsage.StaticDraw);

        vertexArray = gl.GenVertexArray();
        gl.BindVertexArray(vertexArray);
        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(0, 3, GlVertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        gl.VertexAttribPointer(1, 3, GlVertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

        gl.BindVertexArray(default);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, default);
    }

    public void Render()
    {
        gl.UseProgram(program);
        gl.BindVertexArray(vertexArray);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, 3);
        gl.BindVertexArray(default);
        gl.UseProgram(default);
    }

    private GlShaderHandle Compile(GlShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        gl.GetShaderiv(shader, GlShaderParameterName.CompileStatus, out var compiled);
        if (compiled == 0)
            throw new InvalidOperationException(gl.GetShaderInfoLog(shader));
        return shader;
    }
}
