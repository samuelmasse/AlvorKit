namespace AlvorKit.UI.Test;

/// <summary>Minimal fake OpenGL backend so UI tests can construct the sprite pipeline without a GPU.</summary>
internal sealed unsafe class UiTestGl : GlNoop
{
    /// <summary>The next generated OpenGL object id.</summary>
    private uint next = 1;

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
    public override void GetIntegerv(GlGetPName pname, nint data) => *(int*)data = 16;

    /// <inheritdoc/>
    public override void GetShaderiv(GlShaderHandle shader, GlShaderParameterName pname, nint @params) => *(int*)@params = 1;

    /// <inheritdoc/>
    public override void GetProgramiv(GlProgramHandle program, GlProgramProperty pname, nint @params) => *(int*)@params = 1;

    /// <inheritdoc/>
    public override int GetUniformLocation(GlProgramHandle program, nint name) => 0;

    /// <summary>Writes generated ids into an OpenGL output pointer.</summary>
    private void Fill(int n, nint destination)
    {
        var ids = (uint*)destination;
        for (var i = 0; i < n; i++)
            ids[i] = next++;
    }
}
