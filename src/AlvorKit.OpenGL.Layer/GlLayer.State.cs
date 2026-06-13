using GlLogicOp = AlvorKit.OpenGL.LogicOp;
using GlPolygonMode = AlvorKit.OpenGL.PolygonMode;
using GlStencilOp = AlvorKit.OpenGL.StencilOp;

namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    private GlStateSlot<(int, int, int, int)> viewport;
    private GlStateSlot<(int, int, int, int)> scissor;
    private GlStateSlot<(float, float, float, float)> clearColor;
    private GlStateSlot<(float, float, float, float)> blendColor;
    private GlStateSlot<double> clearDepth;
    private GlStateSlot<int> clearStencil;
    private GlStateSlot<DepthFunction> depthFunc;
    private GlStateSlot<bool> depthMask;
    private GlStateSlot<TriangleFace> cullFace;
    private GlStateSlot<FrontFaceDirection> frontFace;
    private GlStateSlot<(TriangleFace, GlPolygonMode)> polygonMode;
    private GlStateSlot<(float, float)> polygonOffset;
    private GlStateSlot<(float, float, float)> polygonOffsetClamp;
    private GlStateSlot<float> lineWidth;
    private GlStateSlot<float> pointSize;
    private GlStateSlot<VertexProvokingMode> provokingVertex;
    private GlStateSlot<uint> primitiveRestartIndex;
    private GlStateSlot<GlLogicOp> logicOp;
    private GlStateSlot<(ClipControlOrigin, ClipControlDepth)> clipControl;
    private GlStateSlot<float> minSampleShading;
    private GlStateSlot<(float, bool)> sampleCoverage;
    private GlStateSlot<ReadBufferMode> readBuffer;
    private GlStateSlot<ClampColorMode> clampColor;
    private GlStateSlot<(BlendingFactor, BlendingFactor)> blendFunc;
    private GlStateSlot<(BlendingFactor, BlendingFactor, BlendingFactor, BlendingFactor)> blendFuncSeparate;
    private GlStateSlot<BlendEquationModeEXT> blendEquation;
    private GlStateSlot<(BlendEquationModeEXT, BlendEquationModeEXT)> blendEquationSeparate;
    private GlStateSlot<(bool, bool, bool, bool)> colorMask;
    private GlStateSlot<(double, double)> depthRange;
    private GlStateSlot<(StencilFunction, int, uint)> stencilFunc;
    private GlStateSlot<uint> stencilMask;
    private GlStateSlot<(GlStencilOp, GlStencilOp, GlStencilOp)> stencilOp;
    private GlStateSlot<PrimitiveType> transformFeedbackActive;

    private readonly GlStateMap<EnableCap, bool> enableMap = new();
    private readonly GlStateMap<(EnableCap, uint), bool> indexedEnableMap = new();
    private readonly GlStateMap<uint, (BlendingFactor, BlendingFactor)> blendFuncMap = new();
    private readonly GlStateMap<uint, (BlendingFactor, BlendingFactor, BlendingFactor, BlendingFactor)> blendFuncSeparateMap = new();
    private readonly GlStateMap<uint, (BlendEquationModeEXT, BlendEquationModeEXT)> blendEquationSeparateMap = new();
    private readonly GlStateMap<uint, (bool, bool, bool, bool)> colorMaskMap = new();
    private readonly GlStateMap<uint, (double, double)> depthRangeMap = new();
    private readonly GlStateMap<TriangleFace, (StencilFunction, int, uint)> stencilFuncSeparateMap = new();
    private readonly GlStateMap<TriangleFace, uint> stencilMaskSeparateMap = new();
    private readonly GlStateMap<TriangleFace, (GlStencilOp, GlStencilOp, GlStencilOp)> stencilOpSeparateMap = new();
    private readonly GlStateMap<uint, (int, int, int, int)> scissorMap = new();
    private readonly GlStateMap<uint, (float, float, float, float)> viewportMap = new();
    private readonly GlStateMap<HintTarget, HintMode> hintMap = new();
    private readonly GlStateMap<uint, uint> sampleMaskMap = new();
    private readonly GlStateMap<PatchParameterName, int> patchParameterMap = new();
    private readonly GlStateMap<PointParameterName, float> pointParameterMap = new();
    private readonly GlStateMap<PixelStoreParameter, double> pixelStoreMap = new();

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="Disable(EnableCap)"/> for the same capability.</remarks>
    public override void Enable(EnableCap cap) { enableMap.Set(nameof(Enable), cap, true); base.Enable(cap); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <see cref="Enable(EnableCap)"/> for the same capability.</remarks>
    public override void Disable(EnableCap cap) { enableMap.Reset(nameof(Disable), cap); base.Disable(cap); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="Disablei(EnableCap, uint)"/> for the same capability and index.</remarks>
    public override void Enablei(EnableCap target, uint index) { indexedEnableMap.Set(nameof(Enablei), (target, index), true); base.Enablei(target, index); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <see cref="Enablei(EnableCap, uint)"/> for the same capability and index.</remarks>
    public override void Disablei(EnableCap target, uint index) { indexedEnableMap.Reset(nameof(Disablei), (target, index)); base.Disablei(target, index); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendFunc()"/>. Cannot be combined with <c>glBlendFunci</c>, <c>glBlendFuncSeparate</c>, or <c>glBlendFuncSeparatei</c>.</remarks>
    public override void BlendFunc(BlendingFactor sfactor, BlendingFactor dfactor)
    {
        if (blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFuncSeparate));
        if (blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFunci));
        if (blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFunc), nameof(BlendFuncSeparatei));
        blendFunc.Set(nameof(BlendFunc), (sfactor, dfactor));
        base.BlendFunc(sfactor, dfactor);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendFunc(uint)"/> for the same buffer. Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFuncSeparate</c>, or <c>glBlendFuncSeparatei</c>.</remarks>
    public override void BlendFunci(uint buf, BlendingFactor src, BlendingFactor dst)
    {
        if (blendFunc.IsSet) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFunc));
        if (blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFuncSeparate));
        if (blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFunci), nameof(BlendFuncSeparatei));
        blendFuncMap.Set(nameof(BlendFunci), buf, (src, dst));
        base.BlendFunci(buf, src, dst);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendFuncSeparate()"/>. Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFunci</c>, or <c>glBlendFuncSeparatei</c>.</remarks>
    public override void BlendFuncSeparate(BlendingFactor sfactorRGB, BlendingFactor dfactorRGB, BlendingFactor sfactorAlpha, BlendingFactor dfactorAlpha)
    {
        if (blendFunc.IsSet) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFunc));
        if (blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFunci));
        if (blendFuncSeparateMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparate), nameof(BlendFuncSeparatei));
        blendFuncSeparate.Set(nameof(BlendFuncSeparate), (sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha));
        base.BlendFuncSeparate(sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendFuncSeparate(uint)"/> for the same buffer. Cannot be combined with <c>glBlendFunc</c>, <c>glBlendFunci</c>, or <c>glBlendFuncSeparate</c>.</remarks>
    public override void BlendFuncSeparatei(uint buf, BlendingFactor srcRGB, BlendingFactor dstRGB, BlendingFactor srcAlpha, BlendingFactor dstAlpha)
    {
        if (blendFunc.IsSet) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFunc));
        if (blendFuncSeparate.IsSet) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFuncSeparate));
        if (blendFuncMap.HasAny) throw new GlConflictException(nameof(BlendFuncSeparatei), nameof(BlendFunci));
        blendFuncSeparateMap.Set(nameof(BlendFuncSeparatei), buf, (srcRGB, dstRGB, srcAlpha, dstAlpha));
        base.BlendFuncSeparatei(buf, srcRGB, dstRGB, srcAlpha, dstAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquation()"/>. Cannot be combined with <c>glBlendEquationSeparate</c> or <c>glBlendEquationSeparatei</c>.</remarks>
    public override void BlendEquation(BlendEquationModeEXT mode)
    {
        if (blendEquationSeparate.IsSet) throw new GlConflictException(nameof(BlendEquation), nameof(BlendEquationSeparate));
        if (blendEquationSeparateMap.HasAny) throw new GlConflictException(nameof(BlendEquation), nameof(BlendEquationSeparatei));
        blendEquation.Set(nameof(BlendEquation), mode);
        base.BlendEquation(mode);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquationSeparate()"/>. Cannot be combined with <c>glBlendEquation</c> or <c>glBlendEquationSeparatei</c>.</remarks>
    public override void BlendEquationSeparate(BlendEquationModeEXT modeRGB, BlendEquationModeEXT modeAlpha)
    {
        if (blendEquation.IsSet) throw new GlConflictException(nameof(BlendEquationSeparate), nameof(BlendEquation));
        if (blendEquationSeparateMap.HasAny) throw new GlConflictException(nameof(BlendEquationSeparate), nameof(BlendEquationSeparatei));
        blendEquationSeparate.Set(nameof(BlendEquationSeparate), (modeRGB, modeAlpha));
        base.BlendEquationSeparate(modeRGB, modeAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendEquationSeparate(uint)"/> for the same buffer. Cannot be combined with <c>glBlendEquation</c> or <c>glBlendEquationSeparate</c>.</remarks>
    public override void BlendEquationSeparatei(uint buf, BlendEquationModeEXT modeRGB, BlendEquationModeEXT modeAlpha)
    {
        if (blendEquation.IsSet) throw new GlConflictException(nameof(BlendEquationSeparatei), nameof(BlendEquation));
        if (blendEquationSeparate.IsSet) throw new GlConflictException(nameof(BlendEquationSeparatei), nameof(BlendEquationSeparate));
        blendEquationSeparateMap.Set(nameof(BlendEquationSeparatei), buf, (modeRGB, modeAlpha));
        base.BlendEquationSeparatei(buf, modeRGB, modeAlpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetBlendColor()"/>.</remarks>
    public override void BlendColor(float red, float green, float blue, float alpha) { blendColor.Set(nameof(BlendColor), (red, green, blue, alpha)); base.BlendColor(red, green, blue, alpha); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearColor()"/>.</remarks>
    public override void ClearColor(float red, float green, float blue, float alpha) { clearColor.Set(nameof(ClearColor), (red, green, blue, alpha)); base.ClearColor(red, green, blue, alpha); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClampColor()"/>.</remarks>
    public override void ClampColor(ClampColorTarget target, ClampColorMode clamp) { clampColor.Set(nameof(ClampColor), clamp); base.ClampColor(target, clamp); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetColorMask()"/>. Cannot be combined with <c>glColorMaski</c>.</remarks>
    public override void ColorMask(bool red, bool green, bool blue, bool alpha)
    {
        if (colorMaskMap.HasAny) throw new GlConflictException(nameof(ColorMask), nameof(ColorMaski));
        colorMask.Set(nameof(ColorMask), (red, green, blue, alpha));
        base.ColorMask(red, green, blue, alpha);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetColorMask(uint)"/> for the same buffer. Cannot be combined with <c>glColorMask</c>.</remarks>
    public override void ColorMaski(uint index, bool r, bool g, bool b, bool a)
    {
        if (colorMask.IsSet) throw new GlConflictException(nameof(ColorMaski), nameof(ColorMask));
        colorMaskMap.Set(nameof(ColorMaski), index, (r, g, b, a));
        base.ColorMaski(index, r, g, b, a);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthFunc()"/>.</remarks>
    public override void DepthFunc(DepthFunction func) { depthFunc.Set(nameof(DepthFunc), func); base.DepthFunc(func); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthMask()"/>.</remarks>
    public override void DepthMask(bool flag) { depthMask.Set(nameof(DepthMask), flag); base.DepthMask(flag); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearDepth()"/>.</remarks>
    public override void ClearDepth(double depth) { clearDepth.Set(nameof(ClearDepth), depth); base.ClearDepth(depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearDepth()"/>.</remarks>
    public override void ClearDepthf(float depth) { clearDepth.Set(nameof(ClearDepthf), depth); base.ClearDepthf(depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClearStencil()"/>.</remarks>
    public override void ClearStencil(int s) { clearStencil.Set(nameof(ClearStencil), s); base.ClearStencil(s); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthRange()"/>. Cannot be combined with <c>glDepthRangeIndexed</c> or <c>glDepthRangeArrayv</c>.</remarks>
    public override void DepthRange(double n, double f)
    {
        if (depthRangeMap.HasAny) throw new GlConflictException(nameof(DepthRange), nameof(DepthRangeIndexed));
        depthRange.Set(nameof(DepthRange), (n, f));
        base.DepthRange(n, f);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthRange()"/>. Cannot be combined with <c>glDepthRangeIndexed</c> or <c>glDepthRangeArrayv</c>.</remarks>
    public override void DepthRangef(float n, float f)
    {
        if (depthRangeMap.HasAny) throw new GlConflictException(nameof(DepthRangef), nameof(DepthRangeIndexed));
        depthRange.Set(nameof(DepthRangef), (n, f));
        base.DepthRangef(n, f);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeIndexed(uint)"/> for the same index. Cannot be combined with <c>glDepthRange</c>.</remarks>
    public override void DepthRangeIndexed(uint index, double n, double f)
    {
        if (depthRange.IsSet) throw new GlConflictException(nameof(DepthRangeIndexed), nameof(DepthRange));
        depthRangeMap.Set(nameof(DepthRangeIndexed), index, (n, f));
        base.DepthRangeIndexed(index, n, f);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDepthRangeArray(uint, int)"/> for the same range. Cannot be combined with <c>glDepthRange</c>.</remarks>
    public override unsafe void DepthRangeArrayv(uint first, int count, nint v)
    {
        if (depthRange.IsSet) throw new GlConflictException(nameof(DepthRangeArrayv), nameof(DepthRange));
        var values = (double*)v;
        for (var i = 0; i < count; i++)
            depthRangeMap.Set(nameof(DepthRangeArrayv), first + (uint)i, (values[i * 2], values[i * 2 + 1]));
        base.DepthRangeArrayv(first, count, v);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilFunc()"/>. Cannot be combined with <c>glStencilFuncSeparate</c>.</remarks>
    public override void StencilFunc(StencilFunction func, int @ref, uint mask)
    {
        if (stencilFuncSeparateMap.HasAny) throw new GlConflictException(nameof(StencilFunc), nameof(StencilFuncSeparate));
        stencilFunc.Set(nameof(StencilFunc), (func, @ref, mask));
        base.StencilFunc(func, @ref, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilFuncSeparate(TriangleFace)"/> for the same face. Cannot be combined with <c>glStencilFunc</c>.</remarks>
    public override void StencilFuncSeparate(TriangleFace face, StencilFunction func, int @ref, uint mask)
    {
        if (stencilFunc.IsSet) throw new GlConflictException(nameof(StencilFuncSeparate), nameof(StencilFunc));
        stencilFuncSeparateMap.Set(nameof(StencilFuncSeparate), face, (func, @ref, mask));
        base.StencilFuncSeparate(face, func, @ref, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilMask()"/>. Cannot be combined with <c>glStencilMaskSeparate</c>.</remarks>
    public override void StencilMask(uint mask)
    {
        if (stencilMaskSeparateMap.HasAny) throw new GlConflictException(nameof(StencilMask), nameof(StencilMaskSeparate));
        stencilMask.Set(nameof(StencilMask), mask);
        base.StencilMask(mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilMaskSeparate(TriangleFace)"/> for the same face. Cannot be combined with <c>glStencilMask</c>.</remarks>
    public override void StencilMaskSeparate(TriangleFace face, uint mask)
    {
        if (stencilMask.IsSet) throw new GlConflictException(nameof(StencilMaskSeparate), nameof(StencilMask));
        stencilMaskSeparateMap.Set(nameof(StencilMaskSeparate), face, mask);
        base.StencilMaskSeparate(face, mask);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilOp()"/>. Cannot be combined with <c>glStencilOpSeparate</c>.</remarks>
    public override void StencilOp(GlStencilOp fail, GlStencilOp zfail, GlStencilOp zpass)
    {
        if (stencilOpSeparateMap.HasAny) throw new GlConflictException(nameof(StencilOp), nameof(StencilOpSeparate));
        stencilOp.Set(nameof(StencilOp), (fail, zfail, zpass));
        base.StencilOp(fail, zfail, zpass);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetStencilOpSeparate(TriangleFace)"/> for the same face. Cannot be combined with <c>glStencilOp</c>.</remarks>
    public override void StencilOpSeparate(TriangleFace face, GlStencilOp sfail, GlStencilOp dpfail, GlStencilOp dppass)
    {
        if (stencilOp.IsSet) throw new GlConflictException(nameof(StencilOpSeparate), nameof(StencilOp));
        stencilOpSeparateMap.Set(nameof(StencilOpSeparate), face, (sfail, dpfail, dppass));
        base.StencilOpSeparate(face, sfail, dpfail, dppass);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetCullFace()"/>.</remarks>
    public override void CullFace(TriangleFace mode) { cullFace.Set(nameof(CullFace), mode); base.CullFace(mode); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetFrontFace()"/>.</remarks>
    public override void FrontFace(FrontFaceDirection mode) { frontFace.Set(nameof(FrontFace), mode); base.FrontFace(mode); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPolygonMode()"/>.</remarks>
    public override void PolygonMode(TriangleFace face, GlPolygonMode mode) { polygonMode.Set(nameof(PolygonMode), (face, mode)); base.PolygonMode(face, mode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPolygonOffset()"/>. Cannot be combined with <c>glPolygonOffsetClamp</c>.</remarks>
    public override void PolygonOffset(float factor, float units)
    {
        if (polygonOffsetClamp.IsSet) throw new GlConflictException(nameof(PolygonOffset), nameof(PolygonOffsetClamp));
        polygonOffset.Set(nameof(PolygonOffset), (factor, units));
        base.PolygonOffset(factor, units);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPolygonOffsetClamp()"/>. Cannot be combined with <c>glPolygonOffset</c>.</remarks>
    public override void PolygonOffsetClamp(float factor, float units, float clamp)
    {
        if (polygonOffset.IsSet) throw new GlConflictException(nameof(PolygonOffsetClamp), nameof(PolygonOffset));
        polygonOffsetClamp.Set(nameof(PolygonOffsetClamp), (factor, units, clamp));
        base.PolygonOffsetClamp(factor, units, clamp);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetLineWidth()"/>.</remarks>
    public override void LineWidth(float width) { lineWidth.Set(nameof(LineWidth), width); base.LineWidth(width); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPointSize()"/>.</remarks>
    public override void PointSize(float size) { pointSize.Set(nameof(PointSize), size); base.PointSize(size); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetProvokingVertex()"/>.</remarks>
    public override void ProvokingVertex(VertexProvokingMode mode) { provokingVertex.Set(nameof(ProvokingVertex), mode); base.ProvokingVertex(mode); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPrimitiveRestartIndex()"/>.</remarks>
    public override void PrimitiveRestartIndex(uint index) { primitiveRestartIndex.Set(nameof(PrimitiveRestartIndex), index); base.PrimitiveRestartIndex(index); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetLogicOp()"/>.</remarks>
    public override void LogicOp(GlLogicOp opcode) { logicOp.Set(nameof(LogicOp), opcode); base.LogicOp(opcode); }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetViewport()"/>. Cannot be combined with <c>glViewportIndexedf</c> or <c>glViewportArrayv</c>.</remarks>
    public override void Viewport(int x, int y, int width, int height)
    {
        if (viewportMap.HasAny) throw new GlConflictException(nameof(Viewport), nameof(ViewportIndexedf));
        viewport.Set(nameof(Viewport), (x, y, width, height));
        base.Viewport(x, y, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetViewportIndexed(uint)"/> for the same index. Cannot be combined with <c>glViewport</c>.</remarks>
    public override void ViewportIndexedf(uint index, float x, float y, float w, float h)
    {
        if (viewport.IsSet) throw new GlConflictException(nameof(ViewportIndexedf), nameof(Viewport));
        viewportMap.Set(nameof(ViewportIndexedf), index, (x, y, w, h));
        base.ViewportIndexedf(index, x, y, w, h);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetViewportArray(uint, int)"/> for the same range. Cannot be combined with <c>glViewport</c>.</remarks>
    public override unsafe void ViewportArrayv(uint first, int count, nint v)
    {
        if (viewport.IsSet) throw new GlConflictException(nameof(ViewportArrayv), nameof(Viewport));
        var values = (float*)v;
        for (var i = 0; i < count; i++)
            viewportMap.Set(nameof(ViewportArrayv), first + (uint)i, (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
        base.ViewportArrayv(first, count, v);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetScissor()"/>. Cannot be combined with <c>glScissorIndexed</c> or <c>glScissorArrayv</c>.</remarks>
    public override void Scissor(int x, int y, int width, int height)
    {
        if (scissorMap.HasAny) throw new GlConflictException(nameof(Scissor), nameof(ScissorIndexed));
        scissor.Set(nameof(Scissor), (x, y, width, height));
        base.Scissor(x, y, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetScissorIndexed(uint)"/> for the same index. Cannot be combined with <c>glScissor</c>.</remarks>
    public override void ScissorIndexed(uint index, int left, int bottom, int width, int height)
    {
        if (scissor.IsSet) throw new GlConflictException(nameof(ScissorIndexed), nameof(Scissor));
        scissorMap.Set(nameof(ScissorIndexed), index, (left, bottom, width, height));
        base.ScissorIndexed(index, left, bottom, width, height);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetScissorArray(uint, int)"/> for the same range. Cannot be combined with <c>glScissor</c>.</remarks>
    public override unsafe void ScissorArrayv(uint first, int count, nint v)
    {
        if (scissor.IsSet) throw new GlConflictException(nameof(ScissorArrayv), nameof(Scissor));
        var values = (int*)v;
        for (var i = 0; i < count; i++)
            scissorMap.Set(nameof(ScissorArrayv), first + (uint)i, (values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]));
        base.ScissorArrayv(first, count, v);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetClipControl()"/>.</remarks>
    public override void ClipControl(ClipControlOrigin origin, ClipControlDepth depth) { clipControl.Set(nameof(ClipControl), (origin, depth)); base.ClipControl(origin, depth); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetHint(HintTarget)"/> for the same target.</remarks>
    public override void Hint(HintTarget target, HintMode mode) { hintMap.Set(nameof(Hint), target, mode); base.Hint(target, mode); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetMinSampleShading()"/>.</remarks>
    public override void MinSampleShading(float value) { minSampleShading.Set(nameof(MinSampleShading), value); base.MinSampleShading(value); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetSampleCoverage()"/>.</remarks>
    public override void SampleCoverage(float value, bool invert) { sampleCoverage.Set(nameof(SampleCoverage), (value, invert)); base.SampleCoverage(value, invert); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetSampleMask(uint)"/> for the same word.</remarks>
    public override void SampleMaski(uint maskNumber, uint mask) { sampleMaskMap.Set(nameof(SampleMaski), maskNumber, mask); base.SampleMaski(maskNumber, mask); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPatchParameter(PatchParameterName)"/> for the same parameter.</remarks>
    public override void PatchParameteri(PatchParameterName pname, int value) { patchParameterMap.Set(nameof(PatchParameteri), pname, value); base.PatchParameteri(pname, value); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPointParameter(PointParameterName)"/> for the same parameter.</remarks>
    public override void PointParameterf(PointParameterName pname, float param) { pointParameterMap.Set(nameof(PointParameterf), pname, param); base.PointParameterf(pname, param); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPointParameter(PointParameterName)"/> for the same parameter.</remarks>
    public override void PointParameteri(PointParameterName pname, int param) { pointParameterMap.Set(nameof(PointParameteri), pname, param); base.PointParameteri(pname, param); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPixelStore(PixelStoreParameter)"/> for the same parameter.</remarks>
    public override void PixelStorei(PixelStoreParameter pname, int param) { pixelStoreMap.Set(nameof(PixelStorei), pname, param); base.PixelStorei(pname, param); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetPixelStore(PixelStoreParameter)"/> for the same parameter.</remarks>
    public override void PixelStoref(PixelStoreParameter pname, float param) { pixelStoreMap.Set(nameof(PixelStoref), pname, param); base.PixelStoref(pname, param); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetReadBuffer()"/>.</remarks>
    public override void ReadBuffer(ReadBufferMode src) { readBuffer.Set(nameof(ReadBuffer), src); base.ReadBuffer(src); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndTransformFeedback</c>.</remarks>
    public override void BeginTransformFeedback(PrimitiveType primitiveMode) { transformFeedbackActive.Set(nameof(BeginTransformFeedback), primitiveMode); base.BeginTransformFeedback(primitiveMode); }
    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginTransformFeedback</c>.</remarks>
    public override void EndTransformFeedback() { transformFeedbackActive.Reset(nameof(EndTransformFeedback)); base.EndTransformFeedback(); }

    /// <summary>Layer: The default value <see cref="ResetBlendFunc()"/> restores.</summary>
    public virtual (BlendingFactor Source, BlendingFactor Destination) DefaultBlendFunc => (BlendingFactor.One, BlendingFactor.Zero);
    /// <summary>Layer: The default value <see cref="ResetBlendFuncSeparate()"/> restores.</summary>
    public virtual (BlendingFactor SourceRgb, BlendingFactor DestinationRgb, BlendingFactor SourceAlpha, BlendingFactor DestinationAlpha) DefaultBlendFuncSeparate => (BlendingFactor.One, BlendingFactor.Zero, BlendingFactor.One, BlendingFactor.Zero);
    /// <summary>Layer: The default value <see cref="ResetBlendEquation()"/> restores.</summary>
    public virtual BlendEquationModeEXT DefaultBlendEquation => BlendEquationModeEXT.FuncAdd;
    /// <summary>Layer: The default value <see cref="ResetBlendEquationSeparate()"/> restores.</summary>
    public virtual (BlendEquationModeEXT Rgb, BlendEquationModeEXT Alpha) DefaultBlendEquationSeparate => (BlendEquationModeEXT.FuncAdd, BlendEquationModeEXT.FuncAdd);
    /// <summary>Layer: The default value <see cref="ResetBlendColor()"/> restores.</summary>
    public virtual (float Red, float Green, float Blue, float Alpha) DefaultBlendColor => (0f, 0f, 0f, 0f);
    /// <summary>Layer: The default value <see cref="ResetClearColor()"/> restores.</summary>
    public virtual (float Red, float Green, float Blue, float Alpha) DefaultClearColor => (0f, 0f, 0f, 0f);
    /// <summary>Layer: The default value <see cref="ResetClampColor()"/> restores.</summary>
    public virtual (ClampColorTarget Target, ClampColorMode Clamp) DefaultClampColor => (ClampColorTarget.ClampReadColor, ClampColorMode.FixedOnly);
    /// <summary>Layer: The default value <see cref="ResetColorMask()"/> restores.</summary>
    public virtual (bool Red, bool Green, bool Blue, bool Alpha) DefaultColorMask => (true, true, true, true);
    /// <summary>Layer: The default value <see cref="ResetDepthFunc()"/> restores.</summary>
    public virtual DepthFunction DefaultDepthFunc => DepthFunction.Less;
    /// <summary>Layer: The default value <see cref="ResetDepthMask()"/> restores.</summary>
    public virtual bool DefaultDepthMask => true;
    /// <summary>Layer: The default value <see cref="ResetClearDepth()"/> restores.</summary>
    public virtual double DefaultClearDepth => 1.0;
    /// <summary>Layer: The default value <see cref="ResetDepthRange()"/> restores.</summary>
    public virtual (double Near, double Far) DefaultDepthRange => (0.0, 1.0);
    /// <summary>Layer: The default value <see cref="ResetClearStencil()"/> restores.</summary>
    public virtual int DefaultClearStencil => 0;
    /// <summary>Layer: The default value <see cref="ResetStencilFunc()"/> restores.</summary>
    public virtual (StencilFunction Func, int Ref, uint Mask) DefaultStencilFunc => (StencilFunction.Always, 0, uint.MaxValue);
    /// <summary>Layer: The default value <see cref="ResetStencilMask()"/> restores.</summary>
    public virtual uint DefaultStencilMask => uint.MaxValue;
    /// <summary>Layer: The default value <see cref="ResetStencilOp()"/> restores.</summary>
    public virtual (GlStencilOp Fail, GlStencilOp ZFail, GlStencilOp ZPass) DefaultStencilOp => (GlStencilOp.Keep, GlStencilOp.Keep, GlStencilOp.Keep);
    /// <summary>Layer: The default value <see cref="ResetCullFace()"/> restores.</summary>
    public virtual TriangleFace DefaultCullFace => TriangleFace.Back;
    /// <summary>Layer: The default value <see cref="ResetFrontFace()"/> restores.</summary>
    public virtual FrontFaceDirection DefaultFrontFace => FrontFaceDirection.Ccw;
    /// <summary>Layer: The default value <see cref="ResetPolygonMode()"/> restores.</summary>
    public virtual (TriangleFace Face, GlPolygonMode Mode) DefaultPolygonMode => (TriangleFace.FrontAndBack, GlPolygonMode.Fill);
    /// <summary>Layer: The default value <see cref="ResetPolygonOffset()"/> restores.</summary>
    public virtual (float Factor, float Units) DefaultPolygonOffset => (0f, 0f);
    /// <summary>Layer: The default value <see cref="ResetPolygonOffsetClamp()"/> restores.</summary>
    public virtual (float Factor, float Units, float Clamp) DefaultPolygonOffsetClamp => (0f, 0f, 0f);
    /// <summary>Layer: The default value <see cref="ResetLineWidth()"/> restores.</summary>
    public virtual float DefaultLineWidth => 1f;
    /// <summary>Layer: The default value <see cref="ResetPointSize()"/> restores.</summary>
    public virtual float DefaultPointSize => 1f;
    /// <summary>Layer: The default value <see cref="ResetProvokingVertex()"/> restores.</summary>
    public virtual VertexProvokingMode DefaultProvokingVertex => VertexProvokingMode.LastVertexConvention;
    /// <summary>Layer: The default value <see cref="ResetPrimitiveRestartIndex()"/> restores.</summary>
    public virtual uint DefaultPrimitiveRestartIndex => 0u;
    /// <summary>Layer: The default value <see cref="ResetLogicOp()"/> restores.</summary>
    public virtual GlLogicOp DefaultLogicOp => GlLogicOp.Copy;
    /// <summary>Layer: The default value <see cref="ResetViewport()"/> restores.</summary>
    public virtual (int X, int Y, int Width, int Height) DefaultViewport => (0, 0, 0, 0);
    /// <summary>Layer: The default value <see cref="ResetScissor()"/> restores.</summary>
    public virtual (int X, int Y, int Width, int Height) DefaultScissor => (0, 0, 0, 0);
    /// <summary>Layer: The default value <see cref="ResetActiveTexture()"/> restores.</summary>
    public virtual TextureUnit DefaultActiveTexture => TextureUnit.Texture0;
    /// <summary>Layer: The default value <see cref="ResetClipControl()"/> restores.</summary>
    public virtual (ClipControlOrigin Origin, ClipControlDepth Depth) DefaultClipControl => (ClipControlOrigin.LowerLeft, ClipControlDepth.NegativeOneToOne);
    /// <summary>Layer: The default value <see cref="ResetHint(HintTarget)"/> restores.</summary>
    public virtual HintMode DefaultHint => HintMode.DontCare;
    /// <summary>Layer: The default value <see cref="ResetMinSampleShading()"/> restores.</summary>
    public virtual float DefaultMinSampleShading => 0f;
    /// <summary>Layer: The default value <see cref="ResetSampleCoverage()"/> restores.</summary>
    public virtual (float Value, bool Invert) DefaultSampleCoverage => (1f, false);
    /// <summary>Layer: The default value <see cref="ResetSampleMask(uint)"/> restores.</summary>
    public virtual uint DefaultSampleMask => uint.MaxValue;
    /// <summary>Layer: The default value <see cref="ResetReadBuffer()"/> restores.</summary>
    public virtual ReadBufferMode DefaultReadBuffer => ReadBufferMode.ColorAttachment0;

    /// <summary>Layer: The default value <see cref="ResetPixelStore(PixelStoreParameter)"/> restores for <paramref name="pname"/>.</summary>
    public virtual int DefaultPixelStore(PixelStoreParameter pname) => pname is PixelStoreParameter.PackAlignment or PixelStoreParameter.UnpackAlignment ? 4 : 0;
    /// <summary>Layer: The default value <see cref="ResetPatchParameter(PatchParameterName)"/> restores for <paramref name="pname"/>.</summary>
    public virtual int DefaultPatchParameter(PatchParameterName pname) => pname == PatchParameterName.PatchVertices ? 3 : 0;
    /// <summary>Layer: The default value <see cref="ResetPointParameter(PointParameterName)"/> restores for <paramref name="pname"/>.</summary>
    public virtual float DefaultPointParameter(PointParameterName pname) => pname == PointParameterName.PointFadeThresholdSize ? 1f : 0f;

    /// <summary>Layer: Restores <c>glBlendFunc</c> to <see cref="DefaultBlendFunc"/>. Must be paired with exactly one earlier call to <c>glBlendFunc</c>.</summary>
    public void ResetBlendFunc() { blendFunc.Reset(nameof(BlendFunc)); base.BlendFunc(DefaultBlendFunc.Source, DefaultBlendFunc.Destination); }
    /// <summary>Layer: Restores <c>glBlendFunci</c> for buffer <paramref name="buf"/>. Must be paired with exactly one earlier call to <c>glBlendFunci</c> for the same buffer.</summary>
    public void ResetBlendFunc(uint buf) { blendFuncMap.Reset(nameof(BlendFunci), buf); base.BlendFunci(buf, DefaultBlendFunc.Source, DefaultBlendFunc.Destination); }
    /// <summary>Layer: Restores <c>glBlendFuncSeparate</c> to <see cref="DefaultBlendFuncSeparate"/>. Must be paired with exactly one earlier call to <c>glBlendFuncSeparate</c>.</summary>
    public void ResetBlendFuncSeparate() { blendFuncSeparate.Reset(nameof(BlendFuncSeparate)); base.BlendFuncSeparate(DefaultBlendFuncSeparate.SourceRgb, DefaultBlendFuncSeparate.DestinationRgb, DefaultBlendFuncSeparate.SourceAlpha, DefaultBlendFuncSeparate.DestinationAlpha); }
    /// <summary>Layer: Restores <c>glBlendFuncSeparatei</c> for buffer <paramref name="buf"/>. Must be paired with exactly one earlier call to <c>glBlendFuncSeparatei</c> for the same buffer.</summary>
    public void ResetBlendFuncSeparate(uint buf) { blendFuncSeparateMap.Reset(nameof(BlendFuncSeparatei), buf); base.BlendFuncSeparatei(buf, DefaultBlendFuncSeparate.SourceRgb, DefaultBlendFuncSeparate.DestinationRgb, DefaultBlendFuncSeparate.SourceAlpha, DefaultBlendFuncSeparate.DestinationAlpha); }
    /// <summary>Layer: Restores <c>glBlendEquation</c> to <see cref="DefaultBlendEquation"/>. Must be paired with exactly one earlier call to <c>glBlendEquation</c>.</summary>
    public void ResetBlendEquation() { blendEquation.Reset(nameof(BlendEquation)); base.BlendEquation(DefaultBlendEquation); }
    /// <summary>Layer: Restores <c>glBlendEquationSeparate</c> to <see cref="DefaultBlendEquationSeparate"/>. Must be paired with exactly one earlier call to <c>glBlendEquationSeparate</c>.</summary>
    public void ResetBlendEquationSeparate() { blendEquationSeparate.Reset(nameof(BlendEquationSeparate)); base.BlendEquationSeparate(DefaultBlendEquationSeparate.Rgb, DefaultBlendEquationSeparate.Alpha); }
    /// <summary>Layer: Restores <c>glBlendEquationSeparatei</c> for buffer <paramref name="buf"/>. Must be paired with exactly one earlier call to <c>glBlendEquationSeparatei</c> for the same buffer.</summary>
    public void ResetBlendEquationSeparate(uint buf) { blendEquationSeparateMap.Reset(nameof(BlendEquationSeparatei), buf); base.BlendEquationSeparatei(buf, DefaultBlendEquationSeparate.Rgb, DefaultBlendEquationSeparate.Alpha); }
    /// <summary>Layer: Restores <c>glBlendColor</c> to <see cref="DefaultBlendColor"/>. Must be paired with exactly one earlier call to <c>glBlendColor</c>.</summary>
    public void ResetBlendColor() { blendColor.Reset(nameof(BlendColor)); base.BlendColor(DefaultBlendColor.Red, DefaultBlendColor.Green, DefaultBlendColor.Blue, DefaultBlendColor.Alpha); }
    /// <summary>Layer: Restores <c>glClearColor</c> to <see cref="DefaultClearColor"/>. Must be paired with exactly one earlier call to <c>glClearColor</c>.</summary>
    public void ResetClearColor() { clearColor.Reset(nameof(ClearColor)); base.ClearColor(DefaultClearColor.Red, DefaultClearColor.Green, DefaultClearColor.Blue, DefaultClearColor.Alpha); }
    /// <summary>Layer: Restores <c>glClampColor</c> to <see cref="DefaultClampColor"/>. Must be paired with exactly one earlier call to <c>glClampColor</c>.</summary>
    public void ResetClampColor() { clampColor.Reset(nameof(ClampColor)); base.ClampColor(DefaultClampColor.Target, DefaultClampColor.Clamp); }
    /// <summary>Layer: Restores <c>glColorMask</c> to <see cref="DefaultColorMask"/>. Must be paired with exactly one earlier call to <c>glColorMask</c>.</summary>
    public void ResetColorMask() { colorMask.Reset(nameof(ColorMask)); base.ColorMask(DefaultColorMask.Red, DefaultColorMask.Green, DefaultColorMask.Blue, DefaultColorMask.Alpha); }
    /// <summary>Layer: Restores <c>glColorMaski</c> for buffer <paramref name="buf"/>. Must be paired with exactly one earlier call to <c>glColorMaski</c> for the same buffer.</summary>
    public void ResetColorMask(uint buf) { colorMaskMap.Reset(nameof(ColorMaski), buf); base.ColorMaski(buf, DefaultColorMask.Red, DefaultColorMask.Green, DefaultColorMask.Blue, DefaultColorMask.Alpha); }
    /// <summary>Layer: Restores <c>glDepthFunc</c> to <see cref="DefaultDepthFunc"/>. Must be paired with exactly one earlier call to <c>glDepthFunc</c>.</summary>
    public void ResetDepthFunc() { depthFunc.Reset(nameof(DepthFunc)); base.DepthFunc(DefaultDepthFunc); }
    /// <summary>Layer: Restores <c>glDepthMask</c> to <see cref="DefaultDepthMask"/>. Must be paired with exactly one earlier call to <c>glDepthMask</c>.</summary>
    public void ResetDepthMask() { depthMask.Reset(nameof(DepthMask)); base.DepthMask(DefaultDepthMask); }
    /// <summary>Layer: Restores <c>glClearDepth</c> to <see cref="DefaultClearDepth"/>. Must be paired with exactly one earlier call to <c>glClearDepth</c> or <c>glClearDepthf</c>.</summary>
    public void ResetClearDepth() { clearDepth.Reset(nameof(ClearDepth)); base.ClearDepth(DefaultClearDepth); }
    /// <summary>Layer: Restores <c>glDepthRange</c> to <see cref="DefaultDepthRange"/>. Must be paired with exactly one earlier call to <c>glDepthRange</c> or <c>glDepthRangef</c>.</summary>
    public void ResetDepthRange() { depthRange.Reset(nameof(DepthRange)); base.DepthRange(DefaultDepthRange.Near, DefaultDepthRange.Far); }
    /// <summary>Layer: Restores <c>glDepthRangeIndexed</c> for viewport <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glDepthRangeIndexed</c> for the same index.</summary>
    public void ResetDepthRangeIndexed(uint index) { depthRangeMap.Reset(nameof(DepthRangeIndexed), index); base.DepthRangeIndexed(index, DefaultDepthRange.Near, DefaultDepthRange.Far); }
    /// <summary>Layer: Restores <c>glDepthRangeArrayv</c> for <paramref name="count"/> viewports from <paramref name="first"/>. Must be paired with exactly one earlier call to <c>glDepthRangeArrayv</c> for the same range.</summary>
    public unsafe void ResetDepthRangeArray(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            depthRangeMap.Reset(nameof(DepthRangeArrayv), first + (uint)i);
        Span<double> values = stackalloc double[count * 2];
        for (var i = 0; i < count; i++) { values[i * 2] = DefaultDepthRange.Near; values[i * 2 + 1] = DefaultDepthRange.Far; }
        fixed (double* pointer = values)
            base.DepthRangeArrayv(first, count, (nint)pointer);
    }
    /// <summary>Layer: Restores <c>glClearStencil</c> to <see cref="DefaultClearStencil"/>. Must be paired with exactly one earlier call to <c>glClearStencil</c>.</summary>
    public void ResetClearStencil() { clearStencil.Reset(nameof(ClearStencil)); base.ClearStencil(DefaultClearStencil); }
    /// <summary>Layer: Restores <c>glStencilFunc</c> to <see cref="DefaultStencilFunc"/>. Must be paired with exactly one earlier call to <c>glStencilFunc</c>.</summary>
    public void ResetStencilFunc() { stencilFunc.Reset(nameof(StencilFunc)); base.StencilFunc(DefaultStencilFunc.Func, DefaultStencilFunc.Ref, DefaultStencilFunc.Mask); }
    /// <summary>Layer: Restores <c>glStencilFuncSeparate</c> for <paramref name="face"/>. Must be paired with exactly one earlier call to <c>glStencilFuncSeparate</c> for the same face.</summary>
    public void ResetStencilFuncSeparate(TriangleFace face) { stencilFuncSeparateMap.Reset(nameof(StencilFuncSeparate), face); base.StencilFuncSeparate(face, DefaultStencilFunc.Func, DefaultStencilFunc.Ref, DefaultStencilFunc.Mask); }
    /// <summary>Layer: Restores <c>glStencilMask</c> to <see cref="DefaultStencilMask"/>. Must be paired with exactly one earlier call to <c>glStencilMask</c>.</summary>
    public void ResetStencilMask() { stencilMask.Reset(nameof(StencilMask)); base.StencilMask(DefaultStencilMask); }
    /// <summary>Layer: Restores <c>glStencilMaskSeparate</c> for <paramref name="face"/>. Must be paired with exactly one earlier call to <c>glStencilMaskSeparate</c> for the same face.</summary>
    public void ResetStencilMaskSeparate(TriangleFace face) { stencilMaskSeparateMap.Reset(nameof(StencilMaskSeparate), face); base.StencilMaskSeparate(face, DefaultStencilMask); }
    /// <summary>Layer: Restores <c>glStencilOp</c> to <see cref="DefaultStencilOp"/>. Must be paired with exactly one earlier call to <c>glStencilOp</c>.</summary>
    public void ResetStencilOp() { stencilOp.Reset(nameof(StencilOp)); base.StencilOp(DefaultStencilOp.Fail, DefaultStencilOp.ZFail, DefaultStencilOp.ZPass); }
    /// <summary>Layer: Restores <c>glStencilOpSeparate</c> for <paramref name="face"/>. Must be paired with exactly one earlier call to <c>glStencilOpSeparate</c> for the same face.</summary>
    public void ResetStencilOpSeparate(TriangleFace face) { stencilOpSeparateMap.Reset(nameof(StencilOpSeparate), face); base.StencilOpSeparate(face, DefaultStencilOp.Fail, DefaultStencilOp.ZFail, DefaultStencilOp.ZPass); }
    /// <summary>Layer: Restores <c>glCullFace</c> to <see cref="DefaultCullFace"/>. Must be paired with exactly one earlier call to <c>glCullFace</c>.</summary>
    public void ResetCullFace() { cullFace.Reset(nameof(CullFace)); base.CullFace(DefaultCullFace); }
    /// <summary>Layer: Restores <c>glFrontFace</c> to <see cref="DefaultFrontFace"/>. Must be paired with exactly one earlier call to <c>glFrontFace</c>.</summary>
    public void ResetFrontFace() { frontFace.Reset(nameof(FrontFace)); base.FrontFace(DefaultFrontFace); }
    /// <summary>Layer: Restores <c>glPolygonMode</c> to <see cref="DefaultPolygonMode"/>. Must be paired with exactly one earlier call to <c>glPolygonMode</c>.</summary>
    public void ResetPolygonMode() { polygonMode.Reset(nameof(PolygonMode)); base.PolygonMode(DefaultPolygonMode.Face, DefaultPolygonMode.Mode); }
    /// <summary>Layer: Restores <c>glPolygonOffset</c> to <see cref="DefaultPolygonOffset"/>. Must be paired with exactly one earlier call to <c>glPolygonOffset</c>.</summary>
    public void ResetPolygonOffset() { polygonOffset.Reset(nameof(PolygonOffset)); base.PolygonOffset(DefaultPolygonOffset.Factor, DefaultPolygonOffset.Units); }
    /// <summary>Layer: Restores <c>glPolygonOffsetClamp</c> to <see cref="DefaultPolygonOffsetClamp"/>. Must be paired with exactly one earlier call to <c>glPolygonOffsetClamp</c>.</summary>
    public void ResetPolygonOffsetClamp() { polygonOffsetClamp.Reset(nameof(PolygonOffsetClamp)); base.PolygonOffsetClamp(DefaultPolygonOffsetClamp.Factor, DefaultPolygonOffsetClamp.Units, DefaultPolygonOffsetClamp.Clamp); }
    /// <summary>Layer: Restores <c>glLineWidth</c> to <see cref="DefaultLineWidth"/>. Must be paired with exactly one earlier call to <c>glLineWidth</c>.</summary>
    public void ResetLineWidth() { lineWidth.Reset(nameof(LineWidth)); base.LineWidth(DefaultLineWidth); }
    /// <summary>Layer: Restores <c>glPointSize</c> to <see cref="DefaultPointSize"/>. Must be paired with exactly one earlier call to <c>glPointSize</c>.</summary>
    public void ResetPointSize() { pointSize.Reset(nameof(PointSize)); base.PointSize(DefaultPointSize); }
    /// <summary>Layer: Restores <c>glProvokingVertex</c> to <see cref="DefaultProvokingVertex"/>. Must be paired with exactly one earlier call to <c>glProvokingVertex</c>.</summary>
    public void ResetProvokingVertex() { provokingVertex.Reset(nameof(ProvokingVertex)); base.ProvokingVertex(DefaultProvokingVertex); }
    /// <summary>Layer: Restores <c>glPrimitiveRestartIndex</c> to <see cref="DefaultPrimitiveRestartIndex"/>. Must be paired with exactly one earlier call to <c>glPrimitiveRestartIndex</c>.</summary>
    public void ResetPrimitiveRestartIndex() { primitiveRestartIndex.Reset(nameof(PrimitiveRestartIndex)); base.PrimitiveRestartIndex(DefaultPrimitiveRestartIndex); }
    /// <summary>Layer: Restores <c>glLogicOp</c> to <see cref="DefaultLogicOp"/>. Must be paired with exactly one earlier call to <c>glLogicOp</c>.</summary>
    public void ResetLogicOp() { logicOp.Reset(nameof(LogicOp)); base.LogicOp(DefaultLogicOp); }
    /// <summary>Layer: Restores <c>glViewport</c> to <see cref="DefaultViewport"/>. Must be paired with exactly one earlier call to <c>glViewport</c>.</summary>
    public void ResetViewport() { viewport.Reset(nameof(Viewport)); base.Viewport(DefaultViewport.X, DefaultViewport.Y, DefaultViewport.Width, DefaultViewport.Height); }
    /// <summary>Layer: Restores <c>glViewportIndexedf</c> for viewport <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glViewportIndexedf</c> for the same index.</summary>
    public void ResetViewportIndexed(uint index) { viewportMap.Reset(nameof(ViewportIndexedf), index); base.ViewportIndexedf(index, DefaultViewport.X, DefaultViewport.Y, DefaultViewport.Width, DefaultViewport.Height); }
    /// <summary>Layer: Restores <c>glViewportArrayv</c> for <paramref name="count"/> viewports from <paramref name="first"/>. Must be paired with exactly one earlier call to <c>glViewportArrayv</c> for the same range.</summary>
    public unsafe void ResetViewportArray(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            viewportMap.Reset(nameof(ViewportArrayv), first + (uint)i);
        Span<float> values = stackalloc float[count * 4];
        for (var i = 0; i < count; i++) { values[i * 4] = DefaultViewport.X; values[i * 4 + 1] = DefaultViewport.Y; values[i * 4 + 2] = DefaultViewport.Width; values[i * 4 + 3] = DefaultViewport.Height; }
        fixed (float* pointer = values)
            base.ViewportArrayv(first, count, (nint)pointer);
    }
    /// <summary>Layer: Restores <c>glScissor</c> to <see cref="DefaultScissor"/>. Must be paired with exactly one earlier call to <c>glScissor</c>.</summary>
    public void ResetScissor() { scissor.Reset(nameof(Scissor)); base.Scissor(DefaultScissor.X, DefaultScissor.Y, DefaultScissor.Width, DefaultScissor.Height); }
    /// <summary>Layer: Restores <c>glScissorIndexed</c> for viewport <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glScissorIndexed</c> for the same index.</summary>
    public void ResetScissorIndexed(uint index) { scissorMap.Reset(nameof(ScissorIndexed), index); base.ScissorIndexed(index, DefaultScissor.X, DefaultScissor.Y, DefaultScissor.Width, DefaultScissor.Height); }
    /// <summary>Layer: Restores <c>glScissorArrayv</c> for <paramref name="count"/> viewports from <paramref name="first"/>. Must be paired with exactly one earlier call to <c>glScissorArrayv</c> for the same range.</summary>
    public unsafe void ResetScissorArray(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            scissorMap.Reset(nameof(ScissorArrayv), first + (uint)i);
        Span<int> values = stackalloc int[count * 4];
        for (var i = 0; i < count; i++) { values[i * 4] = DefaultScissor.X; values[i * 4 + 1] = DefaultScissor.Y; values[i * 4 + 2] = DefaultScissor.Width; values[i * 4 + 3] = DefaultScissor.Height; }
        fixed (int* pointer = values)
            base.ScissorArrayv(first, count, (nint)pointer);
    }
    /// <summary>Layer: Restores <c>glActiveTexture</c> to <see cref="DefaultActiveTexture"/>. Must be paired with exactly one earlier call to <c>glActiveTexture</c>.</summary>
    public void ResetActiveTexture() { activeTexture.Reset(nameof(ActiveTexture)); base.ActiveTexture(DefaultActiveTexture); }
    /// <summary>Layer: Restores <c>glClipControl</c> to <see cref="DefaultClipControl"/>. Must be paired with exactly one earlier call to <c>glClipControl</c>.</summary>
    public void ResetClipControl() { clipControl.Reset(nameof(ClipControl)); base.ClipControl(DefaultClipControl.Origin, DefaultClipControl.Depth); }
    /// <summary>Layer: Restores <c>glHint</c> for <paramref name="target"/>. Must be paired with exactly one earlier call to <c>glHint</c> for the same target.</summary>
    public void ResetHint(HintTarget target) { hintMap.Reset(nameof(Hint), target); base.Hint(target, DefaultHint); }
    /// <summary>Layer: Restores <c>glMinSampleShading</c> to <see cref="DefaultMinSampleShading"/>. Must be paired with exactly one earlier call to <c>glMinSampleShading</c>.</summary>
    public void ResetMinSampleShading() { minSampleShading.Reset(nameof(MinSampleShading)); base.MinSampleShading(DefaultMinSampleShading); }
    /// <summary>Layer: Restores <c>glSampleCoverage</c> to <see cref="DefaultSampleCoverage"/>. Must be paired with exactly one earlier call to <c>glSampleCoverage</c>.</summary>
    public void ResetSampleCoverage() { sampleCoverage.Reset(nameof(SampleCoverage)); base.SampleCoverage(DefaultSampleCoverage.Value, DefaultSampleCoverage.Invert); }
    /// <summary>Layer: Restores <c>glSampleMaski</c> for word <paramref name="maskNumber"/>. Must be paired with exactly one earlier call to <c>glSampleMaski</c> for the same word.</summary>
    public void ResetSampleMask(uint maskNumber) { sampleMaskMap.Reset(nameof(SampleMaski), maskNumber); base.SampleMaski(maskNumber, DefaultSampleMask); }
    /// <summary>Layer: Restores <c>glPatchParameteri</c> for <paramref name="pname"/>. Must be paired with exactly one earlier call to <c>glPatchParameteri</c> for the same parameter.</summary>
    public void ResetPatchParameter(PatchParameterName pname) { patchParameterMap.Reset(nameof(PatchParameteri), pname); base.PatchParameteri(pname, DefaultPatchParameter(pname)); }
    /// <summary>Layer: Restores <c>glPointParameterf</c> for <paramref name="pname"/>. Must be paired with exactly one earlier call to <c>glPointParameterf</c> or <c>glPointParameteri</c> for the same parameter.</summary>
    public void ResetPointParameter(PointParameterName pname) { pointParameterMap.Reset(nameof(PointParameterf), pname); base.PointParameterf(pname, DefaultPointParameter(pname)); }
    /// <summary>Layer: Restores <c>glPixelStorei</c> for <paramref name="pname"/>. Must be paired with exactly one earlier call to <c>glPixelStorei</c> or <c>glPixelStoref</c> for the same parameter.</summary>
    public void ResetPixelStore(PixelStoreParameter pname) { pixelStoreMap.Reset(nameof(PixelStorei), pname); base.PixelStorei(pname, DefaultPixelStore(pname)); }
    /// <summary>Layer: Restores <c>glReadBuffer</c> to <see cref="DefaultReadBuffer"/>. Must be paired with exactly one earlier call to <c>glReadBuffer</c>.</summary>
    public void ResetReadBuffer() { readBuffer.Reset(nameof(ReadBuffer)); base.ReadBuffer(DefaultReadBuffer); }
}
