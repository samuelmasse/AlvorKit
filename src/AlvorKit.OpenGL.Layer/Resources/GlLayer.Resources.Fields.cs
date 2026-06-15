namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>Tracks live texture handles owned by this layer.</summary>
    private readonly GlResourceSet<GlTextureHandle> textures = new("texture", id => (GlTextureHandle)id, handle => (uint)handle);

    /// <summary>Tracks live buffer handles owned by this layer.</summary>
    private readonly GlResourceSet<GlBufferHandle> buffers = new("buffer", id => (GlBufferHandle)id, handle => (uint)handle);

    /// <summary>Tracks live vertex array handles owned by this layer.</summary>
    private readonly GlResourceSet<GlVertexArrayHandle> vertexArrays = new("vertex array", id => (GlVertexArrayHandle)id, handle => (uint)handle);

    /// <summary>Tracks live framebuffer handles owned by this layer.</summary>
    private readonly GlResourceSet<GlFramebufferHandle> framebuffers = new("framebuffer", id => (GlFramebufferHandle)id, handle => (uint)handle);

    /// <summary>Tracks live renderbuffer handles owned by this layer.</summary>
    private readonly GlResourceSet<GlRenderbufferHandle> renderbuffers = new("renderbuffer", id => (GlRenderbufferHandle)id, handle => (uint)handle);

    /// <summary>Tracks live sampler handles owned by this layer.</summary>
    private readonly GlResourceSet<GlSamplerHandle> samplers = new("sampler", id => (GlSamplerHandle)id, handle => (uint)handle);

    /// <summary>Tracks live query handles owned by this layer.</summary>
    private readonly GlResourceSet<GlQueryHandle> queries = new("query", id => (GlQueryHandle)id, handle => (uint)handle);

    /// <summary>Tracks live program pipeline handles owned by this layer.</summary>
    private readonly GlResourceSet<GlProgramPipelineHandle> programPipelines = new("program pipeline", id => (GlProgramPipelineHandle)id, handle => (uint)handle);

    /// <summary>Tracks live transform feedback handles owned by this layer.</summary>
    private readonly GlResourceSet<GlTransformFeedbackHandle> transformFeedbacks = new("transform feedback", id => (GlTransformFeedbackHandle)id, handle => (uint)handle);

    /// <summary>Tracks live shader handles owned by this layer.</summary>
    private readonly GlResourceSet<GlShaderHandle> shaders = new("shader", id => (GlShaderHandle)id, handle => (uint)handle);

    /// <summary>Tracks live program handles owned by this layer.</summary>
    private readonly GlResourceSet<GlProgramHandle> programs = new("program", id => (GlProgramHandle)id, handle => (uint)handle);

    /// <summary>Tracks live sync object pointers owned by this layer.</summary>
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
}
