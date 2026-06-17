namespace AlvorKit.Graphics2D.Fonts.Test;

/// <summary>Small fake OpenGL backend used behind <see cref="GlLayer"/> in Graphics2D font tests.</summary>
internal sealed unsafe class FontsTestGl : GlNoop
{
    /// <summary>The next generated OpenGL object id.</summary>
    private uint next = 1;

    /// <summary>Gets deleted object ids in deletion order.</summary>
    public List<uint> Deleted { get; } = [];

    /// <summary>Gets or sets the texture unit count reported by <see cref="GetIntegerv"/>.</summary>
    public int MaxTextureImageUnits { get; set; } = 16;

    /// <summary>Gets the number of texture allocation calls observed by the backend.</summary>
    public int TexImage2DCalls { get; private set; }

    /// <summary>Gets the number of glyph subimage uploads observed by the backend.</summary>
    public int TexSubImage2DCalls { get; private set; }

    /// <summary>Gets the number of texture parameter calls observed by the backend.</summary>
    public int TexParameterCalls { get; private set; }

    /// <summary>Gets the number of framebuffer texture attachment calls observed by the backend.</summary>
    public int FramebufferTextureCalls { get; private set; }

    /// <summary>Gets the number of draw-buffer calls observed by the backend.</summary>
    public int DrawBufferCalls { get; private set; }

    /// <summary>Gets the number of draw-elements calls observed by the backend.</summary>
    public int DrawElementsCalls { get; private set; }

    /// <summary>Gets the most recent texture subimage upload bytes.</summary>
    public byte[] LastTexSubImageBytes { get; private set; } = [];

    /// <summary>Gets the most recent texture subimage upload rectangle.</summary>
    public (int X, int Y, int Width, int Height) LastTexSubImage { get; private set; }

    /// <summary>Gets the most recent uploaded sprite vertex floats.</summary>
    public float[] LastVertexFloats { get; private set; } = [];

    /// <inheritdoc/>
    public override void GenTextures(int n, nint textures) => Fill(n, textures);

    /// <inheritdoc/>
    public override void GenBuffers(int n, nint buffers) => Fill(n, buffers);

    /// <inheritdoc/>
    public override void GenVertexArrays(int n, nint arrays) => Fill(n, arrays);

    /// <inheritdoc/>
    public override void GenFramebuffers(int n, nint framebuffers) => Fill(n, framebuffers);

    /// <inheritdoc/>
    public override GlShaderHandle CreateShader(GlShaderType type) => (GlShaderHandle)next++;

    /// <inheritdoc/>
    public override GlProgramHandle CreateProgram() => (GlProgramHandle)next++;

    /// <inheritdoc/>
    public override void DeleteTextures(int n, nint textures) => Record(n, textures);

    /// <inheritdoc/>
    public override void DeleteBuffers(int n, nint buffers) => Record(n, buffers);

    /// <inheritdoc/>
    public override void DeleteVertexArrays(int n, nint arrays) => Record(n, arrays);

    /// <inheritdoc/>
    public override void DeleteFramebuffers(int n, nint framebuffers) => Record(n, framebuffers);

    /// <inheritdoc/>
    public override void DeleteShader(GlShaderHandle shader) => Deleted.Add((uint)shader);

    /// <inheritdoc/>
    public override void DeleteProgram(GlProgramHandle program) => Deleted.Add((uint)program);

    /// <inheritdoc/>
    public override void GetIntegerv(GlGetPName pname, nint data) => *(int*)data = MaxTextureImageUnits;

    /// <inheritdoc/>
    public override void GetShaderiv(GlShaderHandle shader, GlShaderParameterName pname, nint @params) => *(int*)@params = 1;

    /// <inheritdoc/>
    public override void GetProgramiv(GlProgramHandle program, GlProgramProperty pname, nint @params) => *(int*)@params = 1;

    /// <inheritdoc/>
    public override int GetUniformLocation(GlProgramHandle program, nint name) => 0;

    /// <inheritdoc/>
    public override void TexImage2D(
        GlTextureTarget target,
        int level,
        GlInternalFormat internalformat,
        int width,
        int height,
        int border,
        GlPixelFormat format,
        GlPixelType type,
        nint pixels) =>
        TexImage2DCalls++;

    /// <inheritdoc/>
    public override void TexSubImage2D(
        GlTextureTarget target,
        int level,
        int xoffset,
        int yoffset,
        int width,
        int height,
        GlPixelFormat format,
        GlPixelType type,
        nint pixels)
    {
        TexSubImage2DCalls++;
        LastTexSubImage = (xoffset, yoffset, width, height);
        LastTexSubImageBytes = new byte[width * height * 4];
        Marshal.Copy(pixels, LastTexSubImageBytes, 0, LastTexSubImageBytes.Length);
    }

    /// <inheritdoc/>
    public override void BufferSubData(GlBufferTarget target, nint offset, nint size, nint data)
    {
        var bytes = new byte[(int)size];
        Marshal.Copy(data, bytes, 0, bytes.Length);
        LastVertexFloats = MemoryMarshal.Cast<byte, float>(bytes).ToArray();
    }

    /// <inheritdoc/>
    public override void TexParameteri(GlTextureTarget target, GlTextureParameterName pname, int param) => TexParameterCalls++;

    /// <inheritdoc/>
    public override void FramebufferTexture(
        GlFramebufferTarget target,
        GlFramebufferAttachment attachment,
        GlTextureHandle texture,
        int level) =>
        FramebufferTextureCalls++;

    /// <inheritdoc/>
    public override void DrawBuffer(GlDrawBufferMode buf) => DrawBufferCalls++;

    /// <inheritdoc/>
    public override void DrawElements(GlPrimitiveType mode, int count, GlDrawElementsType type, nint indices) => DrawElementsCalls++;

    /// <summary>Writes generated ids into an OpenGL output pointer.</summary>
    private void Fill(int n, nint destination)
    {
        var ids = (uint*)destination;
        for (var i = 0; i < n; i++)
            ids[i] = next++;
    }

    /// <summary>Records ids read from an OpenGL deletion pointer.</summary>
    private void Record(int n, nint source)
    {
        var ids = (uint*)source;
        for (var i = 0; i < n; i++)
            Deleted.Add(ids[i]);
    }
}
