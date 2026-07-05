namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClipControl()"/>.</remarks>
    public override void ClipControl(GlClipControlOrigin origin, GlClipControlDepth depth)
    {
        state.clipControl.Set(nameof(ClipControl), (origin, depth));
        base.ClipControl(origin, depth);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetHint(GlHintTarget)"/> for the same target.</remarks>
    public override void Hint(GlHintTarget target, GlHintMode mode) { state.hintMap.Set(nameof(Hint), target, mode); base.Hint(target, mode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetMinSampleShading()"/>.</remarks>
    public override void MinSampleShading(float value) { state.minSampleShading.Set(nameof(MinSampleShading), value); base.MinSampleShading(value); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetSampleCoverage()"/>.</remarks>
    public override void SampleCoverage(float value, bool invert)
    {
        state.sampleCoverage.Set(nameof(SampleCoverage), (value, invert));
        base.SampleCoverage(value, invert);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetSampleMask(uint)"/> for the same word.</remarks>
    public override void SampleMaski(uint maskNumber, uint mask) { state.sampleMaskMap.Set(nameof(SampleMaski), maskNumber, mask); base.SampleMaski(maskNumber, mask); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPatchParameter(GlPatchParameterName)"/> for the same parameter.</remarks>
    public override void PatchParameteri(GlPatchParameterName pname, int value)
    {
        state.patchParameterMap.Set(nameof(PatchParameteri), pname, value);
        base.PatchParameteri(pname, value);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPointParameter(GlPointParameterName)"/> for the same parameter.</remarks>
    public override void PointParameterf(GlPointParameterName pname, float param)
    {
        state.pointParameterMap.Set(nameof(PointParameterf), pname, param);
        base.PointParameterf(pname, param);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPointParameter(GlPointParameterName)"/> for the same parameter.</remarks>
    public override void PointParameteri(GlPointParameterName pname, int param)
    {
        state.pointParameterMap.Set(nameof(PointParameteri), pname, param);
        base.PointParameteri(pname, param);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPixelStore(GlPixelStoreParameter)"/> for the same parameter.</remarks>
    public override void PixelStorei(GlPixelStoreParameter pname, int param)
    {
        state.pixelStoreMap.Set(nameof(PixelStorei), pname, param);
        base.PixelStorei(pname, param);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPixelStore(GlPixelStoreParameter)"/> for the same parameter.</remarks>
    public override void PixelStoref(GlPixelStoreParameter pname, float param)
    {
        state.pixelStoreMap.Set(nameof(PixelStoref), pname, param);
        base.PixelStoref(pname, param);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetReadBuffer()"/>.</remarks>
    public override void ReadBuffer(GlReadBufferMode src) { state.readBuffer.Set(nameof(ReadBuffer), src); base.ReadBuffer(src); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndTransformFeedback</c>.</remarks>
    public override void BeginTransformFeedback(GlPrimitiveType primitiveMode)
    {
        state.transformFeedbackActive.Set(nameof(BeginTransformFeedback), primitiveMode);
        base.BeginTransformFeedback(primitiveMode);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginTransformFeedback</c>.</remarks>
    public override void EndTransformFeedback() { state.transformFeedbackActive.Reset(nameof(EndTransformFeedback)); base.EndTransformFeedback(); }
}
