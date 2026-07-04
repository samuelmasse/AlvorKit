namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Owns the OpenGL resources that display the checked-in azure tentacle monster GLB.</summary>
public sealed class GlbModel : IDisposable
{
    /// <summary>The GLB file copied into the repository resource tree for this demo.</summary>
    private const string ModelFileName = "azure_tentacle_monster_tex256.glb";

    /// <summary>The shared resource subdirectory used by this demo's shaders and model asset.</summary>
    private const string ResourceDirectoryName = "AlvorKit.OpenGL.Demo.AzureTentacle";

    /// <summary>The number of floats in one interleaved position/UV/joints/weights vertex.</summary>
    private const int FloatsPerVertex = 13;

    /// <summary>The byte stride between adjacent interleaved model vertices.</summary>
    private const int VertexStrideBytes = FloatsPerVertex * sizeof(float);

    /// <summary>The byte offset of the position attribute inside one model vertex.</summary>
    private const int PositionOffsetBytes = 0;

    /// <summary>The byte offset of the texture coordinate attribute inside one model vertex.</summary>
    private const int TexCoordOffsetBytes = 3 * sizeof(float);

    /// <summary>The byte offset of the four joint indices inside one model vertex.</summary>
    private const int JointsOffsetBytes = 5 * sizeof(float);

    /// <summary>The byte offset of the four joint weights inside one model vertex.</summary>
    private const int WeightsOffsetBytes = 9 * sizeof(float);

    /// <summary>The target height used to keep the asset comfortably framed in the window.</summary>
    private const float DisplayHeight = 1.85f;

    /// <summary>Vertex shader source for skinning the animated GLB mesh and passing texture coordinates through.</summary>
    private static readonly string VertexShaderSource = ReadShader("glb-model.vert.glsl");

    /// <summary>Fragment shader source for flat texture sampling.</summary>
    private static readonly string FragmentShaderSource = ReadShader("glb-model.frag.glsl");

    /// <summary>The strict OpenGL layer used for all rendering and resource lifetime calls.</summary>
    private readonly GlLayer gl;

    /// <summary>The linked shader program used by every model frame.</summary>
    private readonly GlProgramHandle program;

    /// <summary>The uniform location for the model-view-projection matrix.</summary>
    private readonly int modelViewProjectionLocation;

    /// <summary>The uniform location for the first matrix in the skinning joint palette.</summary>
    private readonly int jointMatricesLocation;

    /// <summary>The vertex array object that captures the GLB attribute and index-buffer layout.</summary>
    private readonly GlVertexArrayHandle vertexArray;

    /// <summary>The immutable buffer containing interleaved model positions, texture coordinates, joints, and weights.</summary>
    private readonly GlBufferHandle vertexBuffer;

    /// <summary>The immutable element buffer containing model triangle indices.</summary>
    private readonly GlBufferHandle indexBuffer;

    /// <summary>The decoded and uploaded base color texture referenced by the GLB material.</summary>
    private readonly GlTextureHandle texture;

    /// <summary>The decoded base color texture width in pixels.</summary>
    private readonly int textureWidth;

    /// <summary>The decoded base color texture height in pixels.</summary>
    private readonly int textureHeight;

    /// <summary>The number of unsigned-int indices submitted by the model draw call.</summary>
    private readonly int indexCount;

    /// <summary>The loaded skinned mesh data and sampled animation state.</summary>
    private readonly AnimatedGlbMesh mesh;

    /// <summary>The model-space center used to normalize the mesh into the demo camera frame.</summary>
    private readonly Vec3 boundsCenter;

    /// <summary>The uniform scale used to normalize the mesh into the demo camera frame.</summary>
    private readonly float modelScale;

    /// <summary>Captures the OpenGL resources and bounds needed by the loaded animated GLB.</summary>
    private GlbModel(
        GlLayer gl,
        GlProgramHandle program,
        int modelViewProjectionLocation,
        int jointMatricesLocation,
        GlVertexArrayHandle vertexArray,
        GlBufferHandle vertexBuffer,
        GlBufferHandle indexBuffer,
        GlTextureHandle texture,
        int textureWidth,
        int textureHeight,
        int indexCount,
        AnimatedGlbMesh mesh,
        Vec3 boundsCenter,
        float modelScale)
    {
        this.gl = gl;
        this.program = program;
        this.modelViewProjectionLocation = modelViewProjectionLocation;
        this.jointMatricesLocation = jointMatricesLocation;
        this.vertexArray = vertexArray;
        this.vertexBuffer = vertexBuffer;
        this.indexBuffer = indexBuffer;
        this.texture = texture;
        this.textureWidth = textureWidth;
        this.textureHeight = textureHeight;
        this.indexCount = indexCount;
        this.mesh = mesh;
        this.boundsCenter = boundsCenter;
        this.modelScale = modelScale;
    }

