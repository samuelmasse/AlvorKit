namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>
    /// Layer: Restores <c>glCullFace</c> to <see cref="DefaultCullFace"/>.
    /// Must be paired with exactly one earlier call to <c>glCullFace</c>.
    /// </summary>
    public void ResetCullFace()
    {
        cullFace.Reset(nameof(CullFace));
        base.CullFace(DefaultCullFace);
    }

    /// <summary>
    /// Layer: Restores <c>glFrontFace</c> to <see cref="DefaultFrontFace"/>.
    /// Must be paired with exactly one earlier call to <c>glFrontFace</c>.
    /// </summary>
    public void ResetFrontFace()
    {
        frontFace.Reset(nameof(FrontFace));
        base.FrontFace(DefaultFrontFace);
    }

    /// <summary>
    /// Layer: Restores <c>glPolygonMode</c> to <see cref="DefaultPolygonMode"/>.
    /// Must be paired with exactly one earlier call to <c>glPolygonMode</c>.
    /// </summary>
    public void ResetPolygonMode()
    {
        polygonMode.Reset(nameof(PolygonMode));
        base.PolygonMode(DefaultPolygonMode.Face, DefaultPolygonMode.Mode);
    }

    /// <summary>
    /// Layer: Restores <c>glPolygonOffset</c> to <see cref="DefaultPolygonOffset"/>.
    /// Must be paired with exactly one earlier call to <c>glPolygonOffset</c>.
    /// </summary>
    public void ResetPolygonOffset()
    {
        polygonOffset.Reset(nameof(PolygonOffset));
        base.PolygonOffset(DefaultPolygonOffset.Factor, DefaultPolygonOffset.Units);
    }

    /// <summary>
    /// Layer: Restores <c>glPolygonOffsetClamp</c> to <see cref="DefaultPolygonOffsetClamp"/>.
    /// Must be paired with exactly one earlier call to <c>glPolygonOffsetClamp</c>.
    /// </summary>
    public void ResetPolygonOffsetClamp()
    {
        polygonOffsetClamp.Reset(nameof(PolygonOffsetClamp));
        base.PolygonOffsetClamp(
            DefaultPolygonOffsetClamp.Factor,
            DefaultPolygonOffsetClamp.Units,
            DefaultPolygonOffsetClamp.Clamp);
    }

    /// <summary>
    /// Layer: Restores <c>glLineWidth</c> to <see cref="DefaultLineWidth"/>.
    /// Must be paired with exactly one earlier call to <c>glLineWidth</c>.
    /// </summary>
    public void ResetLineWidth()
    {
        lineWidth.Reset(nameof(LineWidth));
        base.LineWidth(DefaultLineWidth);
    }

    /// <summary>
    /// Layer: Restores <c>glPointSize</c> to <see cref="DefaultPointSize"/>.
    /// Must be paired with exactly one earlier call to <c>glPointSize</c>.
    /// </summary>
    public void ResetPointSize()
    {
        pointSize.Reset(nameof(PointSize));
        base.PointSize(DefaultPointSize);
    }

    /// <summary>
    /// Layer: Restores <c>glProvokingVertex</c> to <see cref="DefaultProvokingVertex"/>.
    /// Must be paired with exactly one earlier call to <c>glProvokingVertex</c>.
    /// </summary>
    public void ResetProvokingVertex()
    {
        provokingVertex.Reset(nameof(ProvokingVertex));
        base.ProvokingVertex(DefaultProvokingVertex);
    }

    /// <summary>
    /// Layer: Restores <c>glPrimitiveRestartIndex</c> to <see cref="DefaultPrimitiveRestartIndex"/>.
    /// Must be paired with exactly one earlier call to <c>glPrimitiveRestartIndex</c>.
    /// </summary>
    public void ResetPrimitiveRestartIndex()
    {
        primitiveRestartIndex.Reset(nameof(PrimitiveRestartIndex));
        base.PrimitiveRestartIndex(DefaultPrimitiveRestartIndex);
    }

    /// <summary>
    /// Layer: Restores <c>glLogicOp</c> to <see cref="DefaultLogicOp"/>.
    /// Must be paired with exactly one earlier call to <c>glLogicOp</c>.
    /// </summary>
    public void ResetLogicOp()
    {
        logicOp.Reset(nameof(LogicOp));
        base.LogicOp(DefaultLogicOp);
    }

    /// <summary>
    /// Layer: Restores <c>glViewport</c> to <see cref="DefaultViewport"/>.
    /// Must be paired with exactly one earlier call to <c>glViewport</c>.
    /// </summary>
    public void ResetViewport()
    {
        viewport.Reset(nameof(Viewport));
        base.Viewport(DefaultViewport.X, DefaultViewport.Y, DefaultViewport.Width, DefaultViewport.Height);
    }

    /// <summary>
    /// Layer: Restores <c>glViewportIndexedf</c> for viewport <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glViewportIndexedf</c> for the same index.
    /// </summary>
    public void ResetViewportIndexed(uint index)
    {
        viewportMap.Reset(nameof(ViewportIndexedf), index);
        base.ViewportIndexedf(index, DefaultViewport.X, DefaultViewport.Y, DefaultViewport.Width, DefaultViewport.Height);
    }

    /// <summary>
    /// Layer: Restores <c>glScissor</c> to <see cref="DefaultScissor"/>.
    /// Must be paired with exactly one earlier call to <c>glScissor</c>.
    /// </summary>
    public void ResetScissor()
    {
        scissor.Reset(nameof(Scissor));
        base.Scissor(DefaultScissor.X, DefaultScissor.Y, DefaultScissor.Width, DefaultScissor.Height);
    }

    /// <summary>
    /// Layer: Restores <c>glScissorIndexed</c> for viewport <paramref name="index"/>.
    /// Must be paired with exactly one earlier call to <c>glScissorIndexed</c> for the same index.
    /// </summary>
    public void ResetScissorIndexed(uint index)
    {
        scissorMap.Reset(nameof(ScissorIndexed), index);
        base.ScissorIndexed(index, DefaultScissor.X, DefaultScissor.Y, DefaultScissor.Width, DefaultScissor.Height);
    }
}
