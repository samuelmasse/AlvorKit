namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class GlBinTest
{
    /// <summary>Bins drain deferred OpenGL deletes through the root GL layer and detach disposed children.</summary>
    [TestMethod]
    public void Empty_DeletesQueuedResourcesAndTracksChildren()
    {
        var backend = new RecordingGl();
        using var gl = new RootGl(backend);
        var root = new RootBin(gl);
        var child = new GlBin(gl, root);
        var texture = gl.GenTexture();
        var buffer = gl.GenBuffer();
        var renderbuffer = gl.GenRenderbuffer();
        var vertexArray = gl.GenVertexArray();
        var framebuffer = gl.GenFramebuffer();
        var sampler = gl.GenSampler();
        var query = gl.GenQuery();
        var programPipeline = gl.GenProgramPipeline();
        var transformFeedback = gl.GenTransformFeedback();
        var shader = gl.CreateShader(GlShaderType.VertexShader);
        var program = gl.CreateProgram();
        var sync = gl.FenceSync(GlSyncCondition.SyncGpuCommandsComplete, 0);

        root.DeleteTexture(texture);
        root.DeleteBuffer(buffer);
        root.DeleteRenderbuffer(renderbuffer);
        root.DeleteVertexArray(vertexArray);
        root.DeleteFramebuffer(framebuffer);
        root.DeleteSampler(sampler);
        root.DeleteQuery(query);
        root.DeleteProgramPipeline(programPipeline);
        root.DeleteTransformFeedback(transformFeedback);
        root.DeleteShader(shader);
        root.DeleteProgram(program);
        root.DeleteSync(sync);
        root.Empty();
        root.Empty();
        child.Dispose();
        root.Dispose();

        CollectionAssert.AreEqual(new[] { texture.Handle }, backend.DeletedTextures);
        CollectionAssert.AreEqual(new[] { buffer.Handle }, backend.DeletedBuffers);
        CollectionAssert.AreEqual(new[] { renderbuffer.Handle }, backend.DeletedRenderbuffers);
        CollectionAssert.AreEqual(new[] { vertexArray.Handle }, backend.DeletedVertexArrays);
        CollectionAssert.AreEqual(new[] { framebuffer.Handle }, backend.DeletedFramebuffers);
        CollectionAssert.AreEqual(new[] { sampler.Handle }, backend.DeletedSamplers);
        CollectionAssert.AreEqual(new[] { query.Handle }, backend.DeletedQueries);
        CollectionAssert.AreEqual(new[] { programPipeline.Handle }, backend.DeletedProgramPipelines);
        CollectionAssert.AreEqual(new[] { transformFeedback.Handle }, backend.DeletedTransformFeedbacks);
        CollectionAssert.AreEqual(new[] { shader.Handle }, backend.DeletedShaders);
        CollectionAssert.AreEqual(new[] { program.Handle }, backend.DeletedPrograms);
        CollectionAssert.AreEqual(new[] { sync }, backend.DeletedSyncs);
        Assert.AreEqual(0, root.Children.Length);
    }

    private unsafe sealed class RecordingGl : GlNoop
    {
        private uint next = 1;

        public List<uint> DeletedTextures { get; } = [];

        public List<uint> DeletedBuffers { get; } = [];

        public List<uint> DeletedRenderbuffers { get; } = [];

        public List<uint> DeletedVertexArrays { get; } = [];

        public List<uint> DeletedFramebuffers { get; } = [];

        public List<uint> DeletedSamplers { get; } = [];

        public List<uint> DeletedQueries { get; } = [];

        public List<uint> DeletedProgramPipelines { get; } = [];

        public List<uint> DeletedTransformFeedbacks { get; } = [];

        public List<uint> DeletedShaders { get; } = [];

        public List<uint> DeletedPrograms { get; } = [];

        public List<nint> DeletedSyncs { get; } = [];

        public override void GenTextures(int n, nint textures)
        {
            var span = new Span<GlTextureHandle>((void*)textures, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenBuffers(int n, nint buffers)
        {
            var span = new Span<GlBufferHandle>((void*)buffers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenRenderbuffers(int n, nint renderbuffers)
        {
            var span = new Span<GlRenderbufferHandle>((void*)renderbuffers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenVertexArrays(int n, nint arrays)
        {
            var span = new Span<GlVertexArrayHandle>((void*)arrays, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenFramebuffers(int n, nint framebuffers)
        {
            var span = new Span<GlFramebufferHandle>((void*)framebuffers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenSamplers(int n, nint samplers)
        {
            var span = new Span<GlSamplerHandle>((void*)samplers, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenQueries(int n, nint queries)
        {
            var span = new Span<GlQueryHandle>((void*)queries, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenProgramPipelines(int n, nint pipelines)
        {
            var span = new Span<GlProgramPipelineHandle>((void*)pipelines, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override void GenTransformFeedbacks(int n, nint ids)
        {
            var span = new Span<GlTransformFeedbackHandle>((void*)ids, n);
            for (var i = 0; i < span.Length; i++)
                span[i] = new(next++);
        }

        public override GlShaderHandle CreateShader(GlShaderType type) => new(next++);

        public override GlProgramHandle CreateProgram() => new(next++);

        public override nint FenceSync(GlSyncCondition condition, GlSyncBehaviorFlags flags) => (nint)next++;

        public override void DeleteTextures(int n, nint textures)
        {
            var span = new Span<GlTextureHandle>((void*)textures, n);
            foreach (var texture in span)
                DeletedTextures.Add(texture.Handle);
        }

        public override void DeleteBuffers(int n, nint buffers)
        {
            var span = new Span<GlBufferHandle>((void*)buffers, n);
            foreach (var buffer in span)
                DeletedBuffers.Add(buffer.Handle);
        }

        public override void DeleteRenderbuffers(int n, nint renderbuffers)
        {
            var span = new Span<GlRenderbufferHandle>((void*)renderbuffers, n);
            foreach (var renderbuffer in span)
                DeletedRenderbuffers.Add(renderbuffer.Handle);
        }

        public override void DeleteVertexArrays(int n, nint arrays)
        {
            var span = new Span<GlVertexArrayHandle>((void*)arrays, n);
            foreach (var array in span)
                DeletedVertexArrays.Add(array.Handle);
        }

        public override void DeleteFramebuffers(int n, nint framebuffers)
        {
            var span = new Span<GlFramebufferHandle>((void*)framebuffers, n);
            foreach (var framebuffer in span)
                DeletedFramebuffers.Add(framebuffer.Handle);
        }

        public override void DeleteSamplers(int n, nint samplers)
        {
            var span = new Span<GlSamplerHandle>((void*)samplers, n);
            foreach (var sampler in span)
                DeletedSamplers.Add(sampler.Handle);
        }

        public override void DeleteQueries(int n, nint queries)
        {
            var span = new Span<GlQueryHandle>((void*)queries, n);
            foreach (var query in span)
                DeletedQueries.Add(query.Handle);
        }

        public override void DeleteProgramPipelines(int n, nint pipelines)
        {
            var span = new Span<GlProgramPipelineHandle>((void*)pipelines, n);
            foreach (var pipeline in span)
                DeletedProgramPipelines.Add(pipeline.Handle);
        }

        public override void DeleteTransformFeedbacks(int n, nint ids)
        {
            var span = new Span<GlTransformFeedbackHandle>((void*)ids, n);
            foreach (var id in span)
                DeletedTransformFeedbacks.Add(id.Handle);
        }

        public override void DeleteShader(GlShaderHandle shader) => DeletedShaders.Add(shader.Handle);

        public override void DeleteProgram(GlProgramHandle program) => DeletedPrograms.Add(program.Handle);

        public override void DeleteSync(nint sync) => DeletedSyncs.Add(sync);
    }
}
