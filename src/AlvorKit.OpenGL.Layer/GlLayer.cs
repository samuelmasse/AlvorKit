namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer(Gl inner) : GlWrapper(inner), IDisposable
{
    private readonly HashSet<uint> textures = [];
    private readonly HashSet<uint> buffers = [];
    private readonly HashSet<uint> vertexArrays = [];
    private readonly HashSet<uint> framebuffers = [];
    private readonly HashSet<uint> renderbuffers = [];
    private readonly HashSet<uint> samplers = [];
    private readonly HashSet<uint> queries = [];
    private readonly HashSet<uint> programPipelines = [];
    private readonly HashSet<uint> transformFeedbacks = [];
    private readonly HashSet<uint> shaders = [];
    private readonly HashSet<uint> programs = [];
    private readonly HashSet<nint> syncs = [];

    public IReadOnlySet<uint> Textures => textures;
    public IReadOnlySet<uint> Buffers => buffers;
    public IReadOnlySet<uint> VertexArrays => vertexArrays;
    public IReadOnlySet<uint> Framebuffers => framebuffers;
    public IReadOnlySet<uint> Renderbuffers => renderbuffers;
    public IReadOnlySet<uint> Samplers => samplers;
    public IReadOnlySet<uint> Queries => queries;
    public IReadOnlySet<uint> ProgramPipelines => programPipelines;
    public IReadOnlySet<uint> TransformFeedbacks => transformFeedbacks;
    public IReadOnlySet<uint> Shaders => shaders;
    public IReadOnlySet<uint> Programs => programs;
    public IReadOnlySet<nint> Syncs => syncs;

    public override void GenTextures(int n, nint textures) { base.GenTextures(n, textures); Track(this.textures, n, textures); }
    public override void GenBuffers(int n, nint buffers) { base.GenBuffers(n, buffers); Track(this.buffers, n, buffers); }
    public override void GenVertexArrays(int n, nint arrays) { base.GenVertexArrays(n, arrays); Track(vertexArrays, n, arrays); }
    public override void GenFramebuffers(int n, nint framebuffers) { base.GenFramebuffers(n, framebuffers); Track(this.framebuffers, n, framebuffers); }
    public override void GenRenderbuffers(int n, nint renderbuffers) { base.GenRenderbuffers(n, renderbuffers); Track(this.renderbuffers, n, renderbuffers); }
    public override void GenSamplers(int count, nint samplers) { base.GenSamplers(count, samplers); Track(this.samplers, count, samplers); }
    public override void GenQueries(int n, nint ids) { base.GenQueries(n, ids); Track(queries, n, ids); }
    public override void GenProgramPipelines(int n, nint pipelines) { base.GenProgramPipelines(n, pipelines); Track(programPipelines, n, pipelines); }
    public override void GenTransformFeedbacks(int n, nint ids) { base.GenTransformFeedbacks(n, ids); Track(transformFeedbacks, n, ids); }

    public override void CreateTextures(TextureTarget target, int n, nint textures) { base.CreateTextures(target, n, textures); Track(this.textures, n, textures); }
    public override void CreateBuffers(int n, nint buffers) { base.CreateBuffers(n, buffers); Track(this.buffers, n, buffers); }
    public override void CreateVertexArrays(int n, nint arrays) { base.CreateVertexArrays(n, arrays); Track(vertexArrays, n, arrays); }
    public override void CreateFramebuffers(int n, nint framebuffers) { base.CreateFramebuffers(n, framebuffers); Track(this.framebuffers, n, framebuffers); }
    public override void CreateRenderbuffers(int n, nint renderbuffers) { base.CreateRenderbuffers(n, renderbuffers); Track(this.renderbuffers, n, renderbuffers); }
    public override void CreateSamplers(int n, nint samplers) { base.CreateSamplers(n, samplers); Track(this.samplers, n, samplers); }
    public override void CreateQueries(QueryTarget target, int n, nint ids) { base.CreateQueries(target, n, ids); Track(queries, n, ids); }
    public override void CreateProgramPipelines(int n, nint pipelines) { base.CreateProgramPipelines(n, pipelines); Track(programPipelines, n, pipelines); }
    public override void CreateTransformFeedbacks(int n, nint ids) { base.CreateTransformFeedbacks(n, ids); Track(transformFeedbacks, n, ids); }

    public override uint CreateShader(ShaderType type) { var id = base.CreateShader(type); shaders.Add(id); return id; }
    public override uint CreateProgram() { var id = base.CreateProgram(); programs.Add(id); return id; }
    public override uint CreateShaderProgramv(ShaderType type, int count, nint strings) { var id = base.CreateShaderProgramv(type, count, strings); programs.Add(id); return id; }
    public override nint FenceSync(SyncCondition condition, SyncBehaviorFlags flags) { var sync = base.FenceSync(condition, flags); syncs.Add(sync); return sync; }

    public override void DeleteTextures(int n, nint textures)
    {
        var ids = (uint*)textures;
        for (var i = 0; i < n; i++)
        {
            this.textures.Remove(ids[i]);
            textureTargets.Remove(ids[i]);
        }
        base.DeleteTextures(n, textures);
    }
    public override void DeleteBuffers(int n, nint buffers) { Untrack(this.buffers, n, buffers); base.DeleteBuffers(n, buffers); }
    public override void DeleteVertexArrays(int n, nint arrays) { Untrack(vertexArrays, n, arrays); base.DeleteVertexArrays(n, arrays); }
    public override void DeleteFramebuffers(int n, nint framebuffers) { Untrack(this.framebuffers, n, framebuffers); base.DeleteFramebuffers(n, framebuffers); }
    public override void DeleteRenderbuffers(int n, nint renderbuffers) { Untrack(this.renderbuffers, n, renderbuffers); base.DeleteRenderbuffers(n, renderbuffers); }
    public override void DeleteSamplers(int count, nint samplers) { Untrack(this.samplers, count, samplers); base.DeleteSamplers(count, samplers); }
    public override void DeleteQueries(int n, nint ids) { Untrack(queries, n, ids); base.DeleteQueries(n, ids); }
    public override void DeleteProgramPipelines(int n, nint pipelines) { Untrack(programPipelines, n, pipelines); base.DeleteProgramPipelines(n, pipelines); }
    public override void DeleteTransformFeedbacks(int n, nint ids) { Untrack(transformFeedbacks, n, ids); base.DeleteTransformFeedbacks(n, ids); }
    public override void DeleteShader(uint shader) { shaders.Remove(shader); base.DeleteShader(shader); }
    public override void DeleteProgram(uint program) { programs.Remove(program); base.DeleteProgram(program); }
    public override void DeleteSync(nint sync) { syncs.Remove(sync); base.DeleteSync(sync); }

    public void Dispose()
    {
        foreach (var sync in Drain(syncs)) base.DeleteSync(sync);
        DeleteAll(transformFeedbacks, base.DeleteTransformFeedbacks);
        DeleteAll(programPipelines, base.DeleteProgramPipelines);
        DeleteAll(queries, base.DeleteQueries);
        DeleteAll(vertexArrays, base.DeleteVertexArrays);
        DeleteAll(framebuffers, base.DeleteFramebuffers);
        DeleteAll(renderbuffers, base.DeleteRenderbuffers);
        DeleteAll(samplers, base.DeleteSamplers);
        DeleteAll(textures, base.DeleteTextures);
        DeleteAll(buffers, base.DeleteBuffers);
        foreach (var shader in Drain(shaders)) base.DeleteShader(shader);
        foreach (var program in Drain(programs)) base.DeleteProgram(program);
    }

    private static void Track(HashSet<uint> set, int n, nint ids)
    {
        var p = (uint*)ids;
        for (var i = 0; i < n; i++)
            set.Add(p[i]);
    }

    private static void Untrack(HashSet<uint> set, int n, nint ids)
    {
        var p = (uint*)ids;
        for (var i = 0; i < n; i++)
            set.Remove(p[i]);
    }

    private delegate void PluralDelete(int n, nint ids);

    private static void DeleteAll(HashSet<uint> set, PluralDelete delete)
    {
        if (set.Count == 0)
            return;
        var ids = Drain(set);
        fixed (uint* p = ids)
            delete(ids.Length, (nint)p);
    }

    private static T[] Drain<T>(HashSet<T> set)
    {
        var ids = new T[set.Count];
        set.CopyTo(ids);
        set.Clear();
        return ids;
    }
}
