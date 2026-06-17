namespace AlvorKit.OpenGL.Layer.Test;

internal sealed unsafe class RecordingGl : GlNoop
{
    private uint next = 1;

    public List<uint> Deleted { get; } = [];
    public uint[] LastBindBuffersBaseBuffers { get; private set; } = [];
    public uint[] LastBindBuffersRangeBuffers { get; private set; } = [];
    public nint[] LastBindBuffersRangeOffsets { get; private set; } = [];
    public nint[] LastBindBuffersRangeSizes { get; private set; } = [];
    public uint[] LastBindVertexBuffersBuffers { get; private set; } = [];
    public nint[] LastBindVertexBuffersOffsets { get; private set; } = [];
    public int[] LastBindVertexBuffersStrides { get; private set; } = [];
    public uint[] LastBindSamplers { get; private set; } = [];
    public uint[] LastBindImageTextures { get; private set; } = [];
    public uint[] LastBindTextures { get; private set; } = [];

    public override void GenTextures(int n, nint p) => Fill(n, p);
    public override void GenBuffers(int n, nint p) => Fill(n, p);
    public override void GenVertexArrays(int n, nint p) => Fill(n, p);
    public override void GenFramebuffers(int n, nint p) => Fill(n, p);
    public override void GenRenderbuffers(int n, nint p) => Fill(n, p);
    public override void GenSamplers(int n, nint p) => Fill(n, p);
    public override void GenQueries(int n, nint p) => Fill(n, p);
    public override void GenProgramPipelines(int n, nint p) => Fill(n, p);
    public override void GenTransformFeedbacks(int n, nint p) => Fill(n, p);
    public override GlShaderHandle CreateShader(GlShaderType type) => (GlShaderHandle)next++;
    public override GlProgramHandle CreateProgram() => (GlProgramHandle)next++;

    public override void DeleteTextures(int n, nint p) => Record(n, p);
    public override void DeleteBuffers(int n, nint p) => Record(n, p);
    public override void DeleteVertexArrays(int n, nint p) => Record(n, p);
    public override void DeleteFramebuffers(int n, nint p) => Record(n, p);
    public override void DeleteRenderbuffers(int n, nint p) => Record(n, p);
    public override void DeleteSamplers(int n, nint p) => Record(n, p);
    public override void DeleteQueries(int n, nint p) => Record(n, p);
    public override void DeleteProgramPipelines(int n, nint p) => Record(n, p);
    public override void DeleteTransformFeedbacks(int n, nint p) => Record(n, p);
    public override void DeleteShader(GlShaderHandle shader) => Deleted.Add((uint)shader);
    public override void DeleteProgram(GlProgramHandle program) => Deleted.Add((uint)program);
    public override void DeleteSync(nint sync) => Deleted.Add((uint)sync);
    public override nint FenceSync(GlSyncCondition condition, GlSyncBehaviorFlags flags) => (nint)next++;
    public override void BindBuffersBase(GlBufferTarget target, uint first, int count, nint buffers) =>
        LastBindBuffersBaseBuffers = CopyUInts(count, buffers);

    public override void BindBuffersRange(GlBufferTarget target, uint first, int count, nint buffers, nint offsets, nint sizes)
    {
        LastBindBuffersRangeBuffers = CopyUInts(count, buffers);
        LastBindBuffersRangeOffsets = CopyNints(count, offsets);
        LastBindBuffersRangeSizes = CopyNints(count, sizes);
    }

    public override void BindVertexBuffers(uint first, int count, nint buffers, nint offsets, nint strides)
    {
        LastBindVertexBuffersBuffers = CopyUInts(count, buffers);
        LastBindVertexBuffersOffsets = CopyNints(count, offsets);
        LastBindVertexBuffersStrides = CopyInts(count, strides);
    }

    public override void BindSamplers(uint first, int count, nint samplers) => LastBindSamplers = CopyUInts(count, samplers);
    public override void BindImageTextures(uint first, int count, nint textures) => LastBindImageTextures = CopyUInts(count, textures);
    public override void BindTextures(uint first, int count, nint textures) => LastBindTextures = CopyUInts(count, textures);

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

    private static uint[] CopyUInts(int count, nint p)
    {
        var values = new uint[count];
        var ids = (uint*)p;
        for (var i = 0; i < count && p != 0; i++)
            values[i] = ids[i];
        return values;
    }

    private static nint[] CopyNints(int count, nint p)
    {
        var values = new nint[count];
        var ids = (nint*)p;
        for (var i = 0; i < count && p != 0; i++)
            values[i] = ids[i];
        return values;
    }

    private static int[] CopyInts(int count, nint p)
    {
        var values = new int[count];
        var ids = (int*)p;
        for (var i = 0; i < count && p != 0; i++)
            values[i] = ids[i];
        return values;
    }
}
