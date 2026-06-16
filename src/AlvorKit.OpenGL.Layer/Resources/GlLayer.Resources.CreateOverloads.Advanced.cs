namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created renderbuffers, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateRenderbuffers(Span<GlRenderbufferHandle> renderbuffers) => base.CreateRenderbuffers(renderbuffers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created renderbuffer, deleted when the layer is disposed.
    /// </remarks>
    public override GlRenderbufferHandle CreateRenderbuffer() => base.CreateRenderbuffer();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created samplers, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateSamplers(Span<GlSamplerHandle> samplers) => base.CreateSamplers(samplers);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created sampler, deleted when the layer is disposed.
    /// </remarks>
    public override GlSamplerHandle CreateSampler() => base.CreateSampler();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created queries, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateQueries(GlQueryTarget target, Span<GlQueryHandle> ids) => base.CreateQueries(target, ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created query, deleted when the layer is disposed.
    /// </remarks>
    public override GlQueryHandle CreateQuery(GlQueryTarget target) => base.CreateQuery(target);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created program pipelines, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateProgramPipelines(Span<GlProgramPipelineHandle> pipelines) => base.CreateProgramPipelines(pipelines);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created program pipeline, deleted when the layer is disposed.
    /// </remarks>
    public override GlProgramPipelineHandle CreateProgramPipeline() => base.CreateProgramPipeline();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created transform feedback objects, deleted when the layer is disposed.
    /// </remarks>
    public override void CreateTransformFeedbacks(Span<GlTransformFeedbackHandle> ids) => base.CreateTransformFeedbacks(ids);

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created transform feedback object, deleted when the layer is disposed.
    /// </remarks>
    public override GlTransformFeedbackHandle CreateTransformFeedback() => base.CreateTransformFeedback();

    /// <inheritdoc/>
    /// <remarks>
    /// Layer: tracks the created program, deleted when the layer is disposed.
    /// </remarks>
    public override GlProgramHandle CreateShaderProgramv(GlShaderType type, ReadOnlySpan<string> strings) => base.CreateShaderProgramv(type, strings);
}
