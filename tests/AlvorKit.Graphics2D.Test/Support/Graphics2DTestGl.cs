namespace AlvorKit.Graphics2D.Test;

/// <summary>Small fake OpenGL backend used behind <see cref="GlLayer"/> in Graphics2D tests.</summary>
internal sealed unsafe class Graphics2DTestGl : GlNoop
{
    /// <summary>The next generated OpenGL object id.</summary>
    private uint next = 1;

    /// <summary>Gets deleted object ids in deletion order.</summary>
    public List<uint> Deleted { get; } = [];

    /// <summary>Gets or sets the texture unit count reported by <see cref="GetIntegerv"/>.</summary>
    public int MaxTextureImageUnits { get; set; } = 16;

    /// <summary>Gets or sets the shader compile status reported by <see cref="GetShaderiv"/>.</summary>
    public int ShaderCompileStatus { get; set; } = 1;

    /// <summary>Gets or sets the program link status reported by <see cref="GetProgramiv"/>.</summary>
    public int ProgramLinkStatus { get; set; } = 1;

    /// <summary>Gets the number of texture image uploads observed by the backend.</summary>
    public int TexImage2DCalls { get; private set; }

    /// <summary>Gets the internal format from the most recent texture image upload.</summary>
    public GlInternalFormat LastTexImage2DInternalFormat { get; private set; }

    /// <summary>Gets the number of mipmap-generation calls observed by the backend.</summary>
    public int GenerateMipmapCalls { get; private set; }

    /// <summary>Gets the number of integer texture-parameter calls observed by the backend.</summary>
    public int TexParameterCalls { get; private set; }

    /// <summary>Gets the number of draw-elements calls observed by the backend.</summary>
    public int DrawElementsCalls { get; private set; }

    /// <inheritdoc/>
    public override void GenTextures(int n, nint textures) => Fill(n, textures);

    /// <inheritdoc/>
    public override void GenBuffers(int n, nint buffers) => Fill(n, buffers);

    /// <inheritdoc/>
    public override void GenVertexArrays(int n, nint arrays) => Fill(n, arrays);

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
    public override void DeleteShader(GlShaderHandle shader) => Deleted.Add((uint)shader);

    /// <inheritdoc/>
    public override void DeleteProgram(GlProgramHandle program) => Deleted.Add((uint)program);

    /// <inheritdoc/>
    public override void GetIntegerv(GlGetPName pname, nint data) => *(int*)data = MaxTextureImageUnits;

    /// <inheritdoc/>
    public override void GetShaderiv(GlShaderHandle shader, GlShaderParameterName pname, nint @params) => *(int*)@params = ShaderCompileStatus;

    /// <inheritdoc/>
    public override void GetProgramiv(GlProgramHandle program, GlProgramProperty pname, nint @params) => *(int*)@params = ProgramLinkStatus;

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
        nint pixels)
    {
        TexImage2DCalls++;
        LastTexImage2DInternalFormat = internalformat;
    }

    /// <inheritdoc/>
    public override void GenerateMipmap(GlTextureTarget target) => GenerateMipmapCalls++;

    /// <inheritdoc/>
    public override void TexParameteri(GlTextureTarget target, GlTextureParameterName pname, int param) => TexParameterCalls++;

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
