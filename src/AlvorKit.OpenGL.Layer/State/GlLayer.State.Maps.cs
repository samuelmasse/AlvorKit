namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Tracks enabled capabilities by capability.</summary>
    private readonly GlStateMap<GlEnableCap, bool> enableMap = new();

    /// <summary>Tracks indexed enabled capabilities by capability and index.</summary>
    private readonly GlStateMap<(GlEnableCap, uint), bool> indexedEnableMap = new();

    /// <summary>Tracks per-buffer blend function state.</summary>
    private readonly GlStateMap<uint, (GlBlendingFactor, GlBlendingFactor)> blendFuncMap = new();

    /// <summary>Tracks per-buffer separate blend function state.</summary>
    private readonly GlStateMap<uint, (GlBlendingFactor, GlBlendingFactor, GlBlendingFactor, GlBlendingFactor)> blendFuncSeparateMap = new();

    /// <summary>Tracks per-buffer separate blend equation state.</summary>
    private readonly GlStateMap<uint, (GlBlendEquationModeEXT, GlBlendEquationModeEXT)> blendEquationSeparateMap = new();

    /// <summary>Tracks per-buffer color mask state.</summary>
    private readonly GlStateMap<uint, (bool, bool, bool, bool)> colorMaskMap = new();

    /// <summary>Tracks indexed depth range state.</summary>
    private readonly GlStateMap<uint, (double, double)> depthRangeMap = new();

    /// <summary>Tracks per-face stencil function state.</summary>
    private readonly GlStateMap<GlTriangleFace, (GlStencilFunction, int, uint)> stencilFuncSeparateMap = new();

    /// <summary>Tracks per-face stencil mask state.</summary>
    private readonly GlStateMap<GlTriangleFace, uint> stencilMaskSeparateMap = new();

    /// <summary>Tracks per-face stencil operation state.</summary>
    private readonly GlStateMap<GlTriangleFace, (GlStencilOp, GlStencilOp, GlStencilOp)> stencilOpSeparateMap = new();

    /// <summary>Tracks indexed scissor box state.</summary>
    private readonly GlStateMap<uint, (int, int, int, int)> scissorMap = new();

    /// <summary>Tracks indexed viewport state.</summary>
    private readonly GlStateMap<uint, (float, float, float, float)> viewportMap = new();

    /// <summary>Tracks hint state by hint target.</summary>
    private readonly GlStateMap<GlHintTarget, GlHintMode> hintMap = new();

    /// <summary>Tracks sample mask state by mask word.</summary>
    private readonly GlStateMap<uint, uint> sampleMaskMap = new();

    /// <summary>Tracks patch parameter state by parameter name.</summary>
    private readonly GlStateMap<GlPatchParameterName, int> patchParameterMap = new();

    /// <summary>Tracks point parameter state by parameter name.</summary>
    private readonly GlStateMap<GlPointParameterName, float> pointParameterMap = new();

    /// <summary>Tracks pixel store state by parameter name.</summary>
    private readonly GlStateMap<GlPixelStoreParameter, double> pixelStoreMap = new();
}
