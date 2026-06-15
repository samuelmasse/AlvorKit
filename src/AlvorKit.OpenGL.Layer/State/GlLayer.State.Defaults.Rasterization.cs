namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Layer: The default value <see cref="ResetCullFace()"/> restores.</summary>
    public virtual GlTriangleFace DefaultCullFace => GlTriangleFace.Back;

    /// <summary>Layer: The default value <see cref="ResetFrontFace()"/> restores.</summary>
    public virtual GlFrontFaceDirection DefaultFrontFace => GlFrontFaceDirection.Ccw;

    /// <summary>Layer: The default value <see cref="ResetPolygonMode()"/> restores.</summary>
    public virtual (GlTriangleFace Face, GlPolygonMode Mode) DefaultPolygonMode => (GlTriangleFace.FrontAndBack, GlPolygonMode.Fill);

    /// <summary>Layer: The default value <see cref="ResetPolygonOffset()"/> restores.</summary>
    public virtual (float Factor, float Units) DefaultPolygonOffset => (0f, 0f);

    /// <summary>Layer: The default value <see cref="ResetPolygonOffsetClamp()"/> restores.</summary>
    public virtual (float Factor, float Units, float Clamp) DefaultPolygonOffsetClamp => (0f, 0f, 0f);

    /// <summary>Layer: The default value <see cref="ResetLineWidth()"/> restores.</summary>
    public virtual float DefaultLineWidth => 1f;

    /// <summary>Layer: The default value <see cref="ResetPointSize()"/> restores.</summary>
    public virtual float DefaultPointSize => 1f;

    /// <summary>Layer: The default value <see cref="ResetProvokingVertex()"/> restores.</summary>
    public virtual GlVertexProvokingMode DefaultProvokingVertex => GlVertexProvokingMode.LastVertexConvention;

    /// <summary>Layer: The default value <see cref="ResetPrimitiveRestartIndex()"/> restores.</summary>
    public virtual uint DefaultPrimitiveRestartIndex => 0u;

    /// <summary>Layer: The default value <see cref="ResetLogicOp()"/> restores.</summary>
    public virtual GlLogicOp DefaultLogicOp => GlLogicOp.Copy;

    /// <summary>Layer: The default value <see cref="ResetViewport()"/> restores.</summary>
    public virtual (int X, int Y, int Width, int Height) DefaultViewport => (0, 0, 0, 0);

    /// <summary>Layer: The default value <see cref="ResetScissor()"/> restores.</summary>
    public virtual (int X, int Y, int Width, int Height) DefaultScissor => (0, 0, 0, 0);
}
