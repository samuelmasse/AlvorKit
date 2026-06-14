namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    private readonly GlResourceSet<GlTextureHandle> textures = new("texture", id => (GlTextureHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlBufferHandle> buffers = new("buffer", id => (GlBufferHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlVertexArrayHandle> vertexArrays = new("vertex array", id => (GlVertexArrayHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlFramebufferHandle> framebuffers = new("framebuffer", id => (GlFramebufferHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlRenderbufferHandle> renderbuffers = new("renderbuffer", id => (GlRenderbufferHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlSamplerHandle> samplers = new("sampler", id => (GlSamplerHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlQueryHandle> queries = new("query", id => (GlQueryHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlProgramPipelineHandle> programPipelines = new("program pipeline", id => (GlProgramPipelineHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlTransformFeedbackHandle> transformFeedbacks = new("transform feedback", id => (GlTransformFeedbackHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlShaderHandle> shaders = new("shader", id => (GlShaderHandle)id, handle => (uint)handle);
    private readonly GlResourceSet<GlProgramHandle> programs = new("program", id => (GlProgramHandle)id, handle => (uint)handle);
    private readonly HashSet<nint> syncs = [];

    /// <summary>Layer: the textures generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlTextureHandle> Textures => textures.Items;
    /// <summary>Layer: the buffers generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlBufferHandle> Buffers => buffers.Items;
    /// <summary>Layer: the vertex arrays generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlVertexArrayHandle> VertexArrays => vertexArrays.Items;
    /// <summary>Layer: the framebuffers generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlFramebufferHandle> Framebuffers => framebuffers.Items;
    /// <summary>Layer: the renderbuffers generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlRenderbufferHandle> Renderbuffers => renderbuffers.Items;
    /// <summary>Layer: the samplers generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlSamplerHandle> Samplers => samplers.Items;
    /// <summary>Layer: the queries generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlQueryHandle> Queries => queries.Items;
    /// <summary>Layer: the program pipelines generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlProgramPipelineHandle> ProgramPipelines => programPipelines.Items;
    /// <summary>Layer: the transform feedback objects generated or created and not yet deleted.</summary>
    public IReadOnlySet<GlTransformFeedbackHandle> TransformFeedbacks => transformFeedbacks.Items;
    /// <summary>Layer: the shaders created and not yet deleted.</summary>
    public IReadOnlySet<GlShaderHandle> Shaders => shaders.Items;
    /// <summary>Layer: the programs created and not yet deleted.</summary>
    public IReadOnlySet<GlProgramHandle> Programs => programs.Items;
    /// <summary>Layer: the sync objects created and not yet deleted.</summary>
    public IReadOnlySet<nint> Syncs => syncs;

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated textures, deleted when the layer is disposed.</remarks>
    public override void GenTextures(int n, nint textures) { base.GenTextures(n, textures); this.textures.Track(n, textures); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated buffers, deleted when the layer is disposed.</remarks>
    public override void GenBuffers(int n, nint buffers) { base.GenBuffers(n, buffers); this.buffers.Track(n, buffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated vertex arrays, deleted when the layer is disposed.</remarks>
    public override void GenVertexArrays(int n, nint arrays) { base.GenVertexArrays(n, arrays); vertexArrays.Track(n, arrays); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated framebuffers, deleted when the layer is disposed.</remarks>
    public override void GenFramebuffers(int n, nint framebuffers) { base.GenFramebuffers(n, framebuffers); this.framebuffers.Track(n, framebuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated renderbuffers, deleted when the layer is disposed.</remarks>
    public override void GenRenderbuffers(int n, nint renderbuffers) { base.GenRenderbuffers(n, renderbuffers); this.renderbuffers.Track(n, renderbuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated samplers, deleted when the layer is disposed.</remarks>
    public override void GenSamplers(int count, nint samplers) { base.GenSamplers(count, samplers); this.samplers.Track(count, samplers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated queries, deleted when the layer is disposed.</remarks>
    public override void GenQueries(int n, nint ids) { base.GenQueries(n, ids); queries.Track(n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated program pipelines, deleted when the layer is disposed.</remarks>
    public override void GenProgramPipelines(int n, nint pipelines) { base.GenProgramPipelines(n, pipelines); programPipelines.Track(n, pipelines); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the generated transform feedback objects, deleted when the layer is disposed.</remarks>
    public override void GenTransformFeedbacks(int n, nint ids) { base.GenTransformFeedbacks(n, ids); transformFeedbacks.Track(n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created textures, deleted when the layer is disposed.</remarks>
    public override void CreateTextures(GlTextureTarget target, int n, nint textures)
    {
        base.CreateTextures(target, n, textures);
        this.textures.Track(n, textures);
        var ids = (uint*)textures;
        for (var i = 0; i < n; i++)
            TrackTextureTarget(nameof(CreateTextures), (GlTextureHandle)ids[i], target);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created buffers, deleted when the layer is disposed.</remarks>
    public override void CreateBuffers(int n, nint buffers) { base.CreateBuffers(n, buffers); this.buffers.Track(n, buffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created vertex arrays, deleted when the layer is disposed.</remarks>
    public override void CreateVertexArrays(int n, nint arrays) { base.CreateVertexArrays(n, arrays); vertexArrays.Track(n, arrays); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created framebuffers, deleted when the layer is disposed.</remarks>
    public override void CreateFramebuffers(int n, nint framebuffers) { base.CreateFramebuffers(n, framebuffers); this.framebuffers.Track(n, framebuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created renderbuffers, deleted when the layer is disposed.</remarks>
    public override void CreateRenderbuffers(int n, nint renderbuffers) { base.CreateRenderbuffers(n, renderbuffers); this.renderbuffers.Track(n, renderbuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created samplers, deleted when the layer is disposed.</remarks>
    public override void CreateSamplers(int n, nint samplers) { base.CreateSamplers(n, samplers); this.samplers.Track(n, samplers); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created queries, deleted when the layer is disposed.</remarks>
    public override void CreateQueries(GlQueryTarget target, int n, nint ids) { base.CreateQueries(target, n, ids); queries.Track(n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program pipelines, deleted when the layer is disposed.</remarks>
    public override void CreateProgramPipelines(int n, nint pipelines) { base.CreateProgramPipelines(n, pipelines); programPipelines.Track(n, pipelines); }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created transform feedback objects, deleted when the layer is disposed.</remarks>
    public override void CreateTransformFeedbacks(int n, nint ids) { base.CreateTransformFeedbacks(n, ids); transformFeedbacks.Track(n, ids); }

    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created shader, deleted when the layer is disposed.</remarks>
    public override GlShaderHandle CreateShader(GlShaderType type) { var id = base.CreateShader(type); shaders.Track(id); return id; }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program, deleted when the layer is disposed.</remarks>
    public override GlProgramHandle CreateProgram() { var id = base.CreateProgram(); programs.Track(id); return id; }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created program, deleted when the layer is disposed.</remarks>
    public override GlProgramHandle CreateShaderProgramv(GlShaderType type, int count, nint strings) { var id = base.CreateShaderProgramv(type, count, strings); programs.Track(id); return id; }
    /// <inheritdoc/>
    /// <remarks>Layer: tracks the created sync object, deleted when the layer is disposed.</remarks>
    public override nint FenceSync(GlSyncCondition condition, GlSyncBehaviorFlags flags) { var sync = base.FenceSync(condition, flags); syncs.Add(sync); return sync; }

    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted textures and releases their tracked memory.</remarks>
    public override void DeleteTextures(int n, nint textures)
    {
        var ids = this.textures.Untrack(nameof(DeleteTextures), n, textures);
        foreach (var id in ids) { textureTargets.Remove(id); ReleaseTextureMemory(id); }
        base.DeleteTextures(n, textures);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted buffers and releases their tracked memory.</remarks>
    public override void DeleteBuffers(int n, nint buffers)
    {
        var ids = this.buffers.Untrack(nameof(DeleteBuffers), n, buffers);
        foreach (var id in ids) ReleaseBufferMemory(id);
        base.DeleteBuffers(n, buffers);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted vertex arrays.</remarks>
    public override void DeleteVertexArrays(int n, nint arrays) { vertexArrays.Untrack(nameof(DeleteVertexArrays), n, arrays); base.DeleteVertexArrays(n, arrays); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted framebuffers.</remarks>
    public override void DeleteFramebuffers(int n, nint framebuffers) { this.framebuffers.Untrack(nameof(DeleteFramebuffers), n, framebuffers); base.DeleteFramebuffers(n, framebuffers); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted renderbuffers and releases their tracked memory.</remarks>
    public override void DeleteRenderbuffers(int n, nint renderbuffers)
    {
        var ids = this.renderbuffers.Untrack(nameof(DeleteRenderbuffers), n, renderbuffers);
        foreach (var id in ids) ReleaseRenderbufferMemory(id);
        base.DeleteRenderbuffers(n, renderbuffers);
    }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted samplers.</remarks>
    public override void DeleteSamplers(int count, nint samplers) { this.samplers.Untrack(nameof(DeleteSamplers), count, samplers); base.DeleteSamplers(count, samplers); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted queries.</remarks>
    public override void DeleteQueries(int n, nint ids) { queries.Untrack(nameof(DeleteQueries), n, ids); base.DeleteQueries(n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program pipelines.</remarks>
    public override void DeleteProgramPipelines(int n, nint pipelines) { programPipelines.Untrack(nameof(DeleteProgramPipelines), n, pipelines); base.DeleteProgramPipelines(n, pipelines); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted transform feedback objects.</remarks>
    public override void DeleteTransformFeedbacks(int n, nint ids) { transformFeedbacks.Untrack(nameof(DeleteTransformFeedbacks), n, ids); base.DeleteTransformFeedbacks(n, ids); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted shader.</remarks>
    public override void DeleteShader(GlShaderHandle shader) { shaders.Untrack(nameof(DeleteShader), shader); base.DeleteShader(shader); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted program.</remarks>
    public override void DeleteProgram(GlProgramHandle program) { programs.Untrack(nameof(DeleteProgram), program); base.DeleteProgram(program); }
    /// <inheritdoc/>
    /// <remarks>Layer: stops tracking the deleted sync object.</remarks>
    public override void DeleteSync(nint sync)
    {
        if (!syncs.Remove(sync))
            throw new GlResourceNotTrackedException<nint>(nameof(DeleteSync), "sync", sync);
        base.DeleteSync(sync);
    }

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
        foreach (var shader in shaders.Drain()) base.DeleteShader(shader);
        foreach (var program in programs.Drain()) base.DeleteProgram(program);
    }

    private delegate void PluralDelete(int n, nint ids);

    private static void DeleteAll<THandle>(GlResourceSet<THandle> set, PluralDelete delete) where THandle : struct
    {
        if (set.Count == 0)
            return;
        var ids = set.DrainIds();
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
