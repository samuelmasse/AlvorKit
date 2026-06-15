namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Tracks the current viewport state.</summary>
    private GlStateSlot<(int, int, int, int)> viewport;

    /// <summary>Tracks the current scissor box state.</summary>
    private GlStateSlot<(int, int, int, int)> scissor;

    /// <summary>Tracks the current clear color state.</summary>
    private GlStateSlot<(float, float, float, float)> clearColor;

    /// <summary>Tracks the current blend color state.</summary>
    private GlStateSlot<(float, float, float, float)> blendColor;

    /// <summary>Tracks the current clear depth state.</summary>
    private GlStateSlot<double> clearDepth;

    /// <summary>Tracks the current clear stencil state.</summary>
    private GlStateSlot<int> clearStencil;

    /// <summary>Tracks the current depth function state.</summary>
    private GlStateSlot<GlDepthFunction> depthFunc;

    /// <summary>Tracks the current depth mask state.</summary>
    private GlStateSlot<bool> depthMask;

    /// <summary>Tracks the current cull face state.</summary>
    private GlStateSlot<GlTriangleFace> cullFace;

    /// <summary>Tracks the current front face winding state.</summary>
    private GlStateSlot<GlFrontFaceDirection> frontFace;

    /// <summary>Tracks the current polygon mode state.</summary>
    private GlStateSlot<(GlTriangleFace, GlPolygonMode)> polygonMode;

    /// <summary>Tracks the current polygon offset state.</summary>
    private GlStateSlot<(float, float)> polygonOffset;

    /// <summary>Tracks the current polygon offset clamp state.</summary>
    private GlStateSlot<(float, float, float)> polygonOffsetClamp;

    /// <summary>Tracks the current line width state.</summary>
    private GlStateSlot<float> lineWidth;

    /// <summary>Tracks the current point size state.</summary>
    private GlStateSlot<float> pointSize;

    /// <summary>Tracks the current provoking vertex state.</summary>
    private GlStateSlot<GlVertexProvokingMode> provokingVertex;

    /// <summary>Tracks the current primitive restart index state.</summary>
    private GlStateSlot<uint> primitiveRestartIndex;

    /// <summary>Tracks the current logical operation state.</summary>
    private GlStateSlot<GlLogicOp> logicOp;

    /// <summary>Tracks the current clip control state.</summary>
    private GlStateSlot<(GlClipControlOrigin, GlClipControlDepth)> clipControl;

    /// <summary>Tracks the current minimum sample shading state.</summary>
    private GlStateSlot<float> minSampleShading;

    /// <summary>Tracks the current sample coverage state.</summary>
    private GlStateSlot<(float, bool)> sampleCoverage;

    /// <summary>Tracks the current read buffer state.</summary>
    private GlStateSlot<GlReadBufferMode> readBuffer;

    /// <summary>Tracks the current clamp color state.</summary>
    private GlStateSlot<GlClampColorMode> clampColor;

    /// <summary>Tracks the current global blend function state.</summary>
    private GlStateSlot<(GlBlendingFactor, GlBlendingFactor)> blendFunc;

    /// <summary>Tracks the current global separate blend function state.</summary>
    private GlStateSlot<(GlBlendingFactor, GlBlendingFactor, GlBlendingFactor, GlBlendingFactor)> blendFuncSeparate;

    /// <summary>Tracks the current global blend equation state.</summary>
    private GlStateSlot<GlBlendEquationModeEXT> blendEquation;

    /// <summary>Tracks the current global separate blend equation state.</summary>
    private GlStateSlot<(GlBlendEquationModeEXT, GlBlendEquationModeEXT)> blendEquationSeparate;

    /// <summary>Tracks the current global color mask state.</summary>
    private GlStateSlot<(bool, bool, bool, bool)> colorMask;

    /// <summary>Tracks the current global depth range state.</summary>
    private GlStateSlot<(double, double)> depthRange;

    /// <summary>Tracks the current global stencil function state.</summary>
    private GlStateSlot<(GlStencilFunction, int, uint)> stencilFunc;

    /// <summary>Tracks the current global stencil mask state.</summary>
    private GlStateSlot<uint> stencilMask;

    /// <summary>Tracks the current global stencil operation state.</summary>
    private GlStateSlot<(GlStencilOp, GlStencilOp, GlStencilOp)> stencilOp;

    /// <summary>Tracks the current transform feedback active primitive mode.</summary>
    private GlStateSlot<GlPrimitiveType> transformFeedbackActive;
}
