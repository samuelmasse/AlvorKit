namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Exercises resource lifecycle APIs that mirror the primary resource tests.
/// </summary>
[TestClass]
public unsafe class GlLayerResourceCoverageTest
{
    /// <summary>Generated resource families enter and leave their tracking sets.</summary>
    [TestMethod]
    public void GeneratedResourceSets_ExposeAndDeleteAllFamilies()
    {
        var gl = new GlLayer(new RecordingGl());
        uint* id = stackalloc uint[1];

        gl.GenFramebuffers(1, (nint)id);
        var framebuffer = (GlFramebufferHandle)id[0];
        Assert.IsTrue(gl.Framebuffers.Contains(framebuffer));
        gl.DeleteFramebuffers(1, (nint)id);

        gl.GenSamplers(1, (nint)id);
        var sampler = (GlSamplerHandle)id[0];
        Assert.IsTrue(gl.Samplers.Contains(sampler));
        gl.DeleteSamplers(1, (nint)id);

        gl.GenQueries(1, (nint)id);
        var query = (GlQueryHandle)id[0];
        Assert.IsTrue(gl.Queries.Contains(query));
        gl.DeleteQueries(1, (nint)id);

        gl.GenProgramPipelines(1, (nint)id);
        var pipeline = (GlProgramPipelineHandle)id[0];
        Assert.IsTrue(gl.ProgramPipelines.Contains(pipeline));
        gl.DeleteProgramPipelines(1, (nint)id);

        gl.GenTransformFeedbacks(1, (nint)id);
        var feedback = (GlTransformFeedbackHandle)id[0];
        Assert.IsTrue(gl.TransformFeedbacks.Contains(feedback));
        gl.DeleteTransformFeedbacks(1, (nint)id);

        Assert.AreEqual(0, gl.Framebuffers.Count);
        Assert.AreEqual(0, gl.Samplers.Count);
        Assert.AreEqual(0, gl.Queries.Count);
        Assert.AreEqual(0, gl.ProgramPipelines.Count);
        Assert.AreEqual(0, gl.TransformFeedbacks.Count);
    }

    /// <summary>Created resource families enter and leave their tracking sets.</summary>
    [TestMethod]
    public void CreatedResourceSets_ExposeAndDeleteAllFamilies()
    {
        var gl = new GlLayer(new RecordingGl());
        uint* id = stackalloc uint[] { 101 };

        gl.CreateTextures(GlTextureTarget.Texture2D, 1, (nint)id);
        Assert.IsTrue(gl.Textures.Contains((GlTextureHandle)101u));
        gl.DeleteTextures(1, (nint)id);

        id[0] = 0;
        gl.CreateTextures(GlTextureTarget.Texture2D, 1, (nint)id);
        gl.DeleteTextures(1, (nint)id);

        id[0] = 102;
        gl.CreateBuffers(1, (nint)id);
        gl.DeleteBuffers(1, (nint)id);
        id[0] = 103;
        gl.CreateVertexArrays(1, (nint)id);
        Assert.IsTrue(gl.VertexArrays.Contains((GlVertexArrayHandle)103u));
        gl.DeleteVertexArrays(1, (nint)id);
        id[0] = 104;
        gl.CreateFramebuffers(1, (nint)id);
        gl.DeleteFramebuffers(1, (nint)id);
        id[0] = 105;
        gl.CreateRenderbuffers(1, (nint)id);
        Assert.IsTrue(gl.Renderbuffers.Contains((GlRenderbufferHandle)105u));
        gl.DeleteRenderbuffers(1, (nint)id);
        id[0] = 106;
        gl.CreateSamplers(1, (nint)id);
        gl.DeleteSamplers(1, (nint)id);
        id[0] = 107;
        gl.CreateQueries(GlQueryTarget.SamplesPassed, 1, (nint)id);
        gl.DeleteQueries(1, (nint)id);
        id[0] = 108;
        gl.CreateProgramPipelines(1, (nint)id);
        gl.DeleteProgramPipelines(1, (nint)id);
        id[0] = 109;
        gl.CreateTransformFeedbacks(1, (nint)id);
        gl.DeleteTransformFeedbacks(1, (nint)id);

        Assert.AreEqual(0, gl.Textures.Count);
        Assert.AreEqual(0, gl.Buffers.Count);
        Assert.AreEqual(0, gl.VertexArrays.Count);
        Assert.AreEqual(0, gl.Framebuffers.Count);
        Assert.AreEqual(0, gl.Renderbuffers.Count);
        Assert.AreEqual(0, gl.Samplers.Count);
        Assert.AreEqual(0, gl.Queries.Count);
        Assert.AreEqual(0, gl.ProgramPipelines.Count);
        Assert.AreEqual(0, gl.TransformFeedbacks.Count);
    }

