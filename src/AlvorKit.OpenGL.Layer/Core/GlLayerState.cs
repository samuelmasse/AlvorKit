namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// Context-wide tracking state shared by every <see cref="GlLayer"/> node in one hierarchy:
/// strict binding slots, single-assignment state, and memory accounting model the single
/// underlying GL context, while each node keeps its own resource-ownership sets.
/// </summary>
internal sealed class GlLayerState
{
    /// <summary>Tracks the currently bound vertex array object.</summary>
    internal GlBinding vertexArray;

    /// <summary>Tracks the currently used program object.</summary>
    internal GlBinding program;

    /// <summary>Tracks the currently bound program pipeline object.</summary>
    internal GlBinding programPipeline;

    /// <summary>Tracks the currently bound renderbuffer object.</summary>
    internal GlBinding renderbuffer;

    /// <summary>Tracks the currently bound read framebuffer object.</summary>
    internal GlBinding readFramebuffer;

    /// <summary>Tracks the currently bound draw framebuffer object.</summary>
    internal GlBinding drawFramebuffer;

    /// <summary>Tracks the currently bound transform feedback object.</summary>
    internal GlBinding transformFeedbackObject;

    /// <summary>Tracks the currently active conditional render scope.</summary>
    internal GlBinding conditionalRender;

    /// <summary>Tracks the active texture unit set through <c>glActiveTexture</c>.</summary>
    internal GlStateSlot<GlTextureUnit> activeTexture;

    /// <summary>Tracks whether draw buffers are configured for the current framebuffer.</summary>
    internal GlStateSlot<int> drawBuffersCount;

    /// <summary>Tracks generic buffer bindings by target.</summary>
    internal readonly GlBindingMap<GlBufferTarget> bufferBinds = new();

    /// <summary>Tracks indexed buffer bindings by target and index.</summary>
    internal readonly GlBindingMap<(GlBufferTarget, uint)> indexedBufferBinds = new();

    /// <summary>Tracks vertex buffer bindings by binding index.</summary>
    internal readonly GlBindingMap<uint> vertexBufferBinds = new();

    /// <summary>Tracks sampler bindings by texture unit.</summary>
    internal readonly GlBindingMap<uint> samplerBinds = new();

    /// <summary>Tracks image texture bindings by image unit.</summary>
    internal readonly GlBindingMap<uint> imageTextureBinds = new();

    /// <summary>Tracks texture bindings by texture unit and target.</summary>
    internal readonly GlBindingMap<(uint, GlTextureTarget)> textureBinds = new();

    /// <summary>Tracks query scopes by target.</summary>
    internal readonly GlBindingMap<GlQueryTarget> queryBinds = new();

    /// <summary>Tracks indexed query scopes by target and index.</summary>
    internal readonly GlBindingMap<(GlQueryTarget, uint)> queryIndexedBinds = new();

    /// <summary>Tracks the target family first associated with each texture handle.</summary>
    internal readonly Dictionary<GlTextureHandle, GlTextureTarget> textureTargets = [];

    /// <summary>Tracks the current viewport state.</summary>
    internal GlStateSlot<(int, int, int, int)> viewport;

    /// <summary>Tracks the current scissor box state.</summary>
    internal GlStateSlot<(int, int, int, int)> scissor;

    /// <summary>Tracks the current clear color state.</summary>
    internal GlStateSlot<(float, float, float, float)> clearColor;

    /// <summary>Tracks the current blend color state.</summary>
    internal GlStateSlot<(float, float, float, float)> blendColor;

    /// <summary>Tracks the current clear depth state.</summary>
    internal GlStateSlot<double> clearDepth;

    /// <summary>Tracks the current clear stencil state.</summary>
    internal GlStateSlot<int> clearStencil;

    /// <summary>Tracks the current depth function state.</summary>
    internal GlStateSlot<GlDepthFunction> depthFunc;

