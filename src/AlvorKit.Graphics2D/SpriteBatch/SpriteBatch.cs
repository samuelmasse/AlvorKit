namespace AlvorKit.Graphics2D;

/// <summary>Batches textured quads into indexed draw calls grouped by available texture slots.</summary>
public class SpriteBatch : IDisposable
{
    /// <summary>The reusable index buffer used by every render.</summary>
    private readonly QuadIndexBuffer indexBuffer;

    /// <summary>The dynamic vertex buffer populated at the end of each batch.</summary>
    private readonly QuadVertexBuffer<SpriteBatchVertex> vertexBuffer;

    /// <summary>The vertex array that captures the sprite vertex layout.</summary>
    private readonly QuadVertexArray vertexArray;

    /// <summary>The shader program used to render batched sprites.</summary>
    private readonly ShaderProgram program;

    /// <summary>The default white texture used by shape-only draw calls.</summary>
    private readonly Texture texture;

    /// <summary>The current canvas dimensions used by the writer.</summary>
    private readonly SpriteBatchCanvas canvas;

    /// <summary>The pending vertices and texture-section metadata.</summary>
    private readonly SpriteBatchVertices vertices;

    /// <summary>The public draw-command writer.</summary>
    private readonly SpriteBatchWriter writer;

    /// <summary>The renderer that flushes pending vertices to OpenGL.</summary>
    private readonly SpriteBatchRenderer renderer;

    /// <summary>Whether a batch has begun and is waiting for <see cref="End"/>.</summary>
    private bool started;

    /// <summary>Gets the writer used to enqueue draw calls between <see cref="Begin"/> and <see cref="End"/>.</summary>
    public SpriteBatchWriter Writer => writer;

    /// <summary>Creates a sprite batch with custom shader source.</summary>
    public SpriteBatch(GlLayer gl, string vertCode, string fragCode)
    {
        gl.GetIntegerv(GlGetPName.MaxTextureImageUnits, out var textureSlots);

        texture = new Texture2D(gl, (1u, 1u)) { Pixels = [(0xFF, 0xFF, 0xFF, 0xFF)] };
        canvas = new SpriteBatchCanvas();
        vertices = new SpriteBatchVertices(textureSlots);
        writer = new SpriteBatchWriter(texture, canvas, vertices);

        using var vertStage = new ShaderStage(gl, nameof(SpriteBatch), vertCode, GlShaderType.VertexShader);
        using var fragStage = new ShaderStage(gl, nameof(SpriteBatch), fragCode, GlShaderType.FragmentShader);
        program = new ShaderProgram(gl, nameof(SpriteBatch), [vertStage, fragStage]);

        indexBuffer = new QuadIndexBuffer(gl);
        vertexBuffer = new QuadVertexBuffer<SpriteBatchVertex>(gl, SpriteBatchVertex.Size);
        vertexArray = new QuadVertexArray(gl, new SpriteBatchShaderDescriptor(gl).SetAttribPointers, vertexBuffer, indexBuffer);
        renderer = new SpriteBatchRenderer(gl, vertices, program, vertexArray);
        renderer.SetupSamplerUniforms(textureSlots);
    }

    /// <summary>Creates a sprite batch with the built-in shader source.</summary>
    public SpriteBatch(GlLayer gl)
        : this(gl, SpriteBatchShaderSource.Vert(), SpriteBatchShaderSource.Frag(TextureSlots(gl)))
    {
    }

    /// <summary>Begins collecting draw calls for a canvas with the supplied pixel size.</summary>
    public void Begin(Vec2 size)
    {
        if (started)
            throw new InvalidOperationException("Cannot begin sprite batch when it is already started.");

        canvas.Size = size;
        writer.Clip = null;
        started = true;
    }

    /// <summary>Flushes the queued draw calls and ends the current batch.</summary>
    public void End()
    {
        if (!started)
            throw new InvalidOperationException("Cannot end sprite batch when it has not been started.");

        renderer.Render();
        vertices.Reset();
        started = false;
    }

    /// <summary>Deletes all OpenGL resources owned by the sprite batch.</summary>
    public void Dispose()
    {
        texture.Dispose();
        program.Dispose();
        vertexArray.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    /// <summary>Reads the number of available fragment texture slots.</summary>
    private static int TextureSlots(GlLayer gl)
    {
        gl.GetIntegerv(GlGetPName.MaxTextureImageUnits, out var textureSlots);
        return textureSlots;
    }
}
