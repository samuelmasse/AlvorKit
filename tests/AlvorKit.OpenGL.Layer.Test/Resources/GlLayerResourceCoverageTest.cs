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
}