    /// <summary>Span create overloads route through the tracked resource creation path.</summary>
    [TestMethod]
    public void SpanCreateOverloads_TrackEveryCreatedFamily()
    {
        var gl = new GlLayer(new FactoryGl());

        Span<GlTextureHandle> textures = stackalloc GlTextureHandle[1];
        Span<GlBufferHandle> buffers = stackalloc GlBufferHandle[1];
        Span<GlVertexArrayHandle> arrays = stackalloc GlVertexArrayHandle[1];
        Span<GlFramebufferHandle> framebuffers = stackalloc GlFramebufferHandle[1];
        Span<GlRenderbufferHandle> renderbuffers = stackalloc GlRenderbufferHandle[1];
        Span<GlSamplerHandle> samplers = stackalloc GlSamplerHandle[1];
        Span<GlQueryHandle> queries = stackalloc GlQueryHandle[1];
        Span<GlProgramPipelineHandle> pipelines = stackalloc GlProgramPipelineHandle[1];
        Span<GlTransformFeedbackHandle> feedbacks = stackalloc GlTransformFeedbackHandle[1];

        gl.CreateTextures(GlTextureTarget.Texture2D, textures);
        gl.CreateBuffers(buffers);
        gl.CreateVertexArrays(arrays);
        gl.CreateFramebuffers(framebuffers);
        gl.CreateRenderbuffers(renderbuffers);
        gl.CreateSamplers(samplers);
        gl.CreateQueries(GlQueryTarget.SamplesPassed, queries);
        gl.CreateProgramPipelines(pipelines);
        gl.CreateTransformFeedbacks(feedbacks);

        Assert.IsTrue(gl.Textures.Contains(textures[0]));
        Assert.IsTrue(gl.Buffers.Contains(buffers[0]));
        Assert.IsTrue(gl.VertexArrays.Contains(arrays[0]));
        Assert.IsTrue(gl.Framebuffers.Contains(framebuffers[0]));
        Assert.IsTrue(gl.Renderbuffers.Contains(renderbuffers[0]));
        Assert.IsTrue(gl.Samplers.Contains(samplers[0]));
        Assert.IsTrue(gl.Queries.Contains(queries[0]));
        Assert.IsTrue(gl.ProgramPipelines.Contains(pipelines[0]));
        Assert.IsTrue(gl.TransformFeedbacks.Contains(feedbacks[0]));
    }

    /// <summary>Singular create overloads return tracked handles from the same raw creation calls.</summary>
    [TestMethod]
    public void SingularCreateOverloads_TrackReturnedHandles()
    {
        var gl = new GlLayer(new FactoryGl());

        var texture = gl.CreateTexture(GlTextureTarget.Texture2D);
        var buffer = gl.CreateBuffer();
        var array = gl.CreateVertexArray();
        var framebuffer = gl.CreateFramebuffer();
        var renderbuffer = gl.CreateRenderbuffer();
        var sampler = gl.CreateSampler();
        var query = gl.CreateQuery(GlQueryTarget.SamplesPassed);
        var pipeline = gl.CreateProgramPipeline();
        var feedback = gl.CreateTransformFeedback();
        var shaderProgram = gl.CreateShaderProgramv(GlShaderType.FragmentShader, ["void main() {}"]);

        Assert.IsTrue(gl.Textures.Contains(texture));
        Assert.IsTrue(gl.Buffers.Contains(buffer));
        Assert.IsTrue(gl.VertexArrays.Contains(array));
        Assert.IsTrue(gl.Framebuffers.Contains(framebuffer));
        Assert.IsTrue(gl.Renderbuffers.Contains(renderbuffer));
        Assert.IsTrue(gl.Samplers.Contains(sampler));
        Assert.IsTrue(gl.Queries.Contains(query));
        Assert.IsTrue(gl.ProgramPipelines.Contains(pipeline));
        Assert.IsTrue(gl.TransformFeedbacks.Contains(feedback));
        Assert.IsTrue(gl.Programs.Contains(shaderProgram));
    }

    /// <summary>Span generation overloads route through the tracked resource generation path.</summary>
    [TestMethod]
    public void SpanGenOverloads_TrackEveryGeneratedFamily()
    {
        var gl = new GlLayer(new RecordingGl());

        Span<GlTextureHandle> textures = stackalloc GlTextureHandle[1];
        Span<GlBufferHandle> buffers = stackalloc GlBufferHandle[1];
        Span<GlVertexArrayHandle> arrays = stackalloc GlVertexArrayHandle[1];
        Span<GlFramebufferHandle> framebuffers = stackalloc GlFramebufferHandle[1];
        Span<GlRenderbufferHandle> renderbuffers = stackalloc GlRenderbufferHandle[1];
        Span<GlSamplerHandle> samplers = stackalloc GlSamplerHandle[1];
        Span<GlQueryHandle> queries = stackalloc GlQueryHandle[1];
        Span<GlProgramPipelineHandle> pipelines = stackalloc GlProgramPipelineHandle[1];
        Span<GlTransformFeedbackHandle> feedbacks = stackalloc GlTransformFeedbackHandle[1];

        gl.GenTextures(textures);
        gl.GenBuffers(buffers);
        gl.GenVertexArrays(arrays);
        gl.GenFramebuffers(framebuffers);
        gl.GenRenderbuffers(renderbuffers);
        gl.GenSamplers(samplers);
        gl.GenQueries(queries);
        gl.GenProgramPipelines(pipelines);
        gl.GenTransformFeedbacks(feedbacks);

        Assert.IsTrue(gl.Textures.Contains(textures[0]));
        Assert.IsTrue(gl.Buffers.Contains(buffers[0]));
        Assert.IsTrue(gl.VertexArrays.Contains(arrays[0]));
        Assert.IsTrue(gl.Framebuffers.Contains(framebuffers[0]));
        Assert.IsTrue(gl.Renderbuffers.Contains(renderbuffers[0]));
        Assert.IsTrue(gl.Samplers.Contains(samplers[0]));
        Assert.IsTrue(gl.Queries.Contains(queries[0]));
        Assert.IsTrue(gl.ProgramPipelines.Contains(pipelines[0]));
        Assert.IsTrue(gl.TransformFeedbacks.Contains(feedbacks[0]));
    }

