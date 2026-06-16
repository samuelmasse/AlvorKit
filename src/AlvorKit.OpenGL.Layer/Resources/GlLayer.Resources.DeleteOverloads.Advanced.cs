namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteRenderbuffers(int, nint)"/>, pinning <paramref name="renderbuffers"/> for the call.
    /// Layer: requires the deleted renderbuffers to be unbound, then releases their tracked memory.
    /// </remarks>
    public override void DeleteRenderbuffers(ReadOnlySpan<GlRenderbufferHandle> renderbuffers) => base.DeleteRenderbuffers(renderbuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteRenderbuffers(int, nint)"/> with one stack-addressed renderbuffer.
    /// Layer: requires the deleted renderbuffer to be unbound, then releases its tracked memory.
    /// </remarks>
    public override void DeleteRenderbuffer(GlRenderbufferHandle renderbuffer) => base.DeleteRenderbuffer(renderbuffer);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteSamplers(int, nint)"/>, pinning <paramref name="samplers"/> for the call.
    /// Layer: requires the deleted samplers to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteSamplers(ReadOnlySpan<GlSamplerHandle> samplers) => base.DeleteSamplers(samplers);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteSamplers(int, nint)"/> with one stack-addressed sampler.
    /// Layer: requires the deleted sampler to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteSampler(GlSamplerHandle sampler) => base.DeleteSampler(sampler);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteQueries(int, nint)"/>, pinning <paramref name="ids"/> for the call.
    /// Layer: requires the deleted queries to be inactive before removing them from tracking.
    /// </remarks>
    public override void DeleteQueries(ReadOnlySpan<GlQueryHandle> ids) => base.DeleteQueries(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteQueries(int, nint)"/> with one stack-addressed query.
    /// Layer: requires the deleted query to be inactive before removing it from tracking.
    /// </remarks>
    public override void DeleteQuery(GlQueryHandle id) => base.DeleteQuery(id);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteProgramPipelines(int, nint)"/>, pinning <paramref name="pipelines"/> for the call.
    /// Layer: requires the deleted program pipelines to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteProgramPipelines(ReadOnlySpan<GlProgramPipelineHandle> pipelines) => base.DeleteProgramPipelines(pipelines);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteProgramPipelines(int, nint)"/> with one stack-addressed program pipeline.
    /// Layer: requires the deleted program pipeline to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteProgramPipeline(GlProgramPipelineHandle pipeline) => base.DeleteProgramPipeline(pipeline);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteTransformFeedbacks(int, nint)"/>, pinning <paramref name="ids"/> for the call.
    /// Layer: requires the deleted transform feedback objects to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteTransformFeedbacks(ReadOnlySpan<GlTransformFeedbackHandle> ids) => base.DeleteTransformFeedbacks(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Convenience overload. Calls <see cref="DeleteTransformFeedbacks(int, nint)"/> with one stack-addressed transform feedback object.
    /// Layer: requires the deleted transform feedback object to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteTransformFeedback(GlTransformFeedbackHandle id) => base.DeleteTransformFeedback(id);
}
