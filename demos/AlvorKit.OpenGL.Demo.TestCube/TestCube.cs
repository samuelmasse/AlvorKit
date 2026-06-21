namespace AlvorKit.OpenGL.Demo.TestCube;

/// <summary>Owns the OpenGL layer resources that render a rotating, procedurally textured cube.</summary>
public sealed class TestCube : IDisposable
{
    /// <summary>The width and height, in pixels, of the generated checkerboard texture.</summary>
    private const int TextureSize = 64;

    /// <summary>The number of texels along one checker square.</summary>
    private const int CheckerSize = 8;

    /// <summary>The number of indices submitted by the cube draw call.</summary>
    private const int IndexCount = 36;

    /// <summary>The number of floats in one interleaved position/UV/face-color vertex.</summary>
    private const int FloatsPerVertex = 8;

    /// <summary>The byte stride between adjacent interleaved vertices.</summary>
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);

    /// <summary>The byte offset of the position attribute inside a vertex.</summary>
    private const int PositionOffsetBytes = 0;

    /// <summary>The byte offset of the texture coordinate attribute inside a vertex.</summary>
    private const int TexCoordOffsetBytes = 3 * sizeof(float);

    /// <summary>The byte offset of the face color attribute inside a vertex.</summary>
    private const int FaceColorOffsetBytes = 5 * sizeof(float);

    /// <summary>Vertex shader source for transforming positions and passing UVs through.</summary>
    private static readonly string VertexShaderSource = ReadShader("cube.vert.glsl");

    /// <summary>Fragment shader source for sampling the generated checkerboard texture.</summary>
    private static readonly string FragmentShaderSource = ReadShader("cube.frag.glsl");

    /// <summary>The strict OpenGL layer used for all rendering and resource lifetime calls.</summary>
    private readonly GlLayer gl;

    /// <summary>The linked shader program used by every cube frame.</summary>
    private readonly GlProgramHandle program;

    /// <summary>The uniform location for the model-view-projection matrix.</summary>
    private readonly int modelViewProjectionLocation;

    /// <summary>The vertex array object that captures the cube attribute and index-buffer layout.</summary>
    private readonly GlVertexArrayHandle vertexArray;

    /// <summary>The immutable buffer containing interleaved cube positions and texture coordinates.</summary>
    private readonly GlBufferHandle vertexBuffer;

    /// <summary>The immutable element buffer containing the cube triangle indices.</summary>
    private readonly GlBufferHandle indexBuffer;

    /// <summary>The generated checkerboard texture sampled by the cube shader.</summary>
    private readonly GlTextureHandle texture;

    /// <summary>Captures the OpenGL resources needed by the loaded cube demo.</summary>
    private TestCube(
        GlLayer gl,
        GlProgramHandle program,
        int modelViewProjectionLocation,
        GlVertexArrayHandle vertexArray,
        GlBufferHandle vertexBuffer,
        GlBufferHandle indexBuffer,
        GlTextureHandle texture)
    {
        this.gl = gl;
        this.program = program;
        this.modelViewProjectionLocation = modelViewProjectionLocation;
        this.vertexArray = vertexArray;
        this.vertexBuffer = vertexBuffer;
        this.indexBuffer = indexBuffer;
        this.texture = texture;
    }

    /// <summary>Compiles shaders, uploads geometry and texture data, and returns the cube resource owner.</summary>
    public static TestCube Load(GlLayer gl)
    {
        var program = CreateProgram(gl);
        var modelViewProjectionLocation = gl.GetUniformLocation(program, "uModelViewProjection");
        var textureLocation = gl.GetUniformLocation(program, "uTexture");

        gl.UseProgram(program);
        gl.Uniform1i(textureLocation, 0);
        gl.UnuseProgram();

        var (vertexArray, vertexBuffer, indexBuffer) = CreateGeometry(gl);
        var texture = CreateCheckerTexture(gl);

        return new TestCube(gl, program, modelViewProjectionLocation, vertexArray, vertexBuffer, indexBuffer, texture);
    }

    /// <summary>Draws the cube for the current frame and restores every strict layer binding it touches.</summary>
    public void Render(float elapsedSeconds, int framebufferWidth, int framebufferHeight)
    {
        Span<float> matrix = stackalloc float[Mat4.ComponentCount];
        CreateModelViewProjection(elapsedSeconds, framebufferWidth, framebufferHeight).CopyTo(matrix);

        gl.UseProgram(program);
        gl.UniformMatrix4fv(modelViewProjectionLocation, false, matrix);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.BindVertexArray(vertexArray);
        gl.DrawElements(GlPrimitiveType.Triangles, IndexCount, GlDrawElementsType.UnsignedShort, 0);
        gl.UnbindVertexArray();
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.UnuseProgram();
    }

    /// <summary>Deletes GPU resources once the frame loop has stopped drawing the cube.</summary>
    public void Dispose()
    {
        gl.DeleteVertexArray(vertexArray);
        gl.DeleteBuffer(indexBuffer);
        gl.DeleteBuffer(vertexBuffer);
        gl.DeleteTexture(texture);
        gl.DeleteProgram(program);
    }

    /// <summary>Uploads the cube's indexed vertices and records their two vertex attributes in a VAO.</summary>
    private static (GlVertexArrayHandle VertexArray, GlBufferHandle VertexBuffer, GlBufferHandle IndexBuffer) CreateGeometry(GlLayer gl)
    {
        ReadOnlySpan<float> vertices =
        [
            -0.5f, -0.5f, 0.5f, 0f, 0f, 1.00f, 0.82f, 0.45f,
            0.5f, -0.5f, 0.5f, 1f, 0f, 1.00f, 0.82f, 0.45f,
            0.5f, 0.5f, 0.5f, 1f, 1f, 1.00f, 0.82f, 0.45f,
            -0.5f, 0.5f, 0.5f, 0f, 1f, 1.00f, 0.82f, 0.45f,

            0.5f, -0.5f, -0.5f, 0f, 0f, 0.50f, 0.72f, 1.00f,
            -0.5f, -0.5f, -0.5f, 1f, 0f, 0.50f, 0.72f, 1.00f,
            -0.5f, 0.5f, -0.5f, 1f, 1f, 0.50f, 0.72f, 1.00f,
            0.5f, 0.5f, -0.5f, 0f, 1f, 0.50f, 0.72f, 1.00f,

            -0.5f, -0.5f, -0.5f, 0f, 0f, 0.50f, 0.90f, 0.62f,
            -0.5f, -0.5f, 0.5f, 1f, 0f, 0.50f, 0.90f, 0.62f,
            -0.5f, 0.5f, 0.5f, 1f, 1f, 0.50f, 0.90f, 0.62f,
            -0.5f, 0.5f, -0.5f, 0f, 1f, 0.50f, 0.90f, 0.62f,

            0.5f, -0.5f, 0.5f, 0f, 0f, 0.96f, 0.55f, 0.72f,
            0.5f, -0.5f, -0.5f, 1f, 0f, 0.96f, 0.55f, 0.72f,
            0.5f, 0.5f, -0.5f, 1f, 1f, 0.96f, 0.55f, 0.72f,
            0.5f, 0.5f, 0.5f, 0f, 1f, 0.96f, 0.55f, 0.72f,

            -0.5f, 0.5f, 0.5f, 0f, 0f, 0.80f, 0.70f, 1.00f,
            0.5f, 0.5f, 0.5f, 1f, 0f, 0.80f, 0.70f, 1.00f,
            0.5f, 0.5f, -0.5f, 1f, 1f, 0.80f, 0.70f, 1.00f,
            -0.5f, 0.5f, -0.5f, 0f, 1f, 0.80f, 0.70f, 1.00f,

            -0.5f, -0.5f, -0.5f, 0f, 0f, 0.95f, 0.72f, 0.54f,
            0.5f, -0.5f, -0.5f, 1f, 0f, 0.95f, 0.72f, 0.54f,
            0.5f, -0.5f, 0.5f, 1f, 1f, 0.95f, 0.72f, 0.54f,
            -0.5f, -0.5f, 0.5f, 0f, 1f, 0.95f, 0.72f, 0.54f,
        ];
        ReadOnlySpan<ushort> indices =
        [
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20,
        ];

        var vertexArray = gl.GenVertexArray();
        var vertexBuffer = gl.GenBuffer();
        var indexBuffer = gl.GenBuffer();

        gl.BindVertexArray(vertexArray);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vertexBuffer);
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices, GlBufferUsage.StaticDraw);
        gl.VertexAttribPointer(0, 3, GlVertexAttribPointerType.Float, false, VertexStrideBytes, PositionOffsetBytes);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 2, GlVertexAttribPointerType.Float, false, VertexStrideBytes, TexCoordOffsetBytes);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(2, 3, GlVertexAttribPointerType.Float, false, VertexStrideBytes, FaceColorOffsetBytes);
        gl.EnableVertexAttribArray(2);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, indexBuffer);
        gl.BufferData(GlBufferTarget.ElementArrayBuffer, indices, GlBufferUsage.StaticDraw);
        gl.UnbindVertexArray();

        return (vertexArray, vertexBuffer, indexBuffer);
    }

    /// <summary>Creates and uploads a small RGBA checkerboard texture for the cube faces.</summary>
    private static GlTextureHandle CreateCheckerTexture(GlLayer gl)
    {
        var texture = gl.GenTexture();
        var pixels = CreateCheckerPixels();

        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMinFilter, (int)GlTextureMinFilter.LinearMipmapLinear);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMagFilter, (int)GlTextureMagFilter.Linear);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapS, (int)GlTextureWrapMode.Repeat);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapT, (int)GlTextureWrapMode.Repeat);
        gl.TexImage2D(
            GlTextureTarget.Texture2D,
            0,
            GlInternalFormat.Rgba8,
            TextureSize,
            TextureSize,
            0,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            pixels);
        gl.GenerateMipmap(GlTextureTarget.Texture2D);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();

        return texture;
    }

    /// <summary>Builds the procedural RGBA checkerboard used by the texture upload step.</summary>
    private static byte[] CreateCheckerPixels()
    {
        var pixels = new byte[TextureSize * TextureSize * 4];

        for (var y = 0; y < TextureSize; y++)
        {
            for (var x = 0; x < TextureSize; x++)
            {
                var checker = ((x / CheckerSize) + (y / CheckerSize)) % 2 == 0;
                var verticalStripe = x % CheckerSize == 0;
                var horizontalStripe = y % CheckerSize == 0;
                var offset = ((y * TextureSize) + x) * 4;

                pixels[offset] = checker ? (byte)245 : (byte)35;
                pixels[offset + 1] = checker ? (byte)238 : (byte)88;
                pixels[offset + 2] = checker ? (byte)172 : (byte)154;
                pixels[offset + 3] = verticalStripe || horizontalStripe ? (byte)225 : byte.MaxValue;
            }
        }

        return pixels;
    }

    /// <summary>Creates the current frame's model-view-projection matrix.</summary>
    private static Mat4 CreateModelViewProjection(float elapsedSeconds, int framebufferWidth, int framebufferHeight)
    {
        var aspect = framebufferWidth / (float)framebufferHeight;
        var model =
            Mat4.CreateRotationZ(elapsedSeconds * 0.18f) *
            Mat4.CreateRotationX(0.38f + (elapsedSeconds * 0.35f)) *
            Mat4.CreateRotationY(-0.55f + (elapsedSeconds * 0.7f));
        var view = Mat4.LookAt((0f, 0f, 2.8f), Vec3.Zero, Vec3.UnitY);
        var projection = Mat4.CreatePerspectiveFieldOfView(MathF.PI / 4f, aspect, 0.1f, 100f);
        return projection * view * model;
    }

    /// <summary>Compiles both shaders, links them, and deletes the temporary shader objects.</summary>
    private static GlProgramHandle CreateProgram(GlLayer gl)
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
    private static GlShaderHandle CompileShader(GlLayer gl, GlShaderType type, string source)
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
    private static string ReadShader(string name)
    {
        var path = Path.Combine(
            ProjectRoot.ResDirectory(typeof(TestCube)),
            "shaders",
            "AlvorKit.OpenGL.Demo.TestCube",
            name);
        return File.ReadAllText(path, Encoding.UTF8);
    }
}
