namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
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

    /// <summary>Layer: the textures generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> Textures => textures;
    /// <summary>Layer: the buffers generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> Buffers => buffers;
    /// <summary>Layer: the vertex arrays generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> VertexArrays => vertexArrays;
    /// <summary>Layer: the framebuffers generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> Framebuffers => framebuffers;
    /// <summary>Layer: the renderbuffers generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> Renderbuffers => renderbuffers;
    /// <summary>Layer: the samplers generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> Samplers => samplers;
    /// <summary>Layer: the queries generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> Queries => queries;
    /// <summary>Layer: the program pipelines generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> ProgramPipelines => programPipelines;
    /// <summary>Layer: the transform feedback objects generated or created and not yet deleted.</summary>
    public IReadOnlySet<uint> TransformFeedbacks => transformFeedbacks;
    /// <summary>Layer: the shaders created and not yet deleted.</summary>
    public IReadOnlySet<uint> Shaders => shaders;
    /// <summary>Layer: the programs created and not yet deleted.</summary>
    public IReadOnlySet<uint> Programs => programs;
    /// <summary>Layer: the sync objects created and not yet deleted.</summary>
    public IReadOnlySet<nint> Syncs => syncs;

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated textures, deleted when the layer is disposed.</remarks>
    public override void GenTextures(int n, nint textures) { base.GenTextures(n, textures); Track(this.textures, n, textures); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated buffers, deleted when the layer is disposed.</remarks>
    public override void GenBuffers(int n, nint buffers) { base.GenBuffers(n, buffers); Track(this.buffers, n, buffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated vertex arrays, deleted when the layer is disposed.</remarks>
    public override void GenVertexArrays(int n, nint arrays) { base.GenVertexArrays(n, arrays); Track(vertexArrays, n, arrays); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated framebuffers, deleted when the layer is disposed.</remarks>
    public override void GenFramebuffers(int n, nint framebuffers) { base.GenFramebuffers(n, framebuffers); Track(this.framebuffers, n, framebuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated renderbuffers, deleted when the layer is disposed.</remarks>
    public override void GenRenderbuffers(int n, nint renderbuffers) { base.GenRenderbuffers(n, renderbuffers); Track(this.renderbuffers, n, renderbuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated samplers, deleted when the layer is disposed.</remarks>
    public override void GenSamplers(int count, nint samplers) { base.GenSamplers(count, samplers); Track(this.samplers, count, samplers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated queries, deleted when the layer is disposed.</remarks>
    public override void GenQueries(int n, nint ids) { base.GenQueries(n, ids); Track(queries, n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated program pipelines, deleted when the layer is disposed.</remarks>
    public override void GenProgramPipelines(int n, nint pipelines) { base.GenProgramPipelines(n, pipelines); Track(programPipelines, n, pipelines); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated transform feedback objects, deleted when the layer is disposed.</remarks>
    public override void GenTransformFeedbacks(int n, nint ids) { base.GenTransformFeedbacks(n, ids); Track(transformFeedbacks, n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created textures, deleted when the layer is disposed.</remarks>
    public override void CreateTextures(GlTextureTarget target, int n, nint textures) { base.CreateTextures(target, n, textures); Track(this.textures, n, textures); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created buffers, deleted when the layer is disposed.</remarks>
    public override void CreateBuffers(int n, nint buffers) { base.CreateBuffers(n, buffers); Track(this.buffers, n, buffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created vertex arrays, deleted when the layer is disposed.</remarks>
    public override void CreateVertexArrays(int n, nint arrays) { base.CreateVertexArrays(n, arrays); Track(vertexArrays, n, arrays); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created framebuffers, deleted when the layer is disposed.</remarks>
    public override void CreateFramebuffers(int n, nint framebuffers) { base.CreateFramebuffers(n, framebuffers); Track(this.framebuffers, n, framebuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created renderbuffers, deleted when the layer is disposed.</remarks>
    public override void CreateRenderbuffers(int n, nint renderbuffers) { base.CreateRenderbuffers(n, renderbuffers); Track(this.renderbuffers, n, renderbuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created samplers, deleted when the layer is disposed.</remarks>
    public override void CreateSamplers(int n, nint samplers) { base.CreateSamplers(n, samplers); Track(this.samplers, n, samplers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created queries, deleted when the layer is disposed.</remarks>
    public override void CreateQueries(GlQueryTarget target, int n, nint ids) { base.CreateQueries(target, n, ids); Track(queries, n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program pipelines, deleted when the layer is disposed.</remarks>
    public override void CreateProgramPipelines(int n, nint pipelines) { base.CreateProgramPipelines(n, pipelines); Track(programPipelines, n, pipelines); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created transform feedback objects, deleted when the layer is disposed.</remarks>
    public override void CreateTransformFeedbacks(int n, nint ids) { base.CreateTransformFeedbacks(n, ids); Track(transformFeedbacks, n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created shader, deleted when the layer is disposed.</remarks>
    public override GlShaderHandle CreateShader(GlShaderType type) { var id = base.CreateShader(type); shaders.Add((uint)id); return id; }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program, deleted when the layer is disposed.</remarks>
    public override GlProgramHandle CreateProgram() { var id = base.CreateProgram(); programs.Add((uint)id); return id; }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program, deleted when the layer is disposed.</remarks>
    public override GlProgramHandle CreateShaderProgramv(GlShaderType type, int count, nint strings) { var id = base.CreateShaderProgramv(type, count, strings); programs.Add((uint)id); return id; }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created sync object, deleted when the layer is disposed.</remarks>
    public override nint FenceSync(GlSyncCondition condition, GlSyncBehaviorFlags flags) { var sync = base.FenceSync(condition, flags); syncs.Add(sync); return sync; }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted textures and releases their tracked memory.</remarks>
    public override void DeleteTextures(int n, nint textures)
    {
        var ids = (uint*)textures;
        for (var i = 0; i < n; i++) { this.textures.Remove(ids[i]); textureTargets.Remove(ids[i]); ReleaseTextureMemory(ids[i]); }
        base.DeleteTextures(n, textures);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted buffers and releases their tracked memory.</remarks>
    public override void DeleteBuffers(int n, nint buffers)
    {
        var ids = (uint*)buffers;
        for (var i = 0; i < n; i++) { this.buffers.Remove(ids[i]); ReleaseBufferMemory(ids[i]); }
        base.DeleteBuffers(n, buffers);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted vertex arrays.</remarks>
    public override void DeleteVertexArrays(int n, nint arrays) { Untrack(vertexArrays, n, arrays); base.DeleteVertexArrays(n, arrays); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted framebuffers.</remarks>
    public override void DeleteFramebuffers(int n, nint framebuffers) { Untrack(this.framebuffers, n, framebuffers); base.DeleteFramebuffers(n, framebuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted renderbuffers and releases their tracked memory.</remarks>
    public override void DeleteRenderbuffers(int n, nint renderbuffers)
    {
        var ids = (uint*)renderbuffers;
        for (var i = 0; i < n; i++) { this.renderbuffers.Remove(ids[i]); ReleaseRenderbufferMemory(ids[i]); }
        base.DeleteRenderbuffers(n, renderbuffers);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted samplers.</remarks>
    public override void DeleteSamplers(int count, nint samplers) { Untrack(this.samplers, count, samplers); base.DeleteSamplers(count, samplers); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted queries.</remarks>
    public override void DeleteQueries(int n, nint ids) { Untrack(queries, n, ids); base.DeleteQueries(n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program pipelines.</remarks>
    public override void DeleteProgramPipelines(int n, nint pipelines) { Untrack(programPipelines, n, pipelines); base.DeleteProgramPipelines(n, pipelines); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted transform feedback objects.</remarks>
    public override void DeleteTransformFeedbacks(int n, nint ids) { Untrack(transformFeedbacks, n, ids); base.DeleteTransformFeedbacks(n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted shader.</remarks>
    public void DeleteShader(uint shader) => DeleteShader((GlShaderHandle)shader);
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted shader.</remarks>
    public override void DeleteShader(GlShaderHandle shader) { shaders.Remove((uint)shader); base.DeleteShader(shader); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program.</remarks>
    public void DeleteProgram(uint program) => DeleteProgram((GlProgramHandle)program);
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program.</remarks>
    public override void DeleteProgram(GlProgramHandle program) { programs.Remove((uint)program); base.DeleteProgram(program); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted sync object.</remarks>
    public override void DeleteSync(nint sync) { syncs.Remove(sync); base.DeleteSync(sync); }

    /// <summary>Layer: deletes every object still tracked by this layer, in reverse dependency order.</summary>
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
        foreach (var shader in Drain(shaders)) base.DeleteShader((GlShaderHandle)shader);
        foreach (var program in Drain(programs)) base.DeleteProgram((GlProgramHandle)program);
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