    /// <summary>Tracks the current depth mask state.</summary>
    internal GlStateSlot<bool> depthMask;

    /// <summary>Tracks the current cull face state.</summary>
    internal GlStateSlot<GlTriangleFace> cullFace;

    /// <summary>Tracks the current front face winding state.</summary>
    internal GlStateSlot<GlFrontFaceDirection> frontFace;

    /// <summary>Tracks the current polygon mode state.</summary>
    internal GlStateSlot<(GlTriangleFace, GlPolygonMode)> polygonMode;

    /// <summary>Tracks the current polygon offset state.</summary>
    internal GlStateSlot<(float, float)> polygonOffset;

    /// <summary>Tracks the current polygon offset clamp state.</summary>
    internal GlStateSlot<(float, float, float)> polygonOffsetClamp;

    /// <summary>Tracks the current line width state.</summary>
    internal GlStateSlot<float> lineWidth;

    /// <summary>Tracks the current point size state.</summary>
    internal GlStateSlot<float> pointSize;

    /// <summary>Tracks the current provoking vertex state.</summary>
    internal GlStateSlot<GlVertexProvokingMode> provokingVertex;

    /// <summary>Tracks the current primitive restart index state.</summary>
    internal GlStateSlot<uint> primitiveRestartIndex;

    /// <summary>Tracks the current logical operation state.</summary>
    internal GlStateSlot<GlLogicOp> logicOp;

    /// <summary>Tracks the current clip control state.</summary>
    internal GlStateSlot<(GlClipControlOrigin, GlClipControlDepth)> clipControl;

    /// <summary>Tracks the current minimum sample shading state.</summary>
    internal GlStateSlot<float> minSampleShading;

    /// <summary>Tracks the current sample coverage state.</summary>
    internal GlStateSlot<(float, bool)> sampleCoverage;

    /// <summary>Tracks the current read buffer state.</summary>
    internal GlStateSlot<GlReadBufferMode> readBuffer;

    /// <summary>Tracks the current clamp color state.</summary>
    internal GlStateSlot<GlClampColorMode> clampColor;

    /// <summary>Tracks the current global blend function state.</summary>
    internal GlStateSlot<(GlBlendingFactor, GlBlendingFactor)> blendFunc;

    /// <summary>Tracks the current global separate blend function state.</summary>
    internal GlStateSlot<(GlBlendingFactor, GlBlendingFactor, GlBlendingFactor, GlBlendingFactor)> blendFuncSeparate;

    /// <summary>Tracks the current global blend equation state.</summary>
    internal GlStateSlot<GlBlendEquationModeEXT> blendEquation;

    /// <summary>Tracks the current global separate blend equation state.</summary>
    internal GlStateSlot<(GlBlendEquationModeEXT, GlBlendEquationModeEXT)> blendEquationSeparate;

    /// <summary>Tracks the current global color mask state.</summary>
    internal GlStateSlot<(bool, bool, bool, bool)> colorMask;

    /// <summary>Tracks the current global depth range state.</summary>
    internal GlStateSlot<(double, double)> depthRange;

    /// <summary>Tracks the current global stencil function state.</summary>
    internal GlStateSlot<(GlStencilFunction, int, uint)> stencilFunc;

    /// <summary>Tracks the current global stencil mask state.</summary>
    internal GlStateSlot<uint> stencilMask;

    /// <summary>Tracks the current global stencil operation state.</summary>
    internal GlStateSlot<(GlStencilOp, GlStencilOp, GlStencilOp)> stencilOp;

    /// <summary>Tracks the current transform feedback active primitive mode.</summary>
    internal GlStateSlot<GlPrimitiveType> transformFeedbackActive;

    /// <summary>Tracks enabled capabilities by capability.</summary>
    internal readonly GlStateMap<GlEnableCap, bool> enableMap = new();

