namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Exercises rasterization, viewport, scissor, and parameter state families.
/// </summary>
[TestClass]
public unsafe class GlLayerStateCoverageTestRaster
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());

    [TestMethod]
    public void StencilAndRasterState_SetThenReset()
    {
        gl.StencilFuncSeparate(GlTriangleFace.Front, GlStencilFunction.Always, 1, 0xFF);
        gl.ResetStencilFuncSeparate(GlTriangleFace.Front);
        gl.StencilFunc(GlStencilFunction.Always, 1, 0xFF);
        gl.ResetStencilFunc();
        gl.StencilMask(0xFF);
        gl.ResetStencilMask();
        gl.StencilMaskSeparate(GlTriangleFace.Back, 0x0F);
        gl.ResetStencilMaskSeparate(GlTriangleFace.Back);
        gl.StencilOp(GlStencilOp.Keep, GlStencilOp.Replace, GlStencilOp.Incr);
        gl.ResetStencilOp();
        gl.StencilOpSeparate(GlTriangleFace.Front, GlStencilOp.Keep, GlStencilOp.Replace, GlStencilOp.Incr);
        gl.ResetStencilOpSeparate(GlTriangleFace.Front);

        gl.CullFace(GlTriangleFace.Front);
        gl.ResetCullFace();
        gl.FrontFace(GlFrontFaceDirection.Cw);
        gl.ResetFrontFace();
        gl.PolygonMode(GlTriangleFace.FrontAndBack, GlPolygonMode.Line);
        gl.ResetPolygonMode();
        gl.PolygonOffset(1f, 2f);
        gl.ResetPolygonOffset();
        gl.PolygonOffsetClamp(1f, 2f, 3f);
        gl.ResetPolygonOffsetClamp();
        gl.LineWidth(2f);
        gl.ResetLineWidth();
        gl.PointSize(3f);
        gl.ResetPointSize();
        gl.ProvokingVertex(GlVertexProvokingMode.FirstVertexConvention);
        gl.ResetProvokingVertex();
        gl.PrimitiveRestartIndex(99);
        gl.ResetPrimitiveRestartIndex();
        gl.LogicOp(GlLogicOp.Xor);
        gl.ResetLogicOp();
    }

    [TestMethod]
    public void ViewportAndScissorState_SetThenReset()
    {
        gl.ViewportIndexedf(0, 0, 0, 10, 10);
        gl.ResetViewportIndexed(0);
        float* viewports = stackalloc float[] { 0, 0, 10, 10, 1, 1, 8, 8 };
        gl.ViewportArrayv(0, 2, (nint)viewports);
        gl.ResetViewportArray(0, 2);

        gl.Scissor(0, 0, 10, 10);
        gl.ResetScissor();
        gl.ScissorIndexed(0, 0, 0, 10, 10);
        gl.ResetScissorIndexed(0);
        int* scissors = stackalloc int[] { 0, 0, 10, 10, 1, 1, 8, 8 };
        gl.ScissorArrayv(0, 2, (nint)scissors);
        gl.ResetScissorArray(0, 2);
    }

    [TestMethod]
    public void ParameterState_SetThenReset()
    {
        gl.ClipControl(GlClipControlOrigin.UpperLeft, GlClipControlDepth.ZeroToOne);
        gl.ResetClipControl();
        gl.MinSampleShading(0.5f);
        gl.ResetMinSampleShading();
        gl.SampleCoverage(0.5f, true);
        gl.ResetSampleCoverage();
        gl.PatchParameteri(GlPatchParameterName.PatchVertices, 4);
        gl.ResetPatchParameter(GlPatchParameterName.PatchVertices);
        gl.PointParameterf(GlPointParameterName.PointFadeThresholdSize, 2f);
        gl.ResetPointParameter(GlPointParameterName.PointFadeThresholdSize);
        gl.PointParameteri(GlPointParameterName.PointFadeThresholdSize, 3);
        gl.ResetPointParameter(GlPointParameterName.PointFadeThresholdSize);
        gl.PixelStoref(GlPixelStoreParameter.PackAlignment, 1f);
        gl.ResetPixelStore(GlPixelStoreParameter.PackAlignment);
        gl.ReadBuffer(GlReadBufferMode.ColorAttachment0);
        gl.ResetReadBuffer();
        gl.BeginTransformFeedback(GlPrimitiveType.Triangles);
        gl.EndTransformFeedback();
    }
}
