namespace AlvorKit;

/// <summary>Records texture lifetimes while allowing Blend controls and font atlases to run without graphics hardware.</summary>
internal unsafe class BlendRecordingGl : GlNoop
{
    private readonly HashSet<uint> textures = [];
    private uint next = 1;

    internal bool HasTexture(GlTextureHandle texture) => textures.Contains((uint)texture);

    public override void GenTextures(int count, nint destination)
    {
        var ids = new Span<uint>((void*)destination, count);

        for (var i = 0; i < ids.Length; i++)
        {
            ids[i] = next++;
            textures.Add(ids[i]);
        }
    }

    public override void DeleteTextures(int count, nint source)
    {
        foreach (var id in new ReadOnlySpan<uint>((void*)source, count))
            textures.Remove(id);
    }

    public override void GenBuffers(int count, nint destination) => Generate(count, destination);
    public override void GenVertexArrays(int count, nint destination) => Generate(count, destination);
    public override void GenFramebuffers(int count, nint destination) => Generate(count, destination);
    public override GlShaderHandle CreateShader(GlShaderType type) => (GlShaderHandle)next++;
    public override GlProgramHandle CreateProgram() => (GlProgramHandle)next++;
    public override void GetIntegerv(GlGetPName name, nint value) => *(int*)value = 16;
    public override void GetShaderiv(GlShaderHandle shader, GlShaderParameterName name, nint value) => *(int*)value = 1;
    public override void GetProgramiv(GlProgramHandle program, GlProgramProperty name, nint value) => *(int*)value = 1;
    public override int GetUniformLocation(GlProgramHandle program, nint name) => 0;
    public override GlFramebufferStatus CheckFramebufferStatus(GlFramebufferTarget target) => GlFramebufferStatus.FramebufferComplete;

    private void Generate(int count, nint destination)
    {
        var ids = new Span<uint>((void*)destination, count);

        for (var i = 0; i < ids.Length; i++)
            ids[i] = next++;
    }
}
