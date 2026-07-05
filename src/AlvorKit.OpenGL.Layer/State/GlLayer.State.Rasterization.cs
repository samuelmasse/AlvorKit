namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetCullFace()"/>.</remarks>
    public override void CullFace(GlTriangleFace mode) { state.cullFace.Set(nameof(CullFace), mode); base.CullFace(mode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetFrontFace()"/>.</remarks>
    public override void FrontFace(GlFrontFaceDirection mode) { state.frontFace.Set(nameof(FrontFace), mode); base.FrontFace(mode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPolygonMode()"/>.</remarks>
    public override void PolygonMode(GlTriangleFace face, GlPolygonMode mode) { state.polygonMode.Set(nameof(PolygonMode), (face, mode)); base.PolygonMode(face, mode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPolygonOffset()"/>. Cannot be combined with <c>glPolygonOffsetClamp</c>.</remarks>
    public override void PolygonOffset(float factor, float units)
    {
        if (state.polygonOffsetClamp.IsSet) throw new GlConflictException(nameof(PolygonOffset), nameof(PolygonOffsetClamp));
        state.polygonOffset.Set(nameof(PolygonOffset), (factor, units));
        base.PolygonOffset(factor, units);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPolygonOffsetClamp()"/>. Cannot be combined with <c>glPolygonOffset</c>.</remarks>
    public override void PolygonOffsetClamp(float factor, float units, float clamp)
    {
        if (state.polygonOffset.IsSet) throw new GlConflictException(nameof(PolygonOffsetClamp), nameof(PolygonOffset));
        state.polygonOffsetClamp.Set(nameof(PolygonOffsetClamp), (factor, units, clamp));
        base.PolygonOffsetClamp(factor, units, clamp);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetLineWidth()"/>.</remarks>
    public override void LineWidth(float width) { state.lineWidth.Set(nameof(LineWidth), width); base.LineWidth(width); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPointSize()"/>.</remarks>
    public override void PointSize(float size) { state.pointSize.Set(nameof(PointSize), size); base.PointSize(size); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetProvokingVertex()"/>.</remarks>
    public override void ProvokingVertex(GlVertexProvokingMode mode) { state.provokingVertex.Set(nameof(ProvokingVertex), mode); base.ProvokingVertex(mode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPrimitiveRestartIndex()"/>.</remarks>
    public override void PrimitiveRestartIndex(uint index) { state.primitiveRestartIndex.Set(nameof(PrimitiveRestartIndex), index); base.PrimitiveRestartIndex(index); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetLogicOp()"/>.</remarks>
    public override void LogicOp(GlLogicOp opcode) { state.logicOp.Set(nameof(LogicOp), opcode); base.LogicOp(opcode); }
}
