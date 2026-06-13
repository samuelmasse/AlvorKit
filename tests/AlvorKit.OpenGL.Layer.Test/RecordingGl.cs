namespace AlvorKit.OpenGL.Layer.Test;

internal sealed unsafe class RecordingGl : Gl
{
    private uint next = 1;

    public List<uint> Deleted { get; } = [];

    public override void GenTextures(int n, nint p) => Fill(n, p);
    public override void GenBuffers(int n, nint p) => Fill(n, p);
    public override void GenVertexArrays(int n, nint p) => Fill(n, p);
    public override void GenFramebuffers(int n, nint p) => Fill(n, p);
    public override void GenRenderbuffers(int n, nint p) => Fill(n, p);
    public override void GenSamplers(int n, nint p) => Fill(n, p);
    public override void GenQueries(int n, nint p) => Fill(n, p);
    public override void GenProgramPipelines(int n, nint p) => Fill(n, p);
    public override void GenTransformFeedbacks(int n, nint p) => Fill(n, p);
    public override uint CreateShader(ShaderType type) => next++;
    public override uint CreateProgram() => next++;

    public override void DeleteTextures(int n, nint p) => Record(n, p);
    public override void DeleteBuffers(int n, nint p) => Record(n, p);
    public override void DeleteVertexArrays(int n, nint p) => Record(n, p);
    public override void DeleteFramebuffers(int n, nint p) => Record(n, p);
    public override void DeleteRenderbuffers(int n, nint p) => Record(n, p);
    public override void DeleteSamplers(int n, nint p) => Record(n, p);
    public override void DeleteQueries(int n, nint p) => Record(n, p);
    public override void DeleteProgramPipelines(int n, nint p) => Record(n, p);
    public override void DeleteTransformFeedbacks(int n, nint p) => Record(n, p);
    public override void DeleteShader(uint shader) => Deleted.Add(shader);
    public override void DeleteProgram(uint program) => Deleted.Add(program);

    private void Fill(int n, nint p)
    {
        var ids = (uint*)p;
        for (var i = 0; i < n; i++)
            ids[i] = next++;
    }

    private void Record(int n, nint p)
    {
        var ids = (uint*)p;
        for (var i = 0; i < n; i++)
            Deleted.Add(ids[i]);
    }
}