    /// <summary>Gets the number of vertices loaded for the skinned mesh.</summary>
    public int VertexCount => mesh.VertexCount;

    /// <summary>Gets the number of triangles rendered by the model draw call.</summary>
    public int TriangleCount => mesh.TriangleCount;

    /// <summary>Gets the width of the decoded base color texture in pixels.</summary>
    public int TextureWidth => textureWidth;

    /// <summary>Gets the height of the decoded base color texture in pixels.</summary>
    public int TextureHeight => textureHeight;

    /// <summary>Gets the number of joints in the loaded skin.</summary>
    public int JointCount => mesh.JointCount;

    /// <summary>Gets the number of selectable animation slots, including the base-pose slot.</summary>
    public int AnimationCount => mesh.AnimationCount;

    /// <summary>Gets the currently selected animation slot index.</summary>
    public int SelectedAnimationIndex => mesh.SelectedAnimationIndex;

    /// <summary>Loads the GLB asset, decodes its base color texture, uploads GPU resources, and returns the model owner.</summary>
    public static GlbModel Load(GlLayer gl)
    {
        var path = Path.Combine(
            ProjectRoot.ResDirectory(typeof(GlbModel)),
            "models",
            ResourceDirectoryName,
            ModelFileName);
        var mesh = AnimatedGlbMesh.Load(path);
        var program = CreateProgram(gl);
        var modelViewProjectionLocation = gl.GetUniformLocation(program, "uModelViewProjection");
        var jointMatricesLocation = gl.GetUniformLocation(program, "uJointMatrices[0]");
        var textureLocation = gl.GetUniformLocation(program, "uTexture");

        gl.UseProgram(program);
        gl.Uniform1i(textureLocation, 0);
        gl.UnuseProgram();

        var (vertexArray, vertexBuffer, indexBuffer) = CreateGeometry(gl, mesh.Vertices, mesh.Indices);
        var (texture, textureWidth, textureHeight) = CreateTexture(gl, mesh.TextureBytes);
        var boundsCenter = (mesh.BoundsMin + mesh.BoundsMax) * 0.5f;
        var boundsSize = mesh.BoundsMax - mesh.BoundsMin;
        var largestExtent = MathF.Max(boundsSize.X, MathF.Max(boundsSize.Y, boundsSize.Z));
        var modelScale = largestExtent <= 0f ? 1f : DisplayHeight / largestExtent;

        return new GlbModel(
            gl,
            program,
            modelViewProjectionLocation,
            jointMatricesLocation,
            vertexArray,
            vertexBuffer,
            indexBuffer,
            texture,
            textureWidth,
            textureHeight,
            mesh.Indices.Length,
            mesh,
            boundsCenter,
            modelScale);
    }

    /// <summary>Advances the selected animation slot that drives the model's skinning palette.</summary>
    public void Update(float elapsedSeconds) => mesh.UpdateAnimation(elapsedSeconds);

    /// <summary>Selects the next available animation slot.</summary>
    public void SelectNextAnimation() => mesh.SelectNextAnimation();

    /// <summary>Selects the previous available animation slot.</summary>
    public void SelectPreviousAnimation() => mesh.SelectPreviousAnimation();

    /// <summary>Selects one available animation slot by index.</summary>
    public void SelectAnimation(int animationIndex) => mesh.SelectAnimation(animationIndex);

    /// <summary>Gets the display name for one selectable animation slot.</summary>
    public string GetAnimationName(int animationIndex) => mesh.GetAnimationName(animationIndex);

    /// <summary>Gets the duration, in seconds, for one selectable animation slot.</summary>
    public float GetAnimationDuration(int animationIndex) => mesh.GetAnimationDuration(animationIndex);

    /// <summary>Draws the animated model through the supplied camera view and restores transient strict-layer bindings.</summary>
    public void Render(int framebufferWidth, int framebufferHeight, Mat4 view)
    {
        Span<float> modelViewProjection = stackalloc float[Mat4.ComponentCount];
        var model = CreateModelMatrix(boundsCenter, modelScale);
        var projection = Mat4.CreatePerspectiveFieldOfView(
            MathF.PI / 4f,
            framebufferWidth / (float)framebufferHeight,
            0.1f,
            100f);
        (projection * view * model).CopyTo(modelViewProjection);

        gl.UseProgram(program);
        gl.UniformMatrix4fv(modelViewProjectionLocation, false, modelViewProjection);
        gl.UniformMatrix4fv(jointMatricesLocation, false, mesh.JointMatrices);
        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.BindVertexArray(vertexArray);
        gl.DrawElements(GlPrimitiveType.Triangles, indexCount, GlDrawElementsType.UnsignedInt, 0);
        gl.UnbindVertexArray();
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();
        gl.UnuseProgram();
    }

