namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Requires every deleted buffer to be detached from tracked buffer binding slots.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="buffers">The buffers requested for deletion.</param>
    private void RequireBuffersUnbound(string function, ReadOnlySpan<GlBufferHandle> buffers)
    {
        foreach (var buffer in buffers)
            RequireUnbound(function, "buffer", buffer, IsBufferBound((uint)buffer));
    }

    /// <summary>Requires every deleted texture to be detached from tracked texture and image slots.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="textures">The textures requested for deletion.</param>
    private void RequireTexturesUnbound(string function, ReadOnlySpan<GlTextureHandle> textures)
    {
        foreach (var texture in textures)
            RequireUnbound(function, "texture", texture, IsTextureBound((uint)texture));
    }

    /// <summary>Requires every deleted vertex array to be unbound from the current vertex-array slot.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="arrays">The vertex arrays requested for deletion.</param>
    private void RequireVertexArraysUnbound(string function, ReadOnlySpan<GlVertexArrayHandle> arrays)
    {
        foreach (var array in arrays)
            RequireUnbound(function, "vertex array", array, vertexArray.IsBound((uint)array));
    }

    /// <summary>Requires every deleted framebuffer to be unbound from tracked framebuffer slots.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="framebuffers">The framebuffers requested for deletion.</param>
    private void RequireFramebuffersUnbound(string function, ReadOnlySpan<GlFramebufferHandle> framebuffers)
    {
        foreach (var framebuffer in framebuffers)
            RequireUnbound(function, "framebuffer", framebuffer, IsFramebufferBound((uint)framebuffer));
    }

    /// <summary>Requires every deleted renderbuffer to be unbound from the current renderbuffer slot.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="renderbuffers">The renderbuffers requested for deletion.</param>
    private void RequireRenderbuffersUnbound(string function, ReadOnlySpan<GlRenderbufferHandle> renderbuffers)
    {
        foreach (var renderbufferHandle in renderbuffers)
            RequireUnbound(function, "renderbuffer", renderbufferHandle, renderbuffer.IsBound((uint)renderbufferHandle));
    }

    /// <summary>Requires every deleted sampler to be unbound from tracked sampler slots.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="samplers">The samplers requested for deletion.</param>
    private void RequireSamplersUnbound(string function, ReadOnlySpan<GlSamplerHandle> samplers)
    {
        foreach (var sampler in samplers)
            RequireUnbound(function, "sampler", sampler, samplerBinds.ContainsValue((uint)sampler));
    }

    /// <summary>Requires every deleted query to be inactive in tracked query scopes.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="queries">The queries requested for deletion.</param>
    private void RequireQueriesUnbound(string function, ReadOnlySpan<GlQueryHandle> queries)
    {
        foreach (var query in queries)
            RequireUnbound(function, "query", query, IsQueryBound((uint)query));
    }

    /// <summary>Requires every deleted program pipeline to be unbound from the current pipeline slot.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="pipelines">The program pipelines requested for deletion.</param>
    private void RequireProgramPipelinesUnbound(string function, ReadOnlySpan<GlProgramPipelineHandle> pipelines)
    {
        foreach (var pipeline in pipelines)
            RequireUnbound(function, "program pipeline", pipeline, programPipeline.IsBound((uint)pipeline));
    }

    /// <summary>Requires every deleted transform feedback object to be unbound from the current slot.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="feedbacks">The transform feedback objects requested for deletion.</param>
    private void RequireTransformFeedbacksUnbound(string function, ReadOnlySpan<GlTransformFeedbackHandle> feedbacks)
    {
        foreach (var feedback in feedbacks)
            RequireUnbound(function, "transform feedback", feedback, transformFeedbackObject.IsBound((uint)feedback));
    }

    /// <summary>Requires a deleted program to be unused.</summary>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="programHandle">The program requested for deletion.</param>
    private void RequireProgramUnbound(string function, GlProgramHandle programHandle) =>
        RequireUnbound(function, "program", programHandle, program.IsBound((uint)programHandle));

    /// <summary>Returns whether a buffer is live-bound through any tracked buffer binding shape.</summary>
    /// <param name="buffer">The buffer id to inspect.</param>
    /// <returns><see langword="true"/> when the buffer is bound.</returns>
    private bool IsBufferBound(uint buffer) =>
        bufferBinds.ContainsValue(buffer) || indexedBufferBinds.ContainsValue(buffer) || vertexBufferBinds.ContainsValue(buffer);

    /// <summary>Returns whether a texture is live-bound through any tracked texture binding shape.</summary>
    /// <param name="texture">The texture id to inspect.</param>
    /// <returns><see langword="true"/> when the texture is bound.</returns>
    private bool IsTextureBound(uint texture) => textureBinds.ContainsValue(texture) || imageTextureBinds.ContainsValue(texture);

    /// <summary>Returns whether a framebuffer is bound to a tracked framebuffer target.</summary>
    /// <param name="framebuffer">The framebuffer id to inspect.</param>
    /// <returns><see langword="true"/> when the framebuffer is bound.</returns>
    private bool IsFramebufferBound(uint framebuffer) => readFramebuffer.IsBound(framebuffer) || drawFramebuffer.IsBound(framebuffer);

    /// <summary>Returns whether a query is live-bound or active through any tracked query shape.</summary>
    /// <param name="query">The query id to inspect.</param>
    /// <returns><see langword="true"/> when the query is active.</returns>
    private bool IsQueryBound(uint query) =>
        queryBinds.ContainsValue(query) || queryIndexedBinds.ContainsValue(query) || conditionalRender.IsBound(query);

    /// <summary>Throws when a delete candidate is still tracked as bound.</summary>
    /// <typeparam name="THandle">The typed GL handle being checked.</typeparam>
    /// <param name="function">The GL delete function being validated.</param>
    /// <param name="resourceName">The display name for diagnostics.</param>
    /// <param name="handle">The resource requested for deletion.</param>
    /// <param name="bound">Whether the resource is live-bound.</param>
    private static void RequireUnbound<THandle>(string function, string resourceName, THandle handle, bool bound)
    {
        if (bound)
            throw new GlBindConflictException(function, $"attempted to delete {resourceName} {handle}, but it is still bound; unbind it first.");
    }
}
