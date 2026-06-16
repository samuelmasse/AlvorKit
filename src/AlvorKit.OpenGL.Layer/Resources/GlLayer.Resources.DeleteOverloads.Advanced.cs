namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted renderbuffers to be unbound, then releases their tracked memory.
    /// </remarks>
    public override void DeleteRenderbuffers(ReadOnlySpan<GlRenderbufferHandle> renderbuffers) => base.DeleteRenderbuffers(renderbuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted renderbuffer to be unbound, then releases its tracked memory.
    /// </remarks>
    public override void DeleteRenderbuffer(GlRenderbufferHandle renderbuffer) => base.DeleteRenderbuffer(renderbuffer);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted samplers to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteSamplers(ReadOnlySpan<GlSamplerHandle> samplers) => base.DeleteSamplers(samplers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted sampler to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteSampler(GlSamplerHandle sampler) => base.DeleteSampler(sampler);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted queries to be inactive before removing them from tracking.
    /// </remarks>
    public override void DeleteQueries(ReadOnlySpan<GlQueryHandle> ids) => base.DeleteQueries(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted query to be inactive before removing it from tracking.
    /// </remarks>
    public override void DeleteQuery(GlQueryHandle id) => base.DeleteQuery(id);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted program pipelines to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteProgramPipelines(ReadOnlySpan<GlProgramPipelineHandle> pipelines) => base.DeleteProgramPipelines(pipelines);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted program pipeline to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteProgramPipeline(GlProgramPipelineHandle pipeline) => base.DeleteProgramPipeline(pipeline);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted transform feedback objects to be unbound before removing them from tracking.
    /// </remarks>
    public override void DeleteTransformFeedbacks(ReadOnlySpan<GlTransformFeedbackHandle> ids) => base.DeleteTransformFeedbacks(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: requires the deleted transform feedback object to be unbound before removing it from tracking.
    /// </remarks>
    public override void DeleteTransformFeedback(GlTransformFeedbackHandle id) => base.DeleteTransformFeedback(id);
}