    /// <summary>Typed span delete overloads route through the same tracking and backend delete path.</summary>
    [TestMethod]
    public void SpanDeleteOverloads_DeleteEveryTrackedFamily()
    {
        var inner = new RecordingGl();
        var gl = new GlLayer(inner);

        var buffer = gl.GenBuffer();
        var texture = gl.GenTexture();
        var vertexArray = gl.GenVertexArray();
        var framebuffer = gl.GenFramebuffer();
        var renderbuffer = gl.GenRenderbuffer();
        var sampler = gl.GenSampler();
        var query = gl.GenQuery();
        var pipeline = gl.GenProgramPipeline();
        var feedback = gl.GenTransformFeedback();

        gl.DeleteBuffers([buffer]);
        gl.DeleteTextures([texture]);
        gl.DeleteVertexArrays([vertexArray]);
        gl.DeleteFramebuffers([framebuffer]);
        gl.DeleteRenderbuffers([renderbuffer]);
        gl.DeleteSamplers([sampler]);
        gl.DeleteQueries([query]);
        gl.DeleteProgramPipelines([pipeline]);
        gl.DeleteTransformFeedbacks([feedback]);

        CollectionAssert.AreEquivalent(
            new[]
            {
                (uint)buffer,
                (uint)texture,
                (uint)vertexArray,
                (uint)framebuffer,
                (uint)renderbuffer,
                (uint)sampler,
                (uint)query,
                (uint)pipeline,
                (uint)feedback,
            },
            inner.Deleted);
    }

    /// <summary>Singular resource factories track their handles until each explicit delete.</summary>
    [TestMethod]
    public void SingularResourceSets_ExposeAndDeleteAllFamilies()
    {
        var gl = new GlLayer(new RecordingGl());

        var shader = gl.CreateShader(GlShaderType.VertexShader);
        Assert.IsTrue(gl.Shaders.Contains(shader));
        gl.DeleteShader(shader);
        Assert.IsFalse(gl.Shaders.Contains(shader));

        var program = gl.CreateProgram();
        Assert.IsTrue(gl.Programs.Contains(program));
        gl.DeleteProgram(program);
        Assert.IsFalse(gl.Programs.Contains(program));

        var shaderProgram = gl.CreateShaderProgramv(GlShaderType.FragmentShader, 0, 0);
        gl.DeleteProgram(shaderProgram);
        Assert.IsFalse(gl.Programs.Contains(shaderProgram));

        var sync = gl.FenceSync(GlSyncCondition.SyncGpuCommandsComplete, 0);
        Assert.IsTrue(gl.Syncs.Contains(sync));
        gl.DeleteSync(sync);
        Assert.IsFalse(gl.Syncs.Contains(sync));
        Assert.Throws<GlResourceNotTrackedException<nint>>(() => gl.DeleteSync(sync));
    }

    private sealed unsafe class FactoryGl : GlNoop
    {
        private uint next = 500;

        public override void CreateTextures(GlTextureTarget target, int n, nint textures) => Fill(n, textures);
        public override void CreateBuffers(int n, nint buffers) => Fill(n, buffers);
        public override void CreateVertexArrays(int n, nint arrays) => Fill(n, arrays);
        public override void CreateFramebuffers(int n, nint framebuffers) => Fill(n, framebuffers);
        public override void CreateRenderbuffers(int n, nint renderbuffers) => Fill(n, renderbuffers);
        public override void CreateSamplers(int n, nint samplers) => Fill(n, samplers);
        public override void CreateQueries(GlQueryTarget target, int n, nint ids) => Fill(n, ids);
        public override void CreateProgramPipelines(int n, nint pipelines) => Fill(n, pipelines);
        public override void CreateTransformFeedbacks(int n, nint ids) => Fill(n, ids);
        public override GlProgramHandle CreateShaderProgramv(GlShaderType type, int count, nint strings) => (GlProgramHandle)next++;

        private void Fill(int n, nint p)
        {
            var ids = (uint*)p;
            for (var i = 0; i < n; i++)
                ids[i] = next++;
        }
    }
}
