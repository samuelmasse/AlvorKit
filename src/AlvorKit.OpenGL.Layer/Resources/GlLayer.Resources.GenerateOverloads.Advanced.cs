namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated renderbuffers, deleted when the layer is disposed.
    /// </remarks>
    public override void GenRenderbuffers(Span<GlRenderbufferHandle> renderbuffers) => base.GenRenderbuffers(renderbuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated renderbuffer, deleted when the layer is disposed.
    /// </remarks>
    public override GlRenderbufferHandle GenRenderbuffer() => base.GenRenderbuffer();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated samplers, deleted when the layer is disposed.
    /// </remarks>
    public override void GenSamplers(Span<GlSamplerHandle> samplers) => base.GenSamplers(samplers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated sampler, deleted when the layer is disposed.
    /// </remarks>
    public override GlSamplerHandle GenSampler() => base.GenSampler();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated queries, deleted when the layer is disposed.
    /// </remarks>
    public override void GenQueries(Span<GlQueryHandle> ids) => base.GenQueries(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated query, deleted when the layer is disposed.
    /// </remarks>
    public override GlQueryHandle GenQuery() => base.GenQuery();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated program pipelines, deleted when the layer is disposed.
    /// </remarks>
    public override void GenProgramPipelines(Span<GlProgramPipelineHandle> pipelines) => base.GenProgramPipelines(pipelines);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated program pipeline, deleted when the layer is disposed.
    /// </remarks>
    public override GlProgramPipelineHandle GenProgramPipeline() => base.GenProgramPipeline();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated transform feedback objects, deleted when the layer is disposed.
    /// </remarks>
    public override void GenTransformFeedbacks(Span<GlTransformFeedbackHandle> ids) => base.GenTransformFeedbacks(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the generated transform feedback object, deleted when the layer is disposed.
    /// </remarks>
    public override GlTransformFeedbackHandle GenTransformFeedback() => base.GenTransformFeedback();
}