    /// <summary>Tracks indexed enabled capabilities by capability and index.</summary>
    internal readonly GlStateMap<(GlEnableCap, uint), bool> indexedEnableMap = new();

    /// <summary>Tracks per-buffer blend function state.</summary>
    internal readonly GlStateMap<uint, (GlBlendingFactor, GlBlendingFactor)> blendFuncMap = new();

    /// <summary>Tracks per-buffer separate blend function state.</summary>
    internal readonly GlStateMap<uint, (GlBlendingFactor, GlBlendingFactor, GlBlendingFactor, GlBlendingFactor)> blendFuncSeparateMap = new();

    /// <summary>Tracks per-buffer separate blend equation state.</summary>
    internal readonly GlStateMap<uint, (GlBlendEquationModeEXT, GlBlendEquationModeEXT)> blendEquationSeparateMap = new();

    /// <summary>Tracks per-buffer color mask state.</summary>
    internal readonly GlStateMap<uint, (bool, bool, bool, bool)> colorMaskMap = new();

    /// <summary>Tracks indexed depth range state.</summary>
    internal readonly GlStateMap<uint, (double, double)> depthRangeMap = new();

    /// <summary>Tracks per-face stencil function state.</summary>
    internal readonly GlStateMap<GlTriangleFace, (GlStencilFunction, int, uint)> stencilFuncSeparateMap = new();

    /// <summary>Tracks per-face stencil mask state.</summary>
    internal readonly GlStateMap<GlTriangleFace, uint> stencilMaskSeparateMap = new();

    /// <summary>Tracks per-face stencil operation state.</summary>
    internal readonly GlStateMap<GlTriangleFace, (GlStencilOp, GlStencilOp, GlStencilOp)> stencilOpSeparateMap = new();

    /// <summary>Tracks indexed scissor box state.</summary>
    internal readonly GlStateMap<uint, (int, int, int, int)> scissorMap = new();

    /// <summary>Tracks indexed viewport state.</summary>
    internal readonly GlStateMap<uint, (float, float, float, float)> viewportMap = new();

    /// <summary>Tracks hint state by hint target.</summary>
    internal readonly GlStateMap<GlHintTarget, GlHintMode> hintMap = new();

    /// <summary>Tracks sample mask state by mask word.</summary>
    internal readonly GlStateMap<uint, uint> sampleMaskMap = new();

    /// <summary>Tracks patch parameter state by parameter name.</summary>
    internal readonly GlStateMap<GlPatchParameterName, int> patchParameterMap = new();

    /// <summary>Tracks point parameter state by parameter name.</summary>
    internal readonly GlStateMap<GlPointParameterName, float> pointParameterMap = new();

    /// <summary>Tracks pixel store state by parameter name.</summary>
    internal readonly GlStateMap<GlPixelStoreParameter, double> pixelStoreMap = new();

    /// <summary>Tracks the last recorded byte size for each live buffer.</summary>
    internal readonly Dictionary<GlBufferHandle, long> bufferSizes = [];

    /// <summary>Stores the total tracked bytes allocated to live buffers.</summary>
    internal long bufferUsage;

    /// <summary>Tracks the aggregate recorded texture shape for each live texture.</summary>
    internal readonly Dictionary<GlTextureHandle, GlTextureInfo> textureSizes = [];

    /// <summary>Tracks recorded texture storage by texture handle and mip level.</summary>
    internal readonly Dictionary<(GlTextureHandle Texture, int Level), GlTextureInfo> textureLevelSizes = [];

    /// <summary>Stores the total tracked bytes allocated to live textures.</summary>
    internal long textureUsage;

    /// <summary>Tracks the last recorded renderbuffer shape for each live renderbuffer.</summary>
    internal readonly Dictionary<GlRenderbufferHandle, GlRenderbufferInfo> renderbufferSizes = [];

    /// <summary>Stores the total tracked bytes allocated to live renderbuffers.</summary>
    internal long renderbufferUsage;
}