    /// <summary>Deletes GPU resources once the frame loop has stopped drawing the model.</summary>
    public void Dispose()
    {
        gl.DeleteVertexArray(vertexArray);
        gl.DeleteBuffer(indexBuffer);
        gl.DeleteBuffer(vertexBuffer);
        gl.DeleteTexture(texture);
        gl.DeleteProgram(program);
    }

    /// <summary>Uploads the loaded GLB vertices and records their position, texture coordinate, joint, and weight attributes in a VAO.</summary>
    private static (GlVertexArrayHandle VertexArray, GlBufferHandle VertexBuffer, GlBufferHandle IndexBuffer) CreateGeometry(
        GlLayer gl,
        ReadOnlySpan<float> vertices,
        ReadOnlySpan<uint> indices)
    {
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
        gl.VertexAttribPointer(2, 4, GlVertexAttribPointerType.Float, false, VertexStrideBytes, JointsOffsetBytes);
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(3, 4, GlVertexAttribPointerType.Float, false, VertexStrideBytes, WeightsOffsetBytes);
        gl.EnableVertexAttribArray(3);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);

        gl.BindBuffer(GlBufferTarget.ElementArrayBuffer, indexBuffer);
        gl.BufferData(GlBufferTarget.ElementArrayBuffer, indices, GlBufferUsage.StaticDraw);
        gl.UnbindVertexArray();

        return (vertexArray, vertexBuffer, indexBuffer);
    }

    /// <summary>Decodes the GLB's embedded PNG and uploads it as a nearest-neighbor RGBA texture.</summary>
    private static (GlTextureHandle Texture, int Width, int Height) CreateTexture(GlLayer gl, byte[] pngBytes)
    {
        var image = Png.Open(pngBytes);
        var pixels = DecodeRgbaPixels(image);
        var texture = gl.GenTexture();

        gl.ActiveTexture(GlTextureUnit.Texture0);
        gl.BindTexture(GlTextureTarget.Texture2D, texture);
        gl.PixelStorei(GlPixelStoreParameter.UnpackAlignment, 1);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMinFilter, (int)GlTextureMinFilter.Nearest);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureMagFilter, (int)GlTextureMagFilter.Nearest);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapS, (int)GlTextureWrapMode.Repeat);
        gl.TexParameteri(GlTextureTarget.Texture2D, GlTextureParameterName.TextureWrapT, (int)GlTextureWrapMode.Repeat);
        gl.TexImage2D(
            GlTextureTarget.Texture2D,
            0,
            GlInternalFormat.Rgba8,
            image.Width,
            image.Height,
            0,
            GlPixelFormat.Rgba,
            GlPixelType.UnsignedByte,
            pixels);
        gl.ResetPixelStore(GlPixelStoreParameter.UnpackAlignment);
        gl.UnbindTexture(GlTextureTarget.Texture2D);
        gl.ResetActiveTexture();

        return (texture, image.Width, image.Height);
    }

    /// <summary>Copies BigGustave pixels into contiguous RGBA8 bytes for OpenGL texture upload.</summary>
    private static byte[] DecodeRgbaPixels(Png image)
    {
        var pixels = new byte[image.Width * image.Height * 4];
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                var offset = ((y * image.Width) + x) * 4;
                pixels[offset] = pixel.R;
                pixels[offset + 1] = pixel.G;
                pixels[offset + 2] = pixel.B;
                pixels[offset + 3] = pixel.A;
            }
        }

        return pixels;
    }

    /// <summary>Creates a model matrix that centers, scales, and turns the mesh into a static three-quarter pose.</summary>
    private static Mat4 CreateModelMatrix(Vec3 center, float scale) =>
        Mat4.CreateRotationY(-0.35f) * Mat4.CreateScale(scale) * Mat4.CreateTranslation(-center);

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
            ProjectRoot.ResDirectory(typeof(GlbModel)),
            "shaders",
            ResourceDirectoryName,
            name);
        return File.ReadAllText(path, Encoding.UTF8);
    }
}
